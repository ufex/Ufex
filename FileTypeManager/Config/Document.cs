using System;
using System.Xml.Serialization;
using UniversalFileExplorer;

namespace Ufex.Config
{
	[XmlRoot("Ufex")]
	public class Document
	{
		[XmlArray]
		[XmlArrayItem(ElementName = "FileType")]
		public FILETYPE[] FileTypes { get; set; }

		[XmlArray]
		[XmlArrayItem(ElementName = "FileTypeClass")]
		public FILETYPE_CLASS[] FileTypeClasses { get; set; }
	}
}
