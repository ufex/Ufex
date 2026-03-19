using System;
using System.Collections.Generic;
using System.IO;
using Ufex.API;

namespace Ufex.FileType.Config;

public enum OffsetReadType
{
	UInt8,
	Int8,
	UInt16Le,
	UInt16Be,
	Int16Le,
	Int16Be,
	UInt32Le,
	UInt32Be,
	Int32Le,
	Int32Be,
	UInt64Le,
	UInt64Be,
	Int64Le,
	Int64Be,
}

public sealed class OffsetExpression
{
	private readonly INode root;
	private readonly long? constantValue;

	public string RawExpression { get; }

	public bool IsConstant
	{
		get { return constantValue.HasValue; }
	}

	public long ConstantValue
	{
		get
		{
			if (!constantValue.HasValue)
			{
				throw new InvalidOperationException("Expression is not constant.");
			}
			return constantValue.Value;
		}
	}

	private OffsetExpression(string rawExpression, INode rootNode, long? constant)
	{
		RawExpression = rawExpression;
		root = rootNode;
		constantValue = constant;
	}

	public static OffsetExpression Parse(string expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException(nameof(expression));
		}

		string trimmed = expression.Trim();
		if (trimmed.Length == 0)
		{
			throw new FormatException("Offset expression cannot be empty.");
		}

		if (TryParseIntegerLiteral(trimmed, out long literalValue))
		{
			if (literalValue < 0)
			{
				throw new FormatException("Offset expression cannot be a negative constant.");
			}
			return new OffsetExpression(trimmed, new LiteralNode(literalValue), literalValue);
		}

		List<Token> tokens = Tokenize(trimmed);
		OffsetExpressionParser parser = new OffsetExpressionParser(tokens);
		INode root = parser.ParseExpression();
		parser.Expect(TokenKind.End);

		return new OffsetExpression(trimmed, root, null);
	}

	public bool TryEvaluate(byte[] buffer, FileStream fileStream, out long value)
	{
		value = 0;

		if (buffer == null)
		{
			throw new ArgumentNullException(nameof(buffer));
		}

		if (fileStream == null)
		{
			throw new ArgumentNullException(nameof(fileStream));
		}

		if (constantValue.HasValue)
		{
			value = constantValue.Value;
			return true;
		}

		EvaluationContext ctx = new EvaluationContext(buffer, fileStream);
		if (!root.TryEvaluate(ctx, out long result))
		{
			return false;
		}

		if (result < 0)
		{
			return false;
		}

		value = result;
		return true;
	}

	public long Evaluate(byte[] buffer, FileStream fileStream)
	{
		if (TryEvaluate(buffer, fileStream, out long value))
		{
			return value;
		}

		throw new InvalidOperationException("Offset expression evaluation failed.");
	}

	private static bool TryParseIntegerLiteral(string input, out long value)
	{
		value = 0;
		if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			if (input.Length <= 2)
			{
				return false;
			}

			if (!UInt64.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ulong tmp))
			{
				return false;
			}

			if (tmp > Int64.MaxValue)
			{
				return false;
			}

			value = (long)tmp;
			return true;
		}

		return Int64.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value);
	}

	private static List<Token> Tokenize(string expression)
	{
		List<Token> tokens = new List<Token>();
		int i = 0;

		while (i < expression.Length)
		{
			char c = expression[i];

			if (Char.IsWhiteSpace(c))
			{
				i++;
				continue;
			}

			switch (c)
			{
				case '+':
					tokens.Add(new Token(TokenKind.Plus, "+"));
					i++;
					continue;
				case '-':
					tokens.Add(new Token(TokenKind.Minus, "-"));
					i++;
					continue;
				case '*':
					tokens.Add(new Token(TokenKind.Star, "*"));
					i++;
					continue;
				case '/':
					tokens.Add(new Token(TokenKind.Slash, "/"));
					i++;
					continue;
				case '(':
					tokens.Add(new Token(TokenKind.LeftParen, "("));
					i++;
					continue;
				case ')':
					tokens.Add(new Token(TokenKind.RightParen, ")"));
					i++;
					continue;
				case '@':
					tokens.Add(new Token(TokenKind.At, "@"));
					i++;
					continue;
				case ':':
					tokens.Add(new Token(TokenKind.Colon, ":"));
					i++;
					continue;
			}

			if (Char.IsDigit(c))
			{
				int start = i;
				if (c == '0' && i + 1 < expression.Length && (expression[i + 1] == 'x' || expression[i + 1] == 'X'))
				{
					i += 2;
					int hexStart = i;
					while (i < expression.Length && IsHexDigit(expression[i]))
					{
						i++;
					}
					if (i == hexStart)
					{
						throw new FormatException($"Invalid hex literal in offset expression near index {start}.");
					}
				}
				else
				{
					while (i < expression.Length && Char.IsDigit(expression[i]))
					{
						i++;
					}
				}

				tokens.Add(new Token(TokenKind.IntegerLiteral, expression.Substring(start, i - start)));
				continue;
			}

			if (Char.IsLetter(c))
			{
				int start = i;
				i++;
				while (i < expression.Length && (Char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
				{
					i++;
				}

				tokens.Add(new Token(TokenKind.TypeName, expression.Substring(start, i - start)));
				continue;
			}

			throw new FormatException($"Invalid character '{c}' in offset expression at index {i}.");
		}

		tokens.Add(new Token(TokenKind.End, String.Empty));
		return tokens;
	}

	private static bool IsHexDigit(char c)
	{
		return (c >= '0' && c <= '9')
			|| (c >= 'a' && c <= 'f')
			|| (c >= 'A' && c <= 'F');
	}

	private sealed class OffsetExpressionParser
	{
		private readonly List<Token> tokens;
		private int index;

		public OffsetExpressionParser(List<Token> tokens)
		{
			this.tokens = tokens;
			index = 0;
		}

		public INode ParseExpression()
		{
			return ParseAdditive();
		}

		public void Expect(TokenKind kind)
		{
			Token current = Current;
			if (current.Kind != kind)
			{
				throw new FormatException($"Expected token {kind} but got {current.Kind} ('{current.Text}').");
			}
			index++;
		}

		private INode ParseAdditive()
		{
			INode node = ParseMultiplicative();
			while (Current.Kind == TokenKind.Plus || Current.Kind == TokenKind.Minus)
			{
				Token op = Current;
				index++;
				INode right = ParseMultiplicative();
				node = new BinaryNode(node, op.Kind, right);
			}
			return node;
		}

		private INode ParseMultiplicative()
		{
			INode node = ParseUnary();
			while (Current.Kind == TokenKind.Star || Current.Kind == TokenKind.Slash)
			{
				Token op = Current;
				index++;
				INode right = ParseUnary();
				node = new BinaryNode(node, op.Kind, right);
			}
			return node;
		}

		private INode ParseUnary()
		{
			if (Current.Kind == TokenKind.Minus)
			{
				index++;
				INode child = ParseUnary();
				return new UnaryNegateNode(child);
			}
			return ParsePrimary();
		}

		private INode ParsePrimary()
		{
			if (Current.Kind == TokenKind.IntegerLiteral)
			{
				Token literal = Current;
				index++;
				if (!TryParseIntegerLiteral(literal.Text, out long value))
				{
					throw new FormatException($"Invalid integer literal: {literal.Text}");
				}
				return new LiteralNode(value);
			}

			if (Current.Kind == TokenKind.At)
			{
				return ParseFileRead();
			}

			if (Current.Kind == TokenKind.LeftParen)
			{
				index++;
				INode node = ParseExpression();
				Expect(TokenKind.RightParen);
				return node;
			}

			throw new FormatException($"Unexpected token {Current.Kind} ('{Current.Text}') in offset expression.");
		}

		private INode ParseFileRead()
		{
			Expect(TokenKind.At);

			INode offsetExpr;
			if (Current.Kind == TokenKind.IntegerLiteral)
			{
				Token literal = Current;
				index++;
				if (!TryParseIntegerLiteral(literal.Text, out long value))
				{
					throw new FormatException($"Invalid file read offset literal: {literal.Text}");
				}
				offsetExpr = new LiteralNode(value);
			}
			else if (Current.Kind == TokenKind.LeftParen)
			{
				index++;
				offsetExpr = ParseExpression();
				Expect(TokenKind.RightParen);
			}
			else
			{
				throw new FormatException("Expected integer literal or parenthesized expression after '@'.");
			}

			Expect(TokenKind.Colon);

			if (Current.Kind != TokenKind.TypeName)
			{
				throw new FormatException("Expected type name in file read expression.");
			}

			OffsetReadType readType = ParseOffsetReadType(Current.Text);
			index++;

			return new FileReadNode(offsetExpr, readType);
		}

		private Token Current
		{
			get { return tokens[index]; }
		}

		private static OffsetReadType ParseOffsetReadType(string typeName)
		{
			switch (typeName.ToLowerInvariant())
			{
				case "uint8":
					return OffsetReadType.UInt8;
				case "int8":
					return OffsetReadType.Int8;
				case "uint16le":
					return OffsetReadType.UInt16Le;
				case "uint16be":
					return OffsetReadType.UInt16Be;
				case "int16le":
					return OffsetReadType.Int16Le;
				case "int16be":
					return OffsetReadType.Int16Be;
				case "uint32le":
					return OffsetReadType.UInt32Le;
				case "uint32be":
					return OffsetReadType.UInt32Be;
				case "int32le":
					return OffsetReadType.Int32Le;
				case "int32be":
					return OffsetReadType.Int32Be;
				case "uint64le":
					return OffsetReadType.UInt64Le;
				case "uint64be":
					return OffsetReadType.UInt64Be;
				case "int64le":
					return OffsetReadType.Int64Le;
				case "int64be":
					return OffsetReadType.Int64Be;
				default:
					throw new FormatException($"Unsupported offset read type: {typeName}");
			}
		}
	}

	private interface INode
	{
		bool TryEvaluate(EvaluationContext ctx, out long value);
	}

	private sealed class LiteralNode : INode
	{
		private readonly long value;

		public LiteralNode(long value)
		{
			this.value = value;
		}

		public bool TryEvaluate(EvaluationContext ctx, out long value)
		{
			value = this.value;
			return true;
		}
	}

	private sealed class UnaryNegateNode : INode
	{
		private readonly INode child;

		public UnaryNegateNode(INode child)
		{
			this.child = child;
		}

		public bool TryEvaluate(EvaluationContext ctx, out long value)
		{
			value = 0;
			if (!child.TryEvaluate(ctx, out long inner))
			{
				return false;
			}

			try
			{
				value = checked(-inner);
				return true;
			}
			catch (OverflowException)
			{
				return false;
			}
		}
	}

	private sealed class BinaryNode : INode
	{
		private readonly INode left;
		private readonly TokenKind op;
		private readonly INode right;

		public BinaryNode(INode left, TokenKind op, INode right)
		{
			this.left = left;
			this.op = op;
			this.right = right;
		}

		public bool TryEvaluate(EvaluationContext ctx, out long value)
		{
			value = 0;
			if (!left.TryEvaluate(ctx, out long leftValue))
			{
				return false;
			}

			if (!right.TryEvaluate(ctx, out long rightValue))
			{
				return false;
			}

			try
			{
				switch (op)
				{
					case TokenKind.Plus:
						value = checked(leftValue + rightValue);
						return true;
					case TokenKind.Minus:
						value = checked(leftValue - rightValue);
						return true;
					case TokenKind.Star:
						value = checked(leftValue * rightValue);
						return true;
					case TokenKind.Slash:
						if (rightValue == 0)
						{
							return false;
						}
						value = leftValue / rightValue;
						return true;
					default:
						return false;
				}
			}
			catch (OverflowException)
			{
				return false;
			}
		}
	}

	private sealed class FileReadNode : INode
	{
		private readonly INode offsetExpression;
		private readonly OffsetReadType readType;

		public FileReadNode(INode offsetExpression, OffsetReadType readType)
		{
			this.offsetExpression = offsetExpression;
			this.readType = readType;
		}

		public bool TryEvaluate(EvaluationContext ctx, out long value)
		{
			value = 0;
			if (!offsetExpression.TryEvaluate(ctx, out long offset))
			{
				return false;
			}

			if (offset < 0)
			{
				return false;
			}

			return ctx.TryReadValue(offset, readType, out value);
		}
	}

	private sealed class EvaluationContext
	{
		private readonly byte[] buffer;
		private readonly FileStream fileStream;

		public EvaluationContext(byte[] buffer, FileStream fileStream)
		{
			this.buffer = buffer;
			this.fileStream = fileStream;
		}

		public bool TryReadValue(long offset, OffsetReadType type, out long value)
		{
			value = 0;
			int size = GetSize(type);
			if (!TryReadBytes(offset, size, out byte[] bytes))
			{
				return false;
			}

			switch (type)
			{
				case OffsetReadType.UInt8:
					value = bytes[0];
					return true;
				case OffsetReadType.Int8:
					value = unchecked((sbyte)bytes[0]);
					return true;
				case OffsetReadType.UInt16Le:
					value = ByteUtil.BytesToUInt16(bytes, Endian.Little, 0);
					return true;
				case OffsetReadType.UInt16Be:
					value = ByteUtil.BytesToUInt16(bytes, Endian.Big, 0);
					return true;
				case OffsetReadType.Int16Le:
					value = ByteUtil.BytesToInt16(bytes, Endian.Little, 0);
					return true;
				case OffsetReadType.Int16Be:
					value = ByteUtil.BytesToInt16(bytes, Endian.Big, 0);
					return true;
				case OffsetReadType.UInt32Le:
					value = ByteUtil.BytesToUInt32(bytes, Endian.Little, 0);
					return true;
				case OffsetReadType.UInt32Be:
					value = ByteUtil.BytesToUInt32(bytes, Endian.Big, 0);
					return true;
				case OffsetReadType.Int32Le:
					value = ByteUtil.BytesToInt32(bytes, Endian.Little, 0);
					return true;
				case OffsetReadType.Int32Be:
					value = ByteUtil.BytesToInt32(bytes, Endian.Big, 0);
					return true;
				case OffsetReadType.UInt64Le:
					{
						ulong tmp = ByteUtil.BytesToUInt64(bytes, Endian.Little, 0);
						if (tmp > Int64.MaxValue)
						{
							return false;
						}
						value = (long)tmp;
						return true;
					}
				case OffsetReadType.UInt64Be:
					{
						ulong tmp = ByteUtil.BytesToUInt64(bytes, Endian.Big, 0);
						if (tmp > Int64.MaxValue)
						{
							return false;
						}
						value = (long)tmp;
						return true;
					}
				case OffsetReadType.Int64Le:
					value = unchecked((long)ByteUtil.BytesToUInt64(bytes, Endian.Little, 0));
					return true;
				case OffsetReadType.Int64Be:
					value = unchecked((long)ByteUtil.BytesToUInt64(bytes, Endian.Big, 0));
					return true;
				default:
					return false;
			}
		}

		private bool TryReadBytes(long offset, int count, out byte[] bytes)
		{
			bytes = Array.Empty<byte>();
			if (offset < 0 || count < 0)
			{
				return false;
			}

			long endOffset;
			try
			{
				endOffset = checked(offset + count);
			}
			catch (OverflowException)
			{
				return false;
			}

			if (endOffset > fileStream.Length)
			{
				return false;
			}

			bytes = new byte[count];

			if (endOffset <= buffer.Length)
			{
				Buffer.BlockCopy(buffer, (int)offset, bytes, 0, count);
				return true;
			}

			if (!fileStream.CanSeek)
			{
				return false;
			}

			long originalPosition = fileStream.Position;
			try
			{
				fileStream.Seek(offset, SeekOrigin.Begin);
				int totalRead = 0;
				while (totalRead < count)
				{
					int read = fileStream.Read(bytes, totalRead, count - totalRead);
					if (read <= 0)
					{
						return false;
					}
					totalRead += read;
				}
				return true;
			}
			finally
			{
				fileStream.Seek(originalPosition, SeekOrigin.Begin);
			}
		}

		private static int GetSize(OffsetReadType type)
		{
			switch (type)
			{
				case OffsetReadType.UInt8:
				case OffsetReadType.Int8:
					return 1;
				case OffsetReadType.UInt16Le:
				case OffsetReadType.UInt16Be:
				case OffsetReadType.Int16Le:
				case OffsetReadType.Int16Be:
					return 2;
				case OffsetReadType.UInt32Le:
				case OffsetReadType.UInt32Be:
				case OffsetReadType.Int32Le:
				case OffsetReadType.Int32Be:
					return 4;
				case OffsetReadType.UInt64Le:
				case OffsetReadType.UInt64Be:
				case OffsetReadType.Int64Le:
				case OffsetReadType.Int64Be:
					return 8;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported read type.");
			}
		}
	}

	private readonly struct Token
	{
		public TokenKind Kind { get; }
		public string Text { get; }

		public Token(TokenKind kind, string text)
		{
			Kind = kind;
			Text = text;
		}
	}

	private enum TokenKind
	{
		IntegerLiteral,
		TypeName,
		Plus,
		Minus,
		Star,
		Slash,
		LeftParen,
		RightParen,
		At,
		Colon,
		End,
	}
}