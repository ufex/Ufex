using System;

namespace Ufex.API
{
	public struct TreeNodeTag
	{
		public Object Tag;

		// Event Handlers
		public TreeNodeMouseClickEventHandler DoubleClickEventHandler;
		public TreeNodeMouseClickEventHandler RightClickBehavior;

		public FileSpan FileRegion;
	}
}
