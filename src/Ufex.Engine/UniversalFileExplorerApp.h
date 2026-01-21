#pragma once
#include "./Form1.h"
#include "./LoadingDialog.h"

namespace UniversalFileExplorer
{
	public ref class UniversalFileExplorerApp
	{
	public:
		UniversalFileExplorerApp(void);
		~UniversalFileExplorerApp(void);

		int PreRun();

		String^ gBT() { return m_BuildType; };

		Form1^ mainForm;

		Settings^ m_Settings;
		
		void ShowLoadDialog();
		void CloseLoadDialog();


	private:
		LoadingDialog^ m_LoadDialog;

		String^ m_AppPath;

		String^ m_ExecutablePath;
		String^ m_LocalApplicationData;
		String^ m_ApplicationData;
		String^ m_BuildType;

	};
};