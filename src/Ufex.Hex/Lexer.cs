using System;
using System.Collections.Generic;

namespace Ufex.Hex;

internal enum TokenKind
{
	// Literals
	Integer,
	Color,
	// Identifiers
	Identifier,
	// Operators
	Arrow,        // =>
	LParen,
	RParen,
	Comma,
	AndAnd,       // &&
	OrOr,         // ||
	Bang,         // !
	EqualEqual,   // ==
	BangEqual,    // !=
	Less,         // <
	LessEqual,    // <=
	Greater,      // >
	GreaterEqual, // >=
	Ampersand,    // &
	Pipe,         // |
	Caret,        // ^
	Tilde,        // ~
	ShiftLeft,    // <<
	ShiftRight,   // >>
	Minus,        // -
	// Special
	Underscore,   // _
	Eof,
}

internal struct Token
{
	public TokenKind Kind;
	public Int64 IntValue;
	public UInt32 ColorValue;
	public UInt32 DarkColorValue;
	public bool HasDarkColor;
	public string? StringValue;
	public int Line;

	public Token(TokenKind kind, int line)
	{
		Kind = kind;
		IntValue = 0;
		ColorValue = 0;
		DarkColorValue = 0;
		HasDarkColor = false;
		StringValue = null;
		Line = line;
	}
}

internal class Lexer
{
	private readonly string source;
	private int pos;
	private int line;

	public Lexer(string source)
	{
		this.source = source;
		this.pos = 0;
		this.line = 1;
	}

	public List<Token> Tokenize()
	{
		var tokens = new List<Token>();
		while (true)
		{
			var token = NextToken();
			tokens.Add(token);
			if (token.Kind == TokenKind.Eof)
				break;
		}
		return tokens;
	}

	private Token NextToken()
	{
		SkipWhitespaceAndComments();

		if (pos >= source.Length)
			return new Token(TokenKind.Eof, line);

		char c = source[pos];

		// Color literal
		if (c == '#' && pos + 1 < source.Length && IsHexDigit(source[pos + 1]))
		{
			return ReadColor();
		}

		// Number literals
		if (c >= '0' && c <= '9')
		{
			return ReadNumber();
		}

		// Identifiers and keywords
		if (c >= 'a' && c <= 'z' || c == '_')
		{
			return ReadIdentifier();
		}

		// Two-char operators
		if (c == '=' && Peek(1) == '>')
		{
			pos += 2;
			return new Token(TokenKind.Arrow, line);
		}
		if (c == '=' && Peek(1) == '=')
		{
			pos += 2;
			return new Token(TokenKind.EqualEqual, line);
		}
		if (c == '!' && Peek(1) == '=')
		{
			pos += 2;
			return new Token(TokenKind.BangEqual, line);
		}
		if (c == '&' && Peek(1) == '&')
		{
			pos += 2;
			return new Token(TokenKind.AndAnd, line);
		}
		if (c == '|' && Peek(1) == '|')
		{
			pos += 2;
			return new Token(TokenKind.OrOr, line);
		}
		if (c == '<' && Peek(1) == '=')
		{
			pos += 2;
			return new Token(TokenKind.LessEqual, line);
		}
		if (c == '>' && Peek(1) == '=')
		{
			pos += 2;
			return new Token(TokenKind.GreaterEqual, line);
		}
		if (c == '<' && Peek(1) == '<')
		{
			pos += 2;
			return new Token(TokenKind.ShiftLeft, line);
		}
		if (c == '>' && Peek(1) == '>')
		{
			pos += 2;
			return new Token(TokenKind.ShiftRight, line);
		}

		// Single-char operators
		pos++;
		switch (c)
		{
			case '(': return new Token(TokenKind.LParen, line);
			case ')': return new Token(TokenKind.RParen, line);
			case ',': return new Token(TokenKind.Comma, line);
			case '!': return new Token(TokenKind.Bang, line);
			case '<': return new Token(TokenKind.Less, line);
			case '>': return new Token(TokenKind.Greater, line);
			case '&': return new Token(TokenKind.Ampersand, line);
			case '|': return new Token(TokenKind.Pipe, line);
			case '^': return new Token(TokenKind.Caret, line);
			case '~': return new Token(TokenKind.Tilde, line);
			case '-': return new Token(TokenKind.Minus, line);
			default:
				throw new ColorProfileParseException($"Unexpected character '{c}' at line {line}");
		}
	}

	private Token ReadColor()
	{
		int startLine = line;
		pos++; // skip '#'
		int start = pos;
		while (pos < source.Length && IsHexDigit(source[pos]))
			pos++;

		string hex = source.Substring(start, pos - start);
		UInt32 color = ParseHexColor(hex, startLine);

		var token = new Token(TokenKind.Color, startLine);
		token.ColorValue = color;

		// Check for '/' immediately followed by '#' (color pair, no whitespace allowed)
		if (pos < source.Length && source[pos] == '/' && pos + 1 < source.Length && source[pos + 1] == '#')
		{
			pos++; // skip '/'
			pos++; // skip '#'
			int darkStart = pos;
			while (pos < source.Length && IsHexDigit(source[pos]))
				pos++;
			string darkHex = source.Substring(darkStart, pos - darkStart);
			token.DarkColorValue = ParseHexColor(darkHex, startLine);
			token.HasDarkColor = true;
		}

		return token;
	}

	private static UInt32 ParseHexColor(string hex, int line)
	{
		switch (hex.Length)
		{
			case 3: // #RGB -> #RRGGBBFF
				return (UInt32)(
					(HexVal(hex[0]) * 17 << 24) |
					(HexVal(hex[1]) * 17 << 16) |
					(HexVal(hex[2]) * 17 << 8) |
					0xFF);
			case 6: // #RRGGBB -> #RRGGBBFF
				return (Convert.ToUInt32(hex, 16) << 8) | 0xFF;
			case 8: // #RRGGBBAA
				return Convert.ToUInt32(hex, 16);
			default:
				throw new ColorProfileParseException($"Invalid color literal '#{hex}' at line {line}. Expected 3, 6, or 8 hex digits.");
		}
	}

	private Token ReadNumber()
	{
		int startLine = line;
		Int64 value;

		if (source[pos] == '0' && pos + 1 < source.Length)
		{
			char next = source[pos + 1];
			if (next == 'x' || next == 'X')
			{
				pos += 2;
				int start = pos;
				while (pos < source.Length && IsHexDigit(source[pos]))
					pos++;
				if (pos == start)
					throw new ColorProfileParseException($"Expected hex digits after '0x' at line {startLine}");
				value = Convert.ToInt64(source.Substring(start, pos - start), 16);
			}
			else if (next == 'b' || next == 'B')
			{
				pos += 2;
				int start = pos;
				while (pos < source.Length && (source[pos] == '0' || source[pos] == '1'))
					pos++;
				if (pos == start)
					throw new ColorProfileParseException($"Expected binary digits after '0b' at line {startLine}");
				value = Convert.ToInt64(source.Substring(start, pos - start), 2);
			}
			else
			{
				int start = pos;
				while (pos < source.Length && source[pos] >= '0' && source[pos] <= '9')
					pos++;
				value = Int64.Parse(source.Substring(start, pos - start));
			}
		}
		else
		{
			int start = pos;
			while (pos < source.Length && source[pos] >= '0' && source[pos] <= '9')
				pos++;
			value = Int64.Parse(source.Substring(start, pos - start));
		}

		var token = new Token(TokenKind.Integer, startLine);
		token.IntValue = value;
		return token;
	}

	private Token ReadIdentifier()
	{
		int startLine = line;
		int start = pos;
		while (pos < source.Length && (source[pos] >= 'a' && source[pos] <= 'z' || source[pos] == '_'))
			pos++;

		string id = source.Substring(start, pos - start);

		if (id == "_")
			return new Token(TokenKind.Underscore, startLine);

		var token = new Token(TokenKind.Identifier, startLine);
		token.StringValue = id;
		return token;
	}

	private void SkipWhitespaceAndComments()
	{
		while (pos < source.Length)
		{
			char c = source[pos];
			if (c == '\n')
			{
				line++;
				pos++;
			}
			else if (c == '\r')
			{
				pos++;
				if (pos < source.Length && source[pos] == '\n')
					pos++;
				line++;
			}
			else if (c == ' ' || c == '\t')
			{
				pos++;
			}
			else if (c == '#')
			{
				// Check if this is a color literal (# followed by hex digit)
				if (pos + 1 < source.Length && IsHexDigit(source[pos + 1]))
					return;
				// Otherwise it's a comment - skip to end of line
				while (pos < source.Length && source[pos] != '\n' && source[pos] != '\r')
					pos++;
			}
			else
			{
				return;
			}
		}
	}

	private char Peek(int offset)
	{
		int idx = pos + offset;
		return idx < source.Length ? source[idx] : '\0';
	}

	private static bool IsHexDigit(char c)
	{
		return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
	}

	private static int HexVal(char c)
	{
		if (c >= '0' && c <= '9') return c - '0';
		if (c >= 'a' && c <= 'f') return c - 'a' + 10;
		return c - 'A' + 10;
	}
}
