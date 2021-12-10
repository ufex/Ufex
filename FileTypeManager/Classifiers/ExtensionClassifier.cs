using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ufex.Config;
using UniversalFileExplorer;

namespace Ufex.Classifiers
{
	class ExtensionClassifier : FileTypeClassifier
	{
		public override string GetFileType(string filePath, FileStream fileStream)
		{
			if(!filePath.Contains("."))
            {
				// No extension
				return null;
            }
			HashSet<string> matches = new HashSet<string>();
			string extension = filePath.Split('.').Last().ToLower();
			foreach (FILETYPE fileType in FileTypes.FileTypes)
			{
				if (fileType.Extensions != null && fileType.Extensions.Length > 0)
				{
					if (fileType.Extensions.Contains(extension))
					{
						matches.Add(fileType.ID);
					}
				}
			}
			return matches.ToArray()[0]; // TODO
		}
	}
}