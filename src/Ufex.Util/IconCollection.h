#pragma once
using namespace System;
using namespace System::Collections;
using namespace System::Drawing;
using namespace System::Windows::Forms;

public ref class IconCollection :
	public CollectionBase 
{

public:

	IconCollection(void);
	~IconCollection(void);

	// Adds the icon to the collection
	virtual int Add(Icon^ newIcon) { return m_icons->Add(newIcon); };

private:
	ArrayList^ m_icons;

};
