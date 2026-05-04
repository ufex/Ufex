using System;
using System.Collections.Generic;
using System.IO;

namespace Ufex.Hex;

/// <summary>
/// Parses .ufexcolors files into ColorProfile instances.
/// </summary>
public static class ColorProfileParser
{
	/// <summary>
	/// Parses a .ufexcolors file from its text content.
	/// </summary>
	public static ColorProfile Parse(string content)
	{
		var profile = new ColorProfile();

		// Split header and body at '---' separator
		int separatorIndex = FindSeparator(content);
		if (separatorIndex == -1)
			throw new ColorProfileParseException("Missing '---' separator between header and rule body.");

		string header = content.Substring(0, separatorIndex);
		string body = content.Substring(separatorIndex);
		// Skip the separator line itself
		int bodyStart = body.IndexOf('\n');
		if (bodyStart == -1)
			body = "";
		else
			body = body.Substring(bodyStart + 1);

		ParseHeader(header, profile);
		ParseBody(body, profile);

		if (profile.Rules.Count == 0)
			throw new ColorProfileParseException("Profile must contain at least one rule arm.");

		return profile;
	}

	/// <summary>
	/// Parses a .ufexcolors file from a file path.
	/// </summary>
	public static ColorProfile ParseFile(string filePath)
	{
		string content = File.ReadAllText(filePath);
		return Parse(content);
	}

	private static int FindSeparator(string content)
	{
		// Find a line that is exactly "---" (with optional trailing whitespace)
		int i = 0;
		while (i < content.Length)
		{
			int lineStart = i;
			int lineEnd = content.IndexOf('\n', i);
			if (lineEnd == -1)
				lineEnd = content.Length;

			string line = content.Substring(lineStart, lineEnd - lineStart).TrimEnd('\r', ' ', '\t');
			if (line == "---")
				return lineStart;

			i = lineEnd + 1;
		}
		return -1;
	}

	private static void ParseHeader(string header, ColorProfile profile)
	{
		var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var lines = header.Split('\n');

		foreach (string rawLine in lines)
		{
			string line = rawLine.TrimEnd('\r');
			string trimmed = line.TrimStart();

			// Skip blank lines
			if (string.IsNullOrWhiteSpace(trimmed))
				continue;

			// Skip comments
			if (trimmed.StartsWith("#"))
				continue;

			int colonIndex = line.IndexOf(':');
			if (colonIndex == -1)
				throw new ColorProfileParseException($"Invalid header line (missing colon): '{line}'");

			string key = line.Substring(0, colonIndex).Trim();
			string value = line.Substring(colonIndex + 1).Trim();

			string keyLower = key.ToLowerInvariant();

			if (!IsValidHeaderKey(keyLower))
				throw new ColorProfileParseException($"Unknown header key: '{key}'");

			if (seenKeys.Contains(keyLower))
				throw new ColorProfileParseException($"Duplicate header key: '{key}'");

			seenKeys.Add(keyLower);

			switch (keyLower)
			{
				case "id":
					ValidateId(value);
					profile.ID = value;
					break;
				case "name":
					profile.Name = value;
					break;
				case "description":
					profile.Description = value;
					break;
				case "filetypes":
					if (string.IsNullOrWhiteSpace(value))
						throw new ColorProfileParseException("FileTypes field is present but empty. Omit the field for a global profile.");
					ParseFileTypes(value, profile);
					break;
			}
		}

		if (!seenKeys.Contains("id"))
			throw new ColorProfileParseException("Required header field 'ID' is missing.");
		if (!seenKeys.Contains("name"))
			throw new ColorProfileParseException("Required header field 'Name' is missing.");
	}

	private static bool IsValidHeaderKey(string keyLower)
	{
		return keyLower == "id" || keyLower == "name" || keyLower == "description" || keyLower == "filetypes";
	}

	private static void ValidateId(string id)
	{
		if (string.IsNullOrEmpty(id))
			throw new ColorProfileParseException("ID field cannot be empty.");

		char first = id[0];
		if (!((first >= 'a' && first <= 'z') || (first >= '0' && first <= '9')))
			throw new ColorProfileParseException($"ID must start with a lowercase letter or digit, got '{first}'.");

		for (int i = 1; i < id.Length; i++)
		{
			char c = id[i];
			if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-'))
				throw new ColorProfileParseException($"ID contains invalid character '{c}'. Only lowercase letters, digits, underscores, and hyphens are allowed.");
		}
	}

	private static void ParseFileTypes(string value, ColorProfile profile)
	{
		string[] parts = value.Split(',');
		foreach (string part in parts)
		{
			string pattern = part.Trim();
			if (string.IsNullOrEmpty(pattern))
				continue;

			// Validate: wildcard only allowed at end
			int starIndex = pattern.IndexOf('*');
			if (starIndex != -1 && starIndex != pattern.Length - 1)
				throw new ColorProfileParseException($"Wildcard '*' is only allowed at the end of a FileTypes pattern: '{pattern}'");

			profile.FileTypePatterns.Add(pattern);
		}
	}

	private static void ParseBody(string body, ColorProfile profile)
	{
		var lexer = new Lexer(body);
		var tokens = lexer.Tokenize();
		var parser = new RuleParser(tokens);
		profile.Rules = parser.ParseRules();
	}
}

/// <summary>
/// Recursive-descent parser for the rule body DSL.
/// </summary>
internal class RuleParser
{
	private readonly List<Token> tokens;
	private int pos;

	public RuleParser(List<Token> tokens)
	{
		this.tokens = tokens;
		this.pos = 0;
	}

	public List<ColorRule> ParseRules()
	{
		var rules = new List<ColorRule>();

		while (Current().Kind != TokenKind.Eof)
		{
			var expr = ParseExpr();
			Expect(TokenKind.Arrow, "Expected '=>' after rule predicate");
			rules.Add(MakeColorRule(expr));
		}

		return rules;
	}

	// expr := or_expr
	private Expr ParseExpr()
	{
		return ParseOrExpr();
	}

	// or_expr := and_expr { "||" and_expr }
	private Expr ParseOrExpr()
	{
		var left = ParseAndExpr();
		while (Current().Kind == TokenKind.OrOr)
		{
			Advance();
			var right = ParseAndExpr();
			left = new BinaryExpr(left, BinaryOp.Or, right);
		}
		return left;
	}

	// and_expr := not_expr { "&&" not_expr }
	private Expr ParseAndExpr()
	{
		var left = ParseNotExpr();
		while (Current().Kind == TokenKind.AndAnd)
		{
			Advance();
			var right = ParseNotExpr();
			left = new BinaryExpr(left, BinaryOp.And, right);
		}
		return left;
	}

	// not_expr := "!" not_expr | cmp_expr
	private Expr ParseNotExpr()
	{
		if (Current().Kind == TokenKind.Bang)
		{
			Advance();
			var operand = ParseNotExpr();
			return new UnaryExpr(UnaryOp.Not, operand);
		}
		return ParseCmpExpr();
	}

	// cmp_expr := bit_expr [ cmp_op bit_expr ]
	private Expr ParseCmpExpr()
	{
		var left = ParseBitExpr();

		var cmpOp = TryParseCmpOp();
		if (cmpOp.HasValue)
		{
			Advance();
			var right = ParseBitExpr();
			left = new BinaryExpr(left, cmpOp.Value, right);
		}

		return left;
	}

	private BinaryOp? TryParseCmpOp()
	{
		switch (Current().Kind)
		{
			case TokenKind.EqualEqual: return BinaryOp.Equal;
			case TokenKind.BangEqual: return BinaryOp.NotEqual;
			case TokenKind.Less: return BinaryOp.LessThan;
			case TokenKind.LessEqual: return BinaryOp.LessEqual;
			case TokenKind.Greater: return BinaryOp.GreaterThan;
			case TokenKind.GreaterEqual: return BinaryOp.GreaterEqual;
			default: return null;
		}
	}

	// bit_expr := shift_expr { ("&" | "|" | "^") shift_expr }
	private Expr ParseBitExpr()
	{
		var left = ParseShiftExpr();

		while (true)
		{
			BinaryOp op;
			switch (Current().Kind)
			{
				case TokenKind.Ampersand: op = BinaryOp.BitwiseAnd; break;
				case TokenKind.Pipe: op = BinaryOp.BitwiseOr; break;
				case TokenKind.Caret: op = BinaryOp.BitwiseXor; break;
				default: return left;
			}
			Advance();
			var right = ParseShiftExpr();
			left = new BinaryExpr(left, op, right);
		}
	}

	// shift_expr := unary_expr { ("<<" | ">>") unary_expr }
	private Expr ParseShiftExpr()
	{
		var left = ParseUnaryExpr();

		while (true)
		{
			BinaryOp op;
			switch (Current().Kind)
			{
				case TokenKind.ShiftLeft: op = BinaryOp.ShiftLeft; break;
				case TokenKind.ShiftRight: op = BinaryOp.ShiftRight; break;
				default: return left;
			}
			Advance();
			var right = ParseUnaryExpr();
			left = new BinaryExpr(left, op, right);
		}
	}

	// unary_expr := "~" unary_expr | "-" unary_expr | primary
	private Expr ParseUnaryExpr()
	{
		if (Current().Kind == TokenKind.Tilde)
		{
			Advance();
			var operand = ParseUnaryExpr();
			return new UnaryExpr(UnaryOp.BitwiseNot, operand);
		}
		if (Current().Kind == TokenKind.Minus)
		{
			Advance();
			var operand = ParseUnaryExpr();
			return new UnaryExpr(UnaryOp.Negate, operand);
		}
		return ParsePrimary();
	}

	// primary := integer | call | "(" expr ")" | "_"
	private Expr ParsePrimary()
	{
		var token = Current();

		switch (token.Kind)
		{
			case TokenKind.Integer:
				Advance();
				return new IntegerExpr(token.IntValue);

			case TokenKind.Underscore:
				Advance();
				return new WildcardExpr();

			case TokenKind.Identifier:
				return ParseIdentifierOrCall();

			case TokenKind.LParen:
				Advance();
				var expr = ParseExpr();
				Expect(TokenKind.RParen, "Expected ')' after expression");
				return expr;

			default:
				throw new ColorProfileParseException($"Unexpected token '{token.Kind}' at line {token.Line}");
		}
	}

	private Expr ParseIdentifierOrCall()
	{
		var token = Current();
		string name = token.StringValue!;
		Advance();

		// 'b' is a nullary built-in for current byte
		if (name == "b")
			return new CurrentByteExpr();

		// 'at' requires parenthesized argument
		if (name == "at")
		{
			Expect(TokenKind.LParen, "Expected '(' after 'at'");
			var arg = ParseExpr();
			Expect(TokenKind.RParen, "Expected ')' after 'at' argument");
			return new AtExpr(arg);
		}

		throw new ColorProfileParseException($"Unknown identifier '{name}' at line {token.Line}");
	}

	private Token Current()
	{
		return pos < tokens.Count ? tokens[pos] : tokens[tokens.Count - 1];
	}

	private void Advance()
	{
		if (pos < tokens.Count)
			pos++;
	}

	private void Expect(TokenKind kind, string errorMessage)
	{
		if (Current().Kind != kind)
			throw new ColorProfileParseException($"{errorMessage} at line {Current().Line} (got {Current().Kind})");
		Advance();
	}

	private ColorRule MakeColorRule(Expr predicate)
	{
		if (Current().Kind != TokenKind.Color)
			throw new ColorProfileParseException($"Expected color literal after '=>' at line {Current().Line}");

		var token = Current();
		Advance();

		if (token.HasDarkColor)
			return new ColorRule(predicate, token.ColorValue, token.DarkColorValue);
		else
			return new ColorRule(predicate, token.ColorValue);
	}
}
