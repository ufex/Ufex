#pragma once
#include "AssemblyInfo.cpp"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Reflection;
using namespace System::IO;


namespace UniversalFileExplorer
{
	/// <summary> 
	/// Summary for About
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	private ref class About : public System::Windows::Forms::Form
	{
	public: 
		About(void)
		{

			InitializeComponent();

			if(m_DebugMode)
				addLine(L"Debug Mode");

			AddFileVersions();
/*
			Version* v = Environment::get_Version();
			labelVersion->set_Text( v->ToString() );
			
			OperatingSystem* os = Environment::get_OSVersion();
			labelOSVersion->set_Text( os->ToString() );
		
			Int64 ws = Environment::get_WorkingSet();
			labelWorkingSet->set_Text( ws.ToString() );
*/

			/*
			addLine(String::Concat(S"ProductName: ", Application::ProductName));
			addLine(String::Concat(S"ProductVersion: ", Application::ProductVersion ));
			addLine(String::Concat(S"CompanyName: ", Application::CompanyName));
			addLine(String::Concat(S"CurrentCulture: ", Application::CurrentCulture));
			addLine(String::Concat(S"ExecutablePath: ", Application::ExecutablePath));
			addLine(String::Concat(S"CommonAppDataPath: ", Application::CommonAppDataPath));
			addLine(String::Concat(S"CommonAppDataRegistry: ", Application::CommonAppDataRegistry));
			addLine(String::Concat(S"UserAppDataPath: ", Application::UserAppDataPath));
			addLine(String::Concat(S"UserAppDataPath: ", Application::UserAppDataRegistry));
			*/
			//BOOL GetFileVersionInfo(          LPTSTR lptstrFilename,
//    DWORD dwHandle,
//    DWORD dwLen,
//    LPVOID lpData
//);

			//Assembly::Load(
			//System::Version::
		}
	protected: 
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~About()
		{
			if (components)
			{
				delete components;
			}
		}

	private: System::Windows::Forms::Button ^  buttonOK;
	private: System::Windows::Forms::TextBox ^  textAbout;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container^ components;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			System::Resources::ResourceManager ^  resources = gcnew System::Resources::ResourceManager(UniversalFileExplorer::About::typeid);
			this->buttonOK = gcnew System::Windows::Forms::Button();
			this->textAbout = gcnew System::Windows::Forms::TextBox();
			this->SuspendLayout();
			// 
			// buttonOK
			// 
			this->buttonOK->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->buttonOK->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->buttonOK->Location = System::Drawing::Point(296, 240);
			this->buttonOK->Name = L"buttonOK";
			this->buttonOK->TabIndex = 1;
			this->buttonOK->Text = L"OK";
			this->buttonOK->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::About::buttonOK_Click);
			// 
			// textAbout
			// 
			this->textAbout->Location = System::Drawing::Point(8, 8);
			this->textAbout->MaxLength = 65535;
			this->textAbout->Multiline = true;
			this->textAbout->Name = L"textAbout";
			this->textAbout->ReadOnly = true;
			this->textAbout->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->textAbout->Size = System::Drawing::Size(368, 224);
			this->textAbout->TabIndex = 7;
			this->textAbout->Text = L"";
			this->textAbout->WordWrap = false;
			// 
			// About
			// 
			this->AcceptButton = this->buttonOK;
			this->AutoScaleBaseSize = System::Drawing::Size(5, 13);
			this->ClientSize = System::Drawing::Size(384, 272);
			this->Controls->Add(this->textAbout);
			this->Controls->Add(this->buttonOK);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (static_cast<System::Drawing::Icon ^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"About";
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"About";
			this->ResumeLayout(false);

		}		



		public:	String^ m_BuildType;
				bool m_DebugMode;
				bool m_Eval;


	private: System::Void buttonOK_Click(System::Object ^  sender, System::EventArgs ^  e)
			 {
				 this->Close();
			 }

	private: void AddFileVersions()
			 {
				array<String^>^ DLL_Files = gcnew array<String^>(6);
				DLL_Files[0] = L"UfexGui.exe";
				DLL_Files[1] = L"UFEEngine.dll";
				DLL_Files[2] = L"UFEControls.dll";
				DLL_Files[3] = L"FileTypeManager.dll";
				DLL_Files[4] = L"UfexAPI.dll";
				DLL_Files[5] = L"UFEUtil.dll";

				for(int i = 0; i < DLL_Files->Length; i++)
				{
					Assembly^ x;
					String^ fullName = L"";
					try
					{
						x = Assembly::LoadFile(String::Concat(Path::GetDirectoryName(Application::ExecutablePath), L"\\", DLL_Files[i]));
						fullName = x->FullName;
						fullName = fullName->Substring(0, fullName->IndexOf(L", Culture"));
						addLine(String::Concat(fullName, L""));
						x = nullptr;
					}
					catch(Exception^)
					{

					}
				}

				addLine(L"");
				addLine(L"Modules:");

				array<String^>^ mod_Files = gcnew array<String^>(10);
				mod_Files[0] = L"BMP.dll";
				mod_Files[1] = L"GIF.dll";
				mod_Files[2] = L"JPEG.dll";
				mod_Files[3] = L"PNG.dll";
				mod_Files[4] = L"WMF.dll";
				mod_Files[5] = L"EXE.dll";
				mod_Files[6] = L"INI.dll";
				mod_Files[7] = L"OLEComp.dll";
				mod_Files[8] = L"ZIP.dll";
				mod_Files[9] = L"RIFF.dll";
						
				for(int i = 0; i < mod_Files->Length; i++)
				{
					Assembly^ x;
					String^ fullName = L"";
					try
					{
						x = Assembly::LoadFile(String::Concat(Path::GetDirectoryName(Application::ExecutablePath), L"\\modules\\", mod_Files[i]));
						fullName = x->FullName;
						fullName = fullName->Substring(0, fullName->IndexOf(L", Culture"));
						addLine(String::Concat(fullName, L""));
						x = nullptr;
					}
					catch(Exception^)
					{
					}
				}

			 }



	private: void addLine(String^ x)
			 {
				if(!textAbout->Text->Equals(L""))
					textAbout->Text = String::Concat(textAbout->Text, L"\r\n", x);
				else
					textAbout->Text = x;
			 }

	};
}