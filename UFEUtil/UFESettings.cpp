#include "stdafx.h"

#include "./UFESettings.h"

namespace UniversalFileExplorer
{

	/*********************************************************************
	**	Settings()
	**
	**	Date Modified:
	**
	**	Description:  Initializes the ArrayList of settings.
	**
	**	Inputs: None
	**
	**	Ouputs: None
	**
	**********************************************************************/
	Settings::Settings()
	{
		m_Settings = gcnew ArrayList();
	}

	/*********************************************************************
	**	Settings(String* filePath)
	**
	**	Date Modified:
	**
	**	Description: Initializes the ArrayList of settings and sets 
	**					the file path.
	**
	**	Inputs: filePath - the path to the settings file
	**
	**	Ouputs:
	**
	**	Returns:
	**
	**********************************************************************/
	Settings::Settings(String^ filePath)
	{
		m_FilePath = filePath;
		m_Settings = gcnew ArrayList();
	}

	/*********************************************************************
	**	ReadSettings
	**
	**	Date Modified:
	**
	**	Description:
	**
	**	Inputs: None
	**
	**	Ouputs: None
	**
	**	Returns: -1 if an error occured
	**
	**********************************************************************/
	int Settings::ReadSettings()
	{
		XmlTextReader^ xtr;
		try
		{
			xtr = gcnew XmlTextReader(m_FilePath);
		}
		catch(Exception^)
		{
			return -1;
		}

		// Read the beginning of the xml file
		xtr->Read(); // "xml"
		xtr->Read(); // ""
		xtr->Read(); // "UFESettings"
		xtr->Read(); // ""

		xtr->Read(); // "Section"

		while(xtr->Name->Equals("Section"))
		{
			SECTION^ newSection = gcnew SECTION;
			newSection->name = xtr->GetAttribute("Name");
			newSection->settings = gcnew ArrayList();
			
			// Read Whitespace
			xtr->Read();	
			xtr->Read();

			// Read all the settings in the section
			while(!xtr->Name->Equals("Section"))
			{	
				SETTING^ newSetting = gcnew SETTING;
				newSetting->property = xtr->Name;
				
				xtr->Read();
				newSetting->value = xtr->Value;
				xtr->Read();
				XmlAssert((xtr->NodeType == XmlNodeType::EndElement), "Failed to read end node for setting");

				// Add the setting to the section
				newSection->settings->Add(newSetting);

				// Read EndNode
				xtr->Read();
				xtr->Read();

			}
			
			m_Settings->Add(newSection);
			
			xtr->Read();			
			xtr->Read();
		}

		xtr->Close();

		return 0;
	}

	int Settings::WriteSettings()
	{
		XmlTextWriter^ xtw;
		
		try
		{
			xtw = gcnew XmlTextWriter(m_FilePath, Encoding::UTF8);
			xtw->Formatting = Formatting::Indented;
		}
		catch(Exception^)
		{
			return -1;
		}
		
		xtw->WriteStartDocument();

		xtw->WriteStartElement("UFESettings");
		

		// Go through all the settings
		for(int i = 0; i < m_Settings->Count; i++)
		{
			SECTION^ pSection = static_cast<SECTION^>(m_Settings[i]);
			xtw->WriteStartElement("Section");
			xtw->WriteAttributeString("Name", pSection->name);
			for(int j = 0; j < pSection->settings->Count; j++)
			{
				SETTING^ pSetting = static_cast<SETTING^>(pSection->settings[j]);
				xtw->WriteElementString(pSetting->property, pSetting->value);
			}
			xtw->WriteEndElement();
		}

		
		// Close the root element
		xtw->WriteEndElement();
		xtw->WriteEndDocument();

		// Write the file
		xtw->Flush();
		xtw->Close();
		return 0;
	}

	/*********************************************************************
	**	GetSetting
	**
	**	Date Modified: 07/13/05
	**
	**	Description: Returns the value of the specified setting
	**
	**	Inputs: section - the name of the section
	**			property - the name of the property
	**
	**	Ouputs:
	**
	**	Returns: returns the value of the property or NULL if the property
	**				doesn't exist
	**********************************************************************/
	String^ Settings::GetSetting(String^ section, String^ property)
	{
		// Search for the Section
		int sectionIndex = GetSectionIndex(section);

		if(sectionIndex == -1)
			return nullptr;

		int propertyIndex = GetPropertyIndex(property, sectionIndex);
		
		if(propertyIndex == -1)
			return nullptr;
		
		SECTION^ sectionObj = static_cast<SECTION^>(m_Settings[sectionIndex]);

		return (static_cast<SETTING^>(sectionObj->settings[propertyIndex]))->value;
	}		

	int Settings::ChangeSetting(String^ section, String^ property, String^ value)
	{
		// Search for the Section
		int sectionIndex = GetSectionIndex(section);

		if(sectionIndex == -1)
			return -1;

		int propertyIndex = GetPropertyIndex(property, sectionIndex);
		
		if(propertyIndex == -1)
			return -1;
		
		SECTION^ pSection = static_cast<SECTION^>(m_Settings[sectionIndex]);
		SETTING^ pSetting = static_cast<SETTING^>(pSection->settings[propertyIndex]);
		
		pSetting->value = value;

		return 0;
	}

	int Settings::AddSection(String^ section)
	{
		// Make sure the section doesn't already exist
		int sectionIndex = GetSectionIndex(section);
		
		if(sectionIndex == -1)
			AddSectionNoCheck(section);
		else
			return -1;

		return 0;
	}

	int Settings::AddSetting(String^ section, String^ property, String^ value)	
	{
		// Search for the Section
		int sectionIndex = GetSectionIndex(section);

		if(sectionIndex == -1)
			sectionIndex = AddSectionNoCheck(section);

		SECTION^ pSection = static_cast<SECTION^>(m_Settings[sectionIndex]);
		
		int propertyIndex = GetPropertyIndex(property, sectionIndex);
		
		if(propertyIndex != -1)
			return -1;
		else
		{
			SETTING^ newSetting = gcnew SETTING;
			newSetting->property = property;
			newSetting->value = value;
			pSection->settings->Add(newSetting);
		}

		

		return -1;
	}

	int Settings::DeleteSetting(String^ section, String^ property)
	{
		// Search for the Section
		int sectionIndex = GetSectionIndex(section);

		if(sectionIndex == -1)
			return -1;

		int propertyIndex = GetPropertyIndex(property, sectionIndex);
		
		if(propertyIndex == -1)
			return -1;
		
		try
		{
			SECTION^ pSection = static_cast<SECTION^>(m_Settings[sectionIndex]);
			pSection->settings->RemoveAt(propertyIndex);
		}
		catch(Exception^)
		{
			return -2;
		}


		return 0;
	}
		
	int Settings::AddSectionNoCheck(String^ section)
	{
		SECTION^ newSection = gcnew SECTION;

		newSection->name = section;
		newSection->settings = gcnew ArrayList();
		//m_Settings->Add(newSection);

		return m_Settings->Add(newSection);
	}

	int Settings::GetSectionIndex(String^ sectionName)
	{
		// Search for the Section
		for(int i = 0; i < m_Settings->Count; i++)
		{
			if((static_cast<SECTION^>(m_Settings[i]))->name->Equals(sectionName))
				return i;
		}
		return -1;
	}
	
	int Settings::GetPropertyIndex(String^ propertyName, int sectionIndex)
	{
		SECTION^ sectionObj = static_cast<SECTION^>(m_Settings[sectionIndex]);
		ArrayList^ sectSettings = sectionObj->settings;

		// Search the section looking for the specified setting
		for(int i = 0; i < sectSettings->Count; i++)
		{
			if((static_cast<SETTING^>(sectSettings[i]))->property->Equals(propertyName))
				return i;
		}
		return -1;
	}



	void Settings::XmlAssert(bool expression, String^ description)
	{
		if(!expression)
		{
			m_ErrorLog = String::Concat(m_ErrorLog, description, "\r\n");
		}
	}

};