// UFESettings.h

#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Xml;
using namespace System::Text;

namespace UniversalFileExplorer
{

	ref struct SETTING
	{
		String^ property;
		String^ value;
	};

	ref struct SECTION
	{
		String^ name;
		ArrayList^ settings;
	};


	public ref class Settings
	{
	public:
		Settings();
		Settings(String^ filePath);

		int ReadSettings();
		int WriteSettings();

		static String^ GetSetting(String^ section, String^ property);
		static int ChangeSetting(String^ section, String^ property, String^ value);
		static int AddSection(String^ section);
		static int AddSetting(String^ section, String^ property, String^ value);
		static int DeleteSetting(String^ section, String^ property);


	private:

		static int AddSectionNoCheck(String^ section);

		static int GetSectionIndex(String^ sectionName);
		static int GetPropertyIndex(String^ propertyName, int sectionIndex);

		//int SetProperty(int sectionIndex, int propertyIndex);



		// Error Log
		static void XmlAssert(bool expression, String^ description);
		
		
		static String^ m_ErrorLog;


		static String^ m_FilePath;

		// An ArrayList of Sections
		static ArrayList^ m_Settings;

	};
}
