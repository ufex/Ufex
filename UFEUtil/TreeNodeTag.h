#pragma once

using namespace System::Windows::Forms;

namespace UniversalFileExplorer
{
	public ref struct FileSpan
	{
		public: 
			FileSpan(__int64 startPos, __int64 endPos) { StartPosition = startPos; EndPosition = endPos; };
			__int64 StartPosition;
			__int64 EndPosition;
	};

	public ref struct TreeNodeTag
	{
	public:
		Object^ Tag;
		
		// Event Handlers
		TreeNodeMouseClickEventHandler^ DoubleClickEventHandler;
		TreeNodeMouseClickEventHandler^ RightClickBehavior;
		
		FileSpan^ FileRegion;

	};
}
