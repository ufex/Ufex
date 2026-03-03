using System;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

namespace Ufex.API.Types;

/// <summary>
/// Represents an unsigned integer encoded in LEB128 format.
/// </summary>
public struct Leb128UInt : IEquatable<Leb128UInt>, IComparable<Leb128UInt>, IComparable
{
	// The backing field is a BigInteger to handle large values
	private readonly BigInteger _value;
	public readonly int Size { get; init; } // Number of bytes in the LEB128 encoding

	public const int MaxBytes = 16;
	public static readonly Leb128UInt MinValue = new Leb128UInt(BigInteger.Zero);

	/// <summary>
	/// Encode the unsigned integer with ULEB128 encoding
	/// </summary>
	public byte[] Bytes
	{
		get
		{
			BigInteger tempValue = _value;
			List<byte> bytes = new List<byte>();
			bool more = true;

			while(more) {
				byte chunk = (byte)(tempValue & 0x7fUL); // extract a 7-bit chunk
				tempValue >>= 7;

				more = tempValue != 0;
				if(more)
					chunk |= 0x80; // set msb marker that more bytes are coming

				bytes.Add(chunk);
			};
			return bytes.ToArray();
		}
	}

	public Leb128UInt(byte[] value)
	{
		BigInteger result = 0;
		int shift = 0;

		foreach(byte b in value)
		{
			result |= (BigInteger)(b & 0x7F) << shift;
			if ((b & 0x80) == 0)
				break;
			shift += 7;
			if (shift >= MaxBytes * 7) // Protect against malformed data
				throw new InvalidDataException("LEB128 value too large.");
		}

		_value = result;
		Size = value.Length;
	}

	public Leb128UInt(BigInteger value)
	{
		if (value < 0) throw new OverflowException("Leb128UInt cannot be negative.");
		_value = value;
		Size = Bytes.Length;
	}


	// --- Conversions ---

	public static explicit operator uint(Leb128UInt d) => (uint)d._value;
	public static explicit operator int(Leb128UInt d) => (int)d._value;
	public static explicit operator long(Leb128UInt d) => (long)d._value;
	public static implicit operator BigInteger(Leb128UInt d) => d._value;

	// Explicitly convert FROM uint (Narrowing: Unsafe, requires cast)
	public static explicit operator Leb128UInt(uint d) => new Leb128UInt(d);
	
	// Explicitly convert FROM int
	public static explicit operator Leb128UInt(int d)
	{
			if (d < 0) throw new OverflowException("Leb128UInt cannot be negative.");
			return new Leb128UInt((uint)d);
	}

	// --- Operators (Optional but makes it behave like a native type) ---
	
	public static bool operator ==(Leb128UInt a, Leb128UInt b) => a._value == b._value;
	public static bool operator !=(Leb128UInt a, Leb128UInt b) => a._value != b._value;
	
	public static Leb128UInt operator +(Leb128UInt a, Leb128UInt b) => new Leb128UInt(a._value + b._value);
	public static Leb128UInt operator -(Leb128UInt a, Leb128UInt b)
	{
		if (a._value < b._value) throw new OverflowException("Leb128UInt subtraction would result in a negative value.");
		return new Leb128UInt(a._value - b._value);
	}

	// --- Standard Overrides ---

	public override bool Equals(object obj) => obj is Leb128UInt other && Equals(other);
	public bool Equals(Leb128UInt other) => _value == other._value;
	public override int GetHashCode() => _value.GetHashCode();
	public override string ToString() => _value.ToString();

	// --- Comparable Interface ---
	public int CompareTo(Leb128UInt other) => _value.CompareTo(other._value);
	public int CompareTo(object obj)
	{
		if (obj is null) return 1;
		if (obj is Leb128UInt other) return CompareTo(other);
		throw new ArgumentException("Object is not a Leb128UInt");
	}
}