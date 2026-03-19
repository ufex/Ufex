using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ufex.FileType.Config;

public enum RuleOperator
{
	[XmlEnum("eq")]
	Equal,
	[XmlEnum("neq")]
	NotEqual,
	[XmlEnum("gt")]
	GreaterThan,
	[XmlEnum("gte")]
	GreaterThanOrEqual,
	[XmlEnum("lt")]
	LessThan,
	[XmlEnum("lte")]
	LessThanOrEqual,
	[XmlEnum("and")]
	BitwiseAnd,
	[XmlEnum("or")]
	BitwiseOr,
	[XmlEnum("xor")]
	BitwiseXor,
}

public enum RuleGroupMatch
{
	[XmlEnum("all")]
	All,
	[XmlEnum("any")]
	Any,
	[XmlEnum("none")]
	None,
}

public abstract class SignatureNode
{
}

public class Rule : SignatureNode
{
	[XmlAttribute("type")]
	public string Type { get; set; }

	[XmlAttribute("operator")]
	public RuleOperator Operator { get; set; } = RuleOperator.Equal;

	[XmlIgnore]
	public OffsetExpression Offset { get; set; }

	[XmlAttribute("offset")]
	public string RawOffset
	{
		get { return Offset != null ? Offset.RawExpression : "0"; }
		set { Offset = OffsetExpression.Parse(string.IsNullOrWhiteSpace(value) ? "0" : value); }
	}

	[XmlText]
	public string Value { get; set; }
}

public class SearchRule : SignatureNode
{
	[XmlAttribute("type")]
	public string Type { get; set; }

	[XmlAttribute("operator")]
	public RuleOperator Operator { get; set; } = RuleOperator.Equal;

	[XmlIgnore]
	public OffsetExpression Offset { get; set; }

	[XmlAttribute("offset")]
	public string RawOffset
	{
		get { return Offset != null ? Offset.RawExpression : "0"; }
		set { Offset = OffsetExpression.Parse(string.IsNullOrWhiteSpace(value) ? "0" : value); }
	}

	[XmlAttribute("maxLength")]
	public long MaxLength { get; set; }

	[XmlIgnore]
	public bool MaxLengthSpecified { get; set; }

	[XmlText]
	public string Value { get; set; }
}

public class RuleGroup : SignatureNode
{
	[XmlAttribute("match")]
	public RuleGroupMatch Match { get; set; } = RuleGroupMatch.All;

	[XmlIgnore]
	public OffsetExpression BaseOffset { get; set; }

	[XmlAttribute("base")]
	public string RawBase
	{
		get { return BaseOffset != null ? BaseOffset.RawExpression : null; }
		set { BaseOffset = StringIsNullOrWhiteSpace(value) ? null : OffsetExpression.Parse(value); }
	}

	[XmlElement("Rule", typeof(Rule))]
	[XmlElement("SearchRule", typeof(SearchRule))]
	[XmlElement("RuleGroup", typeof(RuleGroup))]
	[XmlElement("RuleRef", typeof(RuleRef))]
	public List<SignatureNode> Items { get; set; } = new List<SignatureNode>();

	private static bool StringIsNullOrWhiteSpace(string value)
	{
		return string.IsNullOrWhiteSpace(value);
	}
}

public class RuleRef : SignatureNode
{
	[XmlAttribute("name")]
	public string Name { get; set; }
}

public class Signature
{
	[XmlAttribute("minSize")]
	public long MinSize { get; set; } = 0;

	[XmlElement("Rule", typeof(Rule))]
	[XmlElement("SearchRule", typeof(SearchRule))]
	[XmlElement("RuleGroup", typeof(RuleGroup))]
	[XmlElement("RuleRef", typeof(RuleRef))]
	public List<SignatureNode> Items { get; set; } = new List<SignatureNode>();
}

public class RuleDefinition
{
	[XmlAttribute("name")]
	public string Name { get; set; }

	[XmlElement("Rule", typeof(Rule))]
	[XmlElement("SearchRule", typeof(SearchRule))]
	[XmlElement("RuleGroup", typeof(RuleGroup))]
	[XmlElement("RuleRef", typeof(RuleRef))]
	public List<SignatureNode> Items { get; set; } = new List<SignatureNode>();
}
