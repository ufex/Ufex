#pragma once

using namespace System;
using namespace System::Collections;

public ref struct InstallerEventArgs
{


};


public ref struct LoaderEventArgs
{
	IconCollection^ icons;


};

public ref class ModuleEvents abstract
{

public:

	virtual void Install(InstallerEventArgs^ e) = 0;

	virtual void Uninstall(InstallerEventArgs^ e) = 0;

	virtual void Load(LoaderEventArgs^ e) = 0;

	virtual void Unload(LoaderEventArgs^ e) = 0;


private:


};
