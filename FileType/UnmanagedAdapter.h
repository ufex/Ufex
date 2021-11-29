#pragma once

ref class UnmanagedAdapter : public FileType
{
public:
	UnmanagedAdapter(void);
	~UnmanagedAdapter(void);


	bool ProcessFile();
	TableData^ GetData(TreeNode^ tn);
	TableData^ GetQuickInfo();
	Image^ GetImage();

private:



};
