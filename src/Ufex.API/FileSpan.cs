using System;

namespace Ufex.API
{
	public struct FileSpan
	{
		public FileSpan(Int64 startPos, Int64 endPos) 
		{ 
			StartPosition = startPos;
			EndPosition = endPos; 
		}
		public Int64 StartPosition;
		public Int64 EndPosition;
	}
}
