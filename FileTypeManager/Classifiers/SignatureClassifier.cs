using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ufex.Config;
using UniversalFileExplorer;

namespace Ufex.Classifiers
{
	class SignatureClassifier : FileTypeClassifier
	{
		const long BUFFER_SIZE = 8096;

		public override string GetFileType(string filePath, FileStream fileStream)
		{
			HashSet<string> matches = new HashSet<string>();
			int bufferSize = (int)Math.Min(BUFFER_SIZE, fileStream.Length);
			byte[] buffer = new byte[bufferSize];
			fileStream.Read(buffer, 0, bufferSize);
			foreach(FILETYPE fileType in FileTypes.FileTypes)
			{
				if(fileType.Signatures != null && fileType.Signatures.Count > 0)
				{
					if(MatchesAnySignature(fileType.Signatures, buffer, fileStream))
					{
						matches.Add(fileType.ID);
					}
				}
			}
			return matches.ToArray()[0]; // TODO
		}

		public bool MatchesAnySignature(List<SignaturePattern[]> signatures, byte[] buffer, FileStream fileStream)
		{
			foreach(SignaturePattern[] patterns in signatures)
			{
				if(MatchesSignature(patterns, buffer, fileStream))
				{
					return true;
				}
			}
			return false;
		}

		public bool MatchesSignature(SignaturePattern[] patterns, byte[] buffer, FileStream fileStream)
		{
			if(patterns.Length == 0)
            {
				return false;
            }
			foreach(SignaturePattern pattern in patterns)
			{
				if(!pattern.Matches(buffer, fileStream))
				{
					return false;
				}
			}
			return true;
		}
	}
}
