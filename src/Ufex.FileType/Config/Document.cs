using System;
using System.Xml.Serialization;
using Ufex.FileType;

namespace Ufex.FileType.Config
{
	[XmlRoot("Ufex")]
	public class Document
	{
		[XmlArray]
		[XmlArrayItem(ElementName = "RuleDefinition")]
		public RuleDefinition[] RuleDefinitions { get; set; }

		[XmlArray]
		[XmlArrayItem(ElementName = "FileType")]
		public FileTypeRecord[] FileTypes { get; set; }

		[XmlArray]
		[XmlArrayItem(ElementName = "FileTypeClass")]
		public FileTypeClassRecord[] FileTypeClasses { get; set; }
	}
}
