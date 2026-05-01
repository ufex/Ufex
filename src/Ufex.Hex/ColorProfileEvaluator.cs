using System;

namespace Ufex.Hex;

/// <summary>
/// Evaluates a ColorProfile against a window of bytes to determine per-byte colors.
/// </summary>
public class ColorProfileEvaluator
{
	private readonly ColorProfile profile;
	private readonly string? theme;

	/// <summary>
	/// Creates an evaluator for the given profile.
	/// </summary>
	/// <param name="profile">The color profile to evaluate.</param>
	/// <param name="theme">The active theme name (e.g. "light" or "dark"). Null uses DefaultColor.</param>
	public ColorProfileEvaluator(ColorProfile profile, string? theme = null)
	{
		this.profile = profile;
		this.theme = theme;
	}

	/// <summary>
	/// Evaluates the profile for a range of displayed bytes.
	/// </summary>
	/// <param name="buffer">The full buffer including context bytes on both sides.</param>
	/// <param name="displayStart">The offset within the buffer where the displayed bytes begin.</param>
	/// <param name="displayCount">The number of displayed bytes to evaluate.</param>
	/// <returns>
	/// An array of UInt32 colors (RRGGBBAA), one per displayed byte.
	/// A value of 0 means no rule matched (use default color).
	/// </returns>
	public UInt32[] Evaluate(byte[] buffer, int displayStart, int displayCount)
	{
		var results = new UInt32[displayCount];

		for (int i = 0; i < displayCount; i++)
		{
			int offset = displayStart + i;
			results[i] = EvaluateByte(buffer, offset);
		}

		return results;
	}

	/// <summary>
	/// Evaluates the profile for a single byte at the given offset within the buffer.
	/// </summary>
	/// <param name="buffer">The full buffer including context bytes.</param>
	/// <param name="offset">The offset of the byte to evaluate within the buffer.</param>
	/// <returns>The RRGGBBAA color, or 0 if no rule matched.</returns>
	public UInt32 EvaluateSingle(byte[] buffer, int offset)
	{
		return EvaluateByte(buffer, offset);
	}

	private UInt32 EvaluateByte(byte[] buffer, int offset)
	{
		foreach (var rule in profile.Rules)
		{
			var result = EvalExpr(rule.Predicate, buffer, offset);
			if (result.Kind == ValueKind.Boolean && result.BoolValue)
				return rule.GetColor(theme);
		}
		return 0;
	}

	private EvalResult EvalExpr(Expr expr, byte[] buffer, int offset)
	{
		switch (expr)
		{
			case WildcardExpr:
				return EvalResult.True;

			case CurrentByteExpr:
				return EvalResult.Integer(buffer[offset]);

			case IntegerExpr intExpr:
				return EvalResult.Integer(intExpr.Value);

			case AtExpr atExpr:
				var offsetResult = EvalExpr(atExpr.Offset, buffer, offset);
				if (offsetResult.Kind != ValueKind.Integer)
					return EvalResult.Oob;
				Int64 targetOffset = offset + offsetResult.IntValue;
				if (targetOffset < 0 || targetOffset >= buffer.Length)
					return EvalResult.Oob;
				return EvalResult.Integer(buffer[targetOffset]);

			case UnaryExpr unary:
				return EvalUnary(unary, buffer, offset);

			case BinaryExpr binary:
				return EvalBinary(binary, buffer, offset);

			default:
				return EvalResult.Oob;
		}
	}

	private EvalResult EvalUnary(UnaryExpr expr, byte[] buffer, int offset)
	{
		var operand = EvalExpr(expr.Operand, buffer, offset);

		switch (expr.Op)
		{
			case UnaryOp.Not:
				if (operand.Kind == ValueKind.Boolean)
					return operand.BoolValue ? EvalResult.False : EvalResult.True;
				return EvalResult.Error;

			case UnaryOp.BitwiseNot:
				if (operand.Kind == ValueKind.Integer)
					return EvalResult.Integer(~operand.IntValue);
				return EvalResult.Error;

			case UnaryOp.Negate:
				if (operand.Kind == ValueKind.Integer)
					return EvalResult.Integer(-operand.IntValue);
				return EvalResult.Error;

			default:
				return EvalResult.Error;
		}
	}

	private EvalResult EvalBinary(BinaryExpr expr, byte[] buffer, int offset)
	{
		var left = EvalExpr(expr.Left, buffer, offset);

		// Short-circuit for logical operators
		switch (expr.Op)
		{
			case BinaryOp.And:
				if (left.Kind == ValueKind.Boolean && !left.BoolValue)
					return EvalResult.False;
				var andRight = EvalExpr(expr.Right, buffer, offset);
				if (left.Kind == ValueKind.Boolean && andRight.Kind == ValueKind.Boolean)
					return andRight.BoolValue ? EvalResult.True : EvalResult.False;
				return EvalResult.Error;

			case BinaryOp.Or:
				if (left.Kind == ValueKind.Boolean && left.BoolValue)
					return EvalResult.True;
				var orRight = EvalExpr(expr.Right, buffer, offset);
				if (left.Kind == ValueKind.Boolean && orRight.Kind == ValueKind.Boolean)
					return orRight.BoolValue ? EvalResult.True : EvalResult.False;
				return EvalResult.Error;
		}

		var right = EvalExpr(expr.Right, buffer, offset);

		// Comparisons: OOB in either operand => false
		switch (expr.Op)
		{
			case BinaryOp.Equal:
			case BinaryOp.NotEqual:
			case BinaryOp.LessThan:
			case BinaryOp.LessEqual:
			case BinaryOp.GreaterThan:
			case BinaryOp.GreaterEqual:
				if (left.Kind == ValueKind.Oob || right.Kind == ValueKind.Oob)
					return EvalResult.False;
				if (left.Kind != ValueKind.Integer || right.Kind != ValueKind.Integer)
				{
					// Boolean equality
					if (left.Kind == ValueKind.Boolean && right.Kind == ValueKind.Boolean &&
						(expr.Op == BinaryOp.Equal || expr.Op == BinaryOp.NotEqual))
					{
						bool eq = left.BoolValue == right.BoolValue;
						return (expr.Op == BinaryOp.Equal ? eq : !eq) ? EvalResult.True : EvalResult.False;
					}
					return EvalResult.Error;
				}
				return EvalCompare(left.IntValue, expr.Op, right.IntValue);
		}

		// Bitwise/shift: require integer operands
		if (left.Kind != ValueKind.Integer || right.Kind != ValueKind.Integer)
			return EvalResult.Error;

		switch (expr.Op)
		{
			case BinaryOp.BitwiseAnd:
				return EvalResult.Integer(left.IntValue & right.IntValue);
			case BinaryOp.BitwiseOr:
				return EvalResult.Integer(left.IntValue | right.IntValue);
			case BinaryOp.BitwiseXor:
				return EvalResult.Integer(left.IntValue ^ right.IntValue);
			case BinaryOp.ShiftLeft:
				return EvalResult.Integer(left.IntValue << (int)(right.IntValue & 63));
			case BinaryOp.ShiftRight:
				return EvalResult.Integer(left.IntValue >> (int)(right.IntValue & 63));
			default:
				return EvalResult.Error;
		}
	}

	private static EvalResult EvalCompare(Int64 left, BinaryOp op, Int64 right)
	{
		bool result = op switch
		{
			BinaryOp.Equal => left == right,
			BinaryOp.NotEqual => left != right,
			BinaryOp.LessThan => left < right,
			BinaryOp.LessEqual => left <= right,
			BinaryOp.GreaterThan => left > right,
			BinaryOp.GreaterEqual => left >= right,
			_ => false,
		};
		return result ? EvalResult.True : EvalResult.False;
	}
}

internal enum ValueKind
{
	Integer,
	Boolean,
	Oob,
	Error,
}

internal struct EvalResult
{
	public ValueKind Kind;
	public Int64 IntValue;
	public bool BoolValue;

	public static readonly EvalResult True = new EvalResult { Kind = ValueKind.Boolean, BoolValue = true };
	public static readonly EvalResult False = new EvalResult { Kind = ValueKind.Boolean, BoolValue = false };
	public static readonly EvalResult Oob = new EvalResult { Kind = ValueKind.Oob };
	public static readonly EvalResult Error = new EvalResult { Kind = ValueKind.Error };

	public static EvalResult Integer(Int64 value) => new EvalResult { Kind = ValueKind.Integer, IntValue = value };
}
