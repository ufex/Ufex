using System;

namespace Ufex.Hex;

public class ColorProfileParseException : Exception
{
	public ColorProfileParseException(string message) : base(message) { }
	public ColorProfileParseException(string message, Exception inner) : base(message, inner) { }
}
