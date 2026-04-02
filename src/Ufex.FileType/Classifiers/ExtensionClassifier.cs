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

	public override string[] DetectFileType(string filePath, FileStream fileStream)
	{
		if(!TryGetNormalizedExtension(filePath, out string extension))
		{
			// No extension
			return null;
		}
		HashSet<string> matches = new HashSet<string>();
		foreach(FileTypeRecord fileType in FileTypes.FileTypes)
		{
			if(fileType.Extensions != null && fileType.Extensions.Length > 0)
			{
				if(fileType.Extensions.Contains(extension))
				{
					matches.Add(fileType.ID);
				}
			}
		}
		return matches.ToArray();
	}

	public override DetectionMatch[] DetectFileTypeDetailed(string filePath, FileStream fileStream)
	{
		if (!TryGetNormalizedExtension(filePath, out string extension))
			return Array.Empty<DetectionMatch>();
		List<DetectionMatch> matches = new();

		foreach (FileTypeRecord fileType in FileTypes.FileTypes)
		{
			if (fileType.Extensions != null && fileType.Extensions.Contains(extension))
			{
				matches.Add(new DetectionMatch
				{
					FileType = fileType,
					Method = MatchMethod.Extension,
					MatchedExtension = extension,
				});
			}
		}

		return matches.ToArray();
	}

	private static bool TryGetNormalizedExtension(string filePath, out string extension)
	{
		extension = string.Empty;

		string rawExtension = Path.GetExtension(filePath);
		if (string.IsNullOrEmpty(rawExtension))
			return false;

		extension = rawExtension.TrimStart('.').ToLowerInvariant();
		return extension.Length > 0;
	}
}
