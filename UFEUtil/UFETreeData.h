// UFETreeData.h

#pragma once

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Windows::Forms;


namespace UFETreeData
{

	// An Icon for a Tree Node Element
	public ref struct TN_ICON
	{
		UInt16 width;
		UInt16 height;
		array<Byte, 2>^ image;
	};

	// A Tree Node Element
	public ref struct TREE_NODE
	{
		String^ name;
		int icon;
		array<TREE_NODE^>^ children;
	};

	public ref class TreeData
	{
	public:
		TreeData(void);
		virtual ~TreeData();

		void AddNode(TREE_NODE^ tn);
		void AddNode(String^ name, int image);

		int get_NumNodes();
		TreeNode^ get_Node(int index);

		TREE_NODE^ NewNode(String^ name);


			
	private:
		void ExpandTreeNodes();

		// Recursive function that converts a TREE_NODE to a TreeNode
		TreeNode^ GetTreeNode(TREE_NODE^ tn);

		int m_NumNodes;
		array<TREE_NODE^>^ m_Nodes;
		array<TN_ICON^>^ m_Icons;

	};
};
