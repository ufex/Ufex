using System;

namespace Ufex.Hex;

/// <summary>
/// Base class for all DSL expression AST nodes.
/// </summary>
public abstract class Expr
{
}

/// <summary>
/// Integer literal (e.g. 0xFF, 42, 0b1010).
/// </summary>
public sealed class IntegerExpr : Expr
{
	public Int64 Value { get; }

	public IntegerExpr(Int64 value)
	{
		Value = value;
	}
}

/// <summary>
/// The current byte: `b`
/// </summary>
public sealed class CurrentByteExpr : Expr
{
}

/// <summary>
/// Neighbor byte access: `at(offset)`
/// </summary>
public sealed class AtExpr : Expr
{
	public Expr Offset { get; }

	public AtExpr(Expr offset)
	{
		Offset = offset;
	}
}

/// <summary>
/// Wildcard predicate: `_` (always true)
/// </summary>
public sealed class WildcardExpr : Expr
{
}

/// <summary>
/// Binary operator expression.
/// </summary>
public sealed class BinaryExpr : Expr
{
	public Expr Left { get; }
	public BinaryOp Op { get; }
	public Expr Right { get; }

	public BinaryExpr(Expr left, BinaryOp op, Expr right)
	{
		Left = left;
		Op = op;
		Right = right;
	}
}

/// <summary>
/// Unary operator expression (!, ~).
/// </summary>
public sealed class UnaryExpr : Expr
{
	public UnaryOp Op { get; }
	public Expr Operand { get; }

	public UnaryExpr(UnaryOp op, Expr operand)
	{
		Op = op;
		Operand = operand;
	}
}

public enum BinaryOp
{
	// Logical
	Or,
	And,
	// Comparison
	Equal,
	NotEqual,
	LessThan,
	LessEqual,
	GreaterThan,
	GreaterEqual,
	// Bitwise
	BitwiseAnd,
	BitwiseOr,
	BitwiseXor,
	// Shift
	ShiftLeft,
	ShiftRight,
}

public enum UnaryOp
{
	Not,
	BitwiseNot,
	Negate,
}
