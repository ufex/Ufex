using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ufex.API;

namespace Ufex.FileType.Classifiers;

class ExtensionClassifier : Ufex.FileType.BaseClassifier
{
	public ExtensionClassifier()
	{
	}

	public ExtensionClassifier(Logger log) : base(log)
	{
	}

	public override string GetFileType(string filePath, FileStream fileStream)
	{
		if(!filePath.Contains("."))
					{
			// No extension
			return null;
					}
		HashSet<string> matches = new HashSet<string>();
		string extension = filePath.Split('.').Last().ToLower();
		foreach (FileTypeRecord fileType in FileTypes.FileTypes)
		{
			if (fileType.Extensions != null && fileType.Extensions.Length > 0)
			{
				if (fileType.Extensions.Contains(extension))
				{
					matches.Add(fileType.ID);
				}
			}
		}
		if(matches.Count > 0)
					{
			return matches.ToArray()[0]; // TODO
		}
		return base.FileTypeUnknown;
	}
}
