#include "stdafx.h"
#include ".\UniversalFileExplorerApp.h"
#using <mscorlib.dll>

#ifdef _DEBUG
	#define BUILD_TYPE	L"Debug"
#else
	#define BUILD_TYPE	L"Release"
#endif


namespace UniversalFileExplorer
{
	UniversalFileExplorerApp::UniversalFileExplorerApp(void)
	{
		m_BuildType = BUILD_TYPE;
		m_LoadDialog = nullptr;
		m_ExecutablePath = Application::ExecutablePath;
		m_ApplicationData = Environment::GetFolderPath(Environment::SpecialFolder::ApplicationData);
		m_LocalApplicationData = Environment::GetFolderPath(Environment::SpecialFolder::LocalApplicationData);
		
		m_AppPath = Path::GetDirectoryName(Application::ExecutablePath);

		m_Settings = gcnew Settings(String::Concat(m_AppPath, L"\\UFE_Settings.xml"));

		try
		{
			m_Settings->ReadSettings();
		}
		catch(Exception^ e)
		{
			MessageBox::Show(String::Concat(L"Error reading settings file: ", e->Message), L"Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
		}
	}

	UniversalFileExplorerApp::~UniversalFileExplorerApp(void)
	{
	}

	int UniversalFileExplorerApp::PreRun()
	{
		return 0;
	}

	void UniversalFileExplorerApp::ShowLoadDialog()
	{
		m_LoadDialog = gcnew LoadingDialog();
		m_LoadDialog->Show();
	}

	void UniversalFileExplorerApp::CloseLoadDialog()
	{
		if(m_LoadDialog != nullptr)
			m_LoadDialog->CloseDialog();
	}

};