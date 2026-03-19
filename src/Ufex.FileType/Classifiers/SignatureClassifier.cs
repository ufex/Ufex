using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ufex.API;
using Ufex.FileType.Config;

namespace Ufex.FileType.Classifiers;

class SignatureClassifier : FileType.BaseClassifier
{
	const long BUFFER_SIZE = 8096;
	const string STRING_TYPE_PREFIX = "string:";

	private sealed class MatchContext
	{
		public byte[] Buffer { get; }
		public FileStream FileStream { get; }
		public long BaseOffset { get; }
		public Dictionary<string, RuleDefinition> RuleDefinitions { get; }

		public MatchContext(byte[] buffer, FileStream fileStream, long baseOffset, Dictionary<string, RuleDefinition> ruleDefinitions)
		{
			Buffer = buffer;
			FileStream = fileStream;
			BaseOffset = baseOffset;
			RuleDefinitions = ruleDefinitions;
		}

		public MatchContext WithBaseOffset(long baseOffset)
		{
			return new MatchContext(Buffer, FileStream, baseOffset, RuleDefinitions);
		}
	}

	public SignatureClassifier()
	{
	}

	public SignatureClassifier(Logger log) : base(log)
	{
	}

	public override string[] DetectFileType(string filePath, FileStream fileStream)
	{
		HashSet<string> matches = new HashSet<string>();
		int bufferSize = (int)Math.Min(BUFFER_SIZE, fileStream.Length);
		byte[] buffer = new byte[bufferSize];
		fileStream.ReadExactly(buffer, 0, bufferSize);
		MatchContext ctx = new MatchContext(buffer, fileStream, 0, FileTypes.RuleDefinitions);

		foreach(FileTypeRecord fileType in FileTypes.FileTypes)
		{
			try
			{
				if(fileType.Signatures != null && fileType.Signatures.Count > 0)
				{
					if(MatchesAnySignature(fileType.Signatures, ctx))
					{
						matches.Add(fileType.ID);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex, "SignatureClassifier.DetectFileType: Failed to match signatures for {FileType}", fileType.ID);
			}
		}
		return matches.ToArray();
	}

	private bool MatchesAnySignature(List<Signature> signatures, MatchContext ctx)
	{
		foreach(Signature signature in signatures)
		{
			if(MatchesSignature(signature, ctx))
			{
				return true;
			}
		}
		return false;
	}

	private bool MatchesSignature(Signature signature, MatchContext ctx)
	{
		if(signature == null || signature.Items == null || signature.Items.Count == 0)
		{
			return false;
		}

		if(signature.MinSize > 0 && ctx.FileStream.Length < signature.MinSize)
		{
			return false;
		}

		foreach(SignatureNode node in signature.Items)
		{
			if(!MatchNode(node, ctx))
			{
				return false;
			}
		}

		return true;
	}

	private bool MatchNode(SignatureNode node, MatchContext ctx)
	{
		switch(node)
		{
			case Rule rule:
				return MatchRule(rule, ctx);
			case SearchRule searchRule:
				return MatchSearchRule(searchRule, ctx);
			case RuleGroup ruleGroup:
				return MatchRuleGroup(ruleGroup, ctx);
			case RuleRef ruleRef:
				return MatchRuleRef(ruleRef, ctx);
			default:
				return false;
		}
	}

	private bool MatchRule(Rule rule, MatchContext ctx)
	{
		if(rule == null || String.IsNullOrWhiteSpace(rule.Type))
		{
			return false;
		}

		if(!ResolveOffset(rule.Offset, ctx, out long offset))
		{
			return false;
		}

		return MatchRuleAtOffset(rule.Type, rule.Operator, rule.Value, offset, ctx);
	}

	private bool MatchSearchRule(SearchRule searchRule, MatchContext ctx)
	{
		if(searchRule == null || String.IsNullOrWhiteSpace(searchRule.Type))
		{
			return false;
		}

		if(!ResolveOffset(searchRule.Offset, ctx, out long startOffset))
		{
			return false;
		}

		if(startOffset >= ctx.FileStream.Length)
		{
			return false;
		}

		long maxLength = searchRule.MaxLengthSpecified ? Math.Max(0, searchRule.MaxLength) : (ctx.FileStream.Length - startOffset);
		long lastOffset = startOffset + maxLength;
		if(lastOffset < startOffset)
		{
			lastOffset = ctx.FileStream.Length;
		}
		if(lastOffset > ctx.FileStream.Length)
		{
			lastOffset = ctx.FileStream.Length;
		}

		for(long currentOffset = startOffset; currentOffset < lastOffset; currentOffset++)
		{
			if(MatchRuleAtOffset(searchRule.Type, searchRule.Operator, searchRule.Value, currentOffset, ctx))
			{
				return true;
			}
		}

		return false;
	}

	private bool MatchRuleGroup(RuleGroup ruleGroup, MatchContext ctx)
	{
		if(ruleGroup == null || ruleGroup.Items == null)
		{
			return false;
		}

		long baseOffset = ctx.BaseOffset;
		if(ruleGroup.BaseOffset != null)
		{
			if(!ruleGroup.BaseOffset.TryEvaluate(ctx.Buffer, ctx.FileStream, out long relativeBase))
			{
				return false;
			}

			try
			{
				baseOffset = checked(baseOffset + relativeBase);
			}
			catch(OverflowException)
			{
				return false;
			}

			if(baseOffset < 0)
			{
				return false;
			}
		}

		MatchContext childContext = ctx.WithBaseOffset(baseOffset);

		switch(ruleGroup.Match)
		{
			case RuleGroupMatch.All:
				foreach(SignatureNode node in ruleGroup.Items)
				{
					if(!MatchNode(node, childContext))
					{
						return false;
					}
				}
				return true;

			case RuleGroupMatch.Any:
				foreach(SignatureNode node in ruleGroup.Items)
				{
					if(MatchNode(node, childContext))
					{
						return true;
					}
				}
				return false;

			case RuleGroupMatch.None:
				foreach(SignatureNode node in ruleGroup.Items)
				{
					if(MatchNode(node, childContext))
					{
						return false;
					}
				}
				return true;
		}

		return false;
	}

	private bool MatchRuleRef(RuleRef ruleRef, MatchContext ctx)
	{
		if(ruleRef == null || String.IsNullOrWhiteSpace(ruleRef.Name))
		{
			return false;
		}

		if(!ctx.RuleDefinitions.TryGetValue(ruleRef.Name, out RuleDefinition definition))
		{
			Log.Warning("SignatureClassifier.MatchRuleRef: RuleDefinition not found: {RuleDefinitionName}", ruleRef.Name);
			return false;
		}

		if(definition.Items == null || definition.Items.Count == 0)
		{
			return false;
		}

		foreach(SignatureNode node in definition.Items)
		{
			if(!MatchNode(node, ctx))
			{
				return false;
			}
		}

		return true;
	}

	private bool ResolveOffset(OffsetExpression expression, MatchContext ctx, out long offset)
	{
		offset = 0;
		OffsetExpression targetExpression = expression ?? OffsetExpression.Parse("0");

		if(!targetExpression.TryEvaluate(ctx.Buffer, ctx.FileStream, out long baseValue))
		{
			return false;
		}

		try
		{
			offset = checked(baseValue + ctx.BaseOffset);
		}
		catch(OverflowException)
		{
			return false;
		}

		if(offset < 0)
		{
			return false;
		}

		return true;
	}

	private bool MatchRuleAtOffset(string type, RuleOperator op, string rawValue, long offset, MatchContext ctx)
	{
		string normalizedType = type.Trim().ToLowerInvariant();

		if(normalizedType == "byte")
		{
			if(!TryReadByte(offset, ctx, out byte fileValue))
			{
				return false;
			}

			byte ruleValue = Parser.Byte(rawValue);
			return EvaluateOperator(fileValue, ruleValue, op);
		}

		if(normalizedType == "bytes")
		{
			byte[] ruleValue = Parser.ByteArray(rawValue);
			if(!TryReadBytes(offset, ruleValue.Length, ctx, out byte[] fileValue))
			{
				return false;
			}

			bool equal = ByteArraysEqual(fileValue, ruleValue);
			if(op == RuleOperator.NotEqual)
			{
				return !equal;
			}
			if(op == RuleOperator.Equal)
			{
				return equal;
			}
			return false;
		}

		if(normalizedType == "string" || normalizedType.StartsWith(STRING_TYPE_PREFIX, StringComparison.Ordinal))
		{
			if(!TryGetRuleStringEncoding(normalizedType, out Encoding encoding))
			{
				return false;
			}
			byte[] ruleValue = encoding.GetBytes(rawValue ?? String.Empty);
			if(!TryReadBytes(offset, ruleValue.Length, ctx, out byte[] fileValue))
			{
				return false;
			}

			bool equal = ByteArraysEqual(fileValue, ruleValue);
			if(op == RuleOperator.NotEqual)
			{
				return !equal;
			}
			if(op == RuleOperator.Equal)
			{
				return equal;
			}
			return false;
		}

		if(!TryReadInteger(normalizedType, offset, ctx, out long fileIntegerValue, out bool fileIntegerUnsigned))
		{
			return false;
		}

		if(!TryParseIntegerRuleValue(normalizedType, rawValue, out long ruleIntegerValue, out bool ruleIntegerUnsigned))
		{
			return false;
		}

		if(fileIntegerUnsigned != ruleIntegerUnsigned)
		{
			return false;
		}

		return EvaluateOperator(fileIntegerValue, ruleIntegerValue, op);
	}

	private static bool TryGetRuleStringEncoding(string normalizedType, out Encoding encoding)
	{
		encoding = Encoding.UTF8;

		if(normalizedType == "string")
		{
			return true;
		}

		string encodingName = normalizedType.Substring(STRING_TYPE_PREFIX.Length);
		switch(encodingName)
		{
			case "utf-8":
			case "utf8":
				encoding = Encoding.UTF8;
				return true;
			case "ascii":
				encoding = Encoding.ASCII;
				return true;
			case "utf-16":
			case "utf16":
			case "utf-16le":
			case "utf16le":
				encoding = Encoding.Unicode;
				return true;
			case "utf-16be":
			case "utf16be":
				encoding = Encoding.BigEndianUnicode;
				return true;
			case "utf-32":
			case "utf32":
			case "utf-32le":
			case "utf32le":
				encoding = Encoding.UTF32;
				return true;
		}

		try
		{
			encoding = Encoding.GetEncoding(encodingName);
			return true;
		}
		catch(ArgumentException)
		{
			return false;
		}
	}

	private bool TryReadInteger(string type, long offset, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = false;

		switch(type)
		{
			case "uint8":
				if(!TryReadByte(offset, ctx, out byte u8Value))
				{
					return false;
				}
				value = u8Value;
				isUnsigned = true;
				return true;
			case "int8":
				if(!TryReadByte(offset, ctx, out byte i8Value))
				{
					return false;
				}
				value = unchecked((sbyte)i8Value);
				isUnsigned = false;
				return true;
			case "uint16le":
				return TryReadUInt16(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "uint16be":
				return TryReadUInt16(offset, Endian.Big, ctx, out value, out isUnsigned);
			case "int16le":
				return TryReadInt16(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "int16be":
				return TryReadInt16(offset, Endian.Big, ctx, out value, out isUnsigned);
			case "uint32le":
				return TryReadUInt32(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "uint32be":
				return TryReadUInt32(offset, Endian.Big, ctx, out value, out isUnsigned);
			case "int32le":
				return TryReadInt32(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "int32be":
				return TryReadInt32(offset, Endian.Big, ctx, out value, out isUnsigned);
			case "uint64le":
				return TryReadUInt64(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "uint64be":
				return TryReadUInt64(offset, Endian.Big, ctx, out value, out isUnsigned);
			case "int64le":
				return TryReadInt64(offset, Endian.Little, ctx, out value, out isUnsigned);
			case "int64be":
				return TryReadInt64(offset, Endian.Big, ctx, out value, out isUnsigned);
			default:
				return false;
		}
	}

	private bool TryParseIntegerRuleValue(string type, string rawValue, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = false;

		switch(type)
		{
			case "uint8":
				value = Parser.Byte(rawValue);
				isUnsigned = true;
				return true;
			case "int8":
				if(rawValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					value = unchecked((sbyte)Parser.Byte(rawValue));
				}
				else
				{
					value = SByte.Parse(rawValue);
				}
				isUnsigned = false;
				return true;
			case "uint16le":
			case "uint16be":
				value = Parser.UInt16(rawValue);
				isUnsigned = true;
				return true;
			case "int16le":
			case "int16be":
				value = Parser.Int16(rawValue);
				isUnsigned = false;
				return true;
			case "uint32le":
			case "uint32be":
				value = Parser.UInt32(rawValue);
				isUnsigned = true;
				return true;
			case "int32le":
			case "int32be":
				value = Parser.Int32(rawValue);
				isUnsigned = false;
				return true;
			case "uint64le":
			case "uint64be":
				{
					ulong tmp = Parser.UInt64(rawValue);
					if(tmp > Int64.MaxValue)
					{
						return false;
					}
					value = (long)tmp;
					isUnsigned = true;
					return true;
				}
			case "int64le":
			case "int64be":
				value = Parser.Int64(rawValue);
				isUnsigned = false;
				return true;
			default:
				return false;
		}
	}

	private static bool EvaluateOperator(long fileValue, long ruleValue, RuleOperator op)
	{
		switch(op)
		{
			case RuleOperator.Equal:
				return fileValue == ruleValue;
			case RuleOperator.NotEqual:
				return fileValue != ruleValue;
			case RuleOperator.GreaterThan:
				return fileValue > ruleValue;
			case RuleOperator.GreaterThanOrEqual:
				return fileValue >= ruleValue;
			case RuleOperator.LessThan:
				return fileValue < ruleValue;
			case RuleOperator.LessThanOrEqual:
				return fileValue <= ruleValue;
			case RuleOperator.BitwiseAnd:
				return (fileValue & ruleValue) != 0;
			case RuleOperator.BitwiseOr:
				return (fileValue | ruleValue) != 0;
			case RuleOperator.BitwiseXor:
				return (fileValue ^ ruleValue) != 0;
			default:
				return false;
		}
	}

	private static bool ByteArraysEqual(byte[] left, byte[] right)
	{
		if(left.Length != right.Length)
		{
			return false;
		}

		for(int i = 0; i < left.Length; i++)
		{
			if(left[i] != right[i])
			{
				return false;
			}
		}

		return true;
	}

	private bool TryReadByte(long offset, MatchContext ctx, out byte value)
	{
		value = 0;
		if(!TryReadBytes(offset, 1, ctx, out byte[] bytes))
		{
			return false;
		}

		value = bytes[0];
		return true;
	}

	private bool TryReadUInt16(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = true;
		if(!TryReadBytes(offset, 2, ctx, out byte[] bytes))
		{
			return false;
		}

		value = ByteUtil.BytesToUInt16(bytes, endian, 0);
		return true;
	}

	private bool TryReadInt16(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = false;
		if(!TryReadBytes(offset, 2, ctx, out byte[] bytes))
		{
			return false;
		}

		value = ByteUtil.BytesToInt16(bytes, endian, 0);
		return true;
	}

	private bool TryReadUInt32(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = true;
		if(!TryReadBytes(offset, 4, ctx, out byte[] bytes))
		{
			return false;
		}

		value = ByteUtil.BytesToUInt32(bytes, endian, 0);
		return true;
	}

	private bool TryReadInt32(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = false;
		if(!TryReadBytes(offset, 4, ctx, out byte[] bytes))
		{
			return false;
		}

		value = ByteUtil.BytesToInt32(bytes, endian, 0);
		return true;
	}

	private bool TryReadUInt64(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = true;
		if(!TryReadBytes(offset, 8, ctx, out byte[] bytes))
		{
			return false;
		}

		ulong tmp = ByteUtil.BytesToUInt64(bytes, endian, 0);
		if(tmp > Int64.MaxValue)
		{
			return false;
		}

		value = (long)tmp;
		return true;
	}

	private bool TryReadInt64(long offset, Endian endian, MatchContext ctx, out long value, out bool isUnsigned)
	{
		value = 0;
		isUnsigned = false;
		if(!TryReadBytes(offset, 8, ctx, out byte[] bytes))
		{
			return false;
		}

		if(endian == Endian.Little)
		{
			value = unchecked((long)ByteUtil.BytesToUInt64(bytes, Endian.Little, 0));
		}
		else
		{
			value = unchecked((long)ByteUtil.BytesToUInt64(bytes, Endian.Big, 0));
		}
		return true;
	}

	private bool TryReadBytes(long offset, int count, MatchContext ctx, out byte[] bytes)
	{
		bytes = Array.Empty<byte>();
		if(offset < 0 || count < 0)
		{
			return false;
		}

		long endOffset;
		try
		{
			endOffset = checked(offset + count);
		}
		catch(OverflowException)
		{
			return false;
		}

		if(endOffset > ctx.FileStream.Length)
		{
			return false;
		}

		bytes = new byte[count];
		if(endOffset <= ctx.Buffer.Length)
		{
			Buffer.BlockCopy(ctx.Buffer, (int)offset, bytes, 0, count);
			return true;
		}

		long originalPosition = ctx.FileStream.Position;
		try
		{
			ctx.FileStream.Seek(offset, SeekOrigin.Begin);
			int totalRead = 0;
			while(totalRead < count)
			{
				int bytesRead = ctx.FileStream.Read(bytes, totalRead, count - totalRead);
				if(bytesRead <= 0)
				{
					return false;
				}
				totalRead += bytesRead;
			}
			return true;
		}
		finally
		{
			ctx.FileStream.Seek(originalPosition, SeekOrigin.Begin);
		}
	}
}
