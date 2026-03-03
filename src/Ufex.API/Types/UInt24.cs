using System;
using System.Runtime.InteropServices;

namespace Ufex.API.Types;

// StructLayout ensures we control the memory layout if needed, 
// though for a single field struct, generic auto layout is usually fine.
[StructLayout(LayoutKind.Sequential)]
public struct UInt24 : IEquatable<UInt24>, IComparable<UInt24>, IComparable
{
	// The backing field is 32-bits for memory alignment speed.
	private readonly uint _value;

	public const uint MaxValue = 16777215; // 2^24 - 1
	public const uint MinValue = 0;

	// Constructor checks bounds to prevent logic errors before they hit the disk
	public UInt24(uint value)
	{
		if (value > MaxValue)
		{
			throw new OverflowException($"Value {value} exceeds UInt24 max value of {MaxValue}");
		}
		_value = value;
	}

	// --- Conversions ---

	// Implicitly convert TO uint (Widening: Safe)
	public static implicit operator uint(UInt24 d) => d._value;
	public static implicit operator int(UInt24 d) => (int)d._value;
	public static implicit operator long(UInt24 d) => d._value;

	// Explicitly convert FROM uint (Narrowing: Unsafe, requires cast)
	public static explicit operator UInt24(uint d) => new UInt24(d);
	
	// Explicitly convert FROM int
	public static explicit operator UInt24(int d)
	{
		if (d < 0) throw new OverflowException("UInt24 cannot be negative.");
		return new UInt24((uint)d);
	}

	// --- Operators (Optional but makes it behave like a native type) ---
	
	public static bool operator ==(UInt24 a, UInt24 b) => a._value == b._value;
	public static bool operator !=(UInt24 a, UInt24 b) => a._value != b._value;
	
	public static UInt24 operator +(UInt24 a, UInt24 b) => (UInt24)(a._value + b._value);
	public static UInt24 operator -(UInt24 a, UInt24 b) => (UInt24)(a._value - b._value);

	// --- Standard Overrides ---

	public override bool Equals(object obj) => obj is UInt24 other && Equals(other);
	public bool Equals(UInt24 other) => _value == other._value;
	public override int GetHashCode() => _value.GetHashCode();
	public override string ToString() => _value.ToString();

	// --- Comparable Interface ---
	public int CompareTo(UInt24 other) => _value.CompareTo(other._value);
	public int CompareTo(object obj)
	{
		if (obj is null) return 1;
		if (obj is UInt24 other) return CompareTo(other);
		throw new ArgumentException("Object is not a UInt24");
	}
}