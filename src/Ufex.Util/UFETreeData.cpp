// UFETreeData.cpp

#include "StdAfx.h"
#include ".\UFETreeData.h"


namespace UFETreeData
{
	TreeData::TreeData(void)
	{
		m_Nodes = gcnew array<TREE_NODE^>(10);
		m_Icons = gcnew array<TN_ICON^>(10);
	}

	TreeData::~TreeData()
	{
		delete m_Nodes;
		delete m_Icons;
	}

	void TreeData::AddNode(TREE_NODE^ tn)
	{
		if(m_Nodes->Length <= m_NumNodes)
			ExpandTreeNodes();

		m_Nodes[m_NumNodes] = tn;
		m_NumNodes++;
	}
	
	void TreeData::AddNode(String^ name, int icon)
	{		
		if(m_Nodes->Length <= m_NumNodes)
			ExpandTreeNodes();
		
		TREE_NODE^ tmpTreeNode = gcnew TREE_NODE;
		tmpTreeNode->name = name;
		tmpTreeNode->icon = icon;
		tmpTreeNode->children = nullptr;
		m_Nodes[m_NumNodes] = tmpTreeNode;
		m_NumNodes++;
	}

	TREE_NODE^ TreeData::NewNode(String^ name)
	{
		TREE_NODE^ tmpTreeNode = gcnew TREE_NODE;
		tmpTreeNode->name = name;
		tmpTreeNode->icon = -1;
		tmpTreeNode->children = nullptr;
		return tmpTreeNode;
	}


	int TreeData::get_NumNodes()
	{
		return m_NumNodes;
	}

	TreeNode^ TreeData::get_Node(int index)
	{
		return GetTreeNode(m_Nodes[index]);
	}

	TreeNode^ TreeData::GetTreeNode(TREE_NODE^ tn)
	{
		TreeNode^ tmpTreeNode = gcnew TreeNode(tn->name);
		if(tn->children != nullptr)
		{
			// Add all the children (recursion!)
			for(int i = 0; i < tn->children->Length; tn)
			{
				tmpTreeNode->Nodes->Add(GetTreeNode(tn->children[i]));
			}
		}
		return tmpTreeNode;
	}

	void TreeData::ExpandTreeNodes()
	{
		// Add more elememts
	}

};