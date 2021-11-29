#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Globalization;

//using namespace UniversalFileExplorer;

namespace UniversalFileExplorer
{
	/// <summary> 
	/// Summary for FTMForm
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class FTMForm : public System::Windows::Forms::Form
	{
	private:
		//Instance of the FileTypeManager
		FileTypeDb^ m_FileTypesDB;

		// Contains the FILETYPE objects that were loaded from the XML file
		array<FILETYPE^>^ m_FileTypes;
		
		// Contains types that were added by the user
		ArrayList^ m_NewTypes;
		
		// Contains the FILETYPE object of the currently selected file type
		FILETYPE^ m_CurFileType;
		
		// Index of the type that is currently selected
		int m_CurTypeIndex;
		
		// Set to true if the currently selected type has been modified
		bool m_CurTypeMod;

	private: System::Windows::Forms::Label ^ label1;
	private: System::Windows::Forms::Label ^  label2;
	private: System::Windows::Forms::GroupBox ^  groupBoxProperties;
	private: System::Windows::Forms::TextBox ^  txtTypeID;
	private: System::Windows::Forms::TextBox ^  txtTypeDesc;
	private: System::Windows::Forms::Button ^  buttonNewType;
	private: System::Windows::Forms::Button ^  buttonOK;
	private: System::Windows::Forms::Button ^  buttonCancel;
	private: System::Windows::Forms::Label ^  label6;
	private: System::Windows::Forms::ComboBox ^  cmbGenType;
	private: System::Windows::Forms::Label ^  label3;
	private: System::Windows::Forms::TextBox ^  txtMIMEType;
	private: System::Windows::Forms::Label ^  lblBind;
	private: System::Windows::Forms::ComboBox ^  cmbBind;
	private: System::Windows::Forms::Button ^  btnDeleteType;
	private: System::Windows::Forms::Label ^  label4;
	private: System::Windows::Forms::ComboBox ^  cmbFileTypeClassID;
	private: System::Windows::Forms::ListBox ^  lstFileTypes;

	public: 
		FTMForm(void)
		{
			InitializeComponent();
		}

		FTMForm(FileTypeDb^ ftdb)
		{
			
			InitializeComponent();
			
			// Create an array to hold gcnew types
			m_NewTypes = gcnew ArrayList();
			
			m_FileTypesDB = ftdb;

			m_FileTypes = m_FileTypesDB->GetFileTypes();


			if(m_FileTypes != nullptr)
			{
				for(int i = 0; i < m_FileTypes->Length; i++)
				{
					lstFileTypes->Items->Add(m_FileTypes[i]->id);
				}
			}
			m_CurTypeMod = false;
		}
        
	protected: 
		~FTMForm()
		{
			if (components)
			{
				delete components;
			}
		}


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
			System::Resources::ResourceManager ^  resources = gcnew System::Resources::ResourceManager(UniversalFileExplorer::FTMForm::typeid);
			this->buttonOK = gcnew System::Windows::Forms::Button();
			this->buttonCancel = gcnew System::Windows::Forms::Button();
			this->lstFileTypes = gcnew System::Windows::Forms::ListBox();
			this->groupBoxProperties = gcnew System::Windows::Forms::GroupBox();
			this->label4 = gcnew System::Windows::Forms::Label();
			this->cmbFileTypeClassID = gcnew System::Windows::Forms::ComboBox();
			this->lblBind = gcnew System::Windows::Forms::Label();
			this->cmbBind = gcnew System::Windows::Forms::ComboBox();
			this->txtMIMEType = gcnew System::Windows::Forms::TextBox();
			this->label3 = gcnew System::Windows::Forms::Label();
			this->cmbGenType = gcnew System::Windows::Forms::ComboBox();
			this->label6 = gcnew System::Windows::Forms::Label();
			this->label2 = gcnew System::Windows::Forms::Label();
			this->label1 = gcnew System::Windows::Forms::Label();
			this->txtTypeDesc = gcnew System::Windows::Forms::TextBox();
			this->txtTypeID = gcnew System::Windows::Forms::TextBox();
			this->buttonNewType = gcnew System::Windows::Forms::Button();
			this->btnDeleteType = gcnew System::Windows::Forms::Button();
			this->groupBoxProperties->SuspendLayout();
			this->SuspendLayout();
			// 
			// buttonOK
			// 
			this->buttonOK->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->buttonOK->Location = System::Drawing::Point(376, 224);
			this->buttonOK->Name = L"buttonOK";
			this->buttonOK->TabIndex = 0;
			this->buttonOK->Text = L"OK";
			this->buttonOK->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::buttonOK_Click);
			// 
			// buttonCancel
			// 
			this->buttonCancel->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->buttonCancel->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->buttonCancel->Location = System::Drawing::Point(464, 224);
			this->buttonCancel->Name = L"buttonCancel";
			this->buttonCancel->TabIndex = 1;
			this->buttonCancel->Text = L"Cancel";
			this->buttonCancel->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::buttonCancel_Click);
			// 
			// lstFileTypes
			// 
			this->lstFileTypes->Location = System::Drawing::Point(16, 16);
			this->lstFileTypes->Name = L"lstFileTypes";
			this->lstFileTypes->Size = System::Drawing::Size(240, 199);
			this->lstFileTypes->TabIndex = 3;
			this->lstFileTypes->SelectedIndexChanged += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::lstFileTypes_SelectedIndexChanged);
			// 
			// groupBoxProperties
			// 
			this->groupBoxProperties->Controls->Add(this->label4);
			this->groupBoxProperties->Controls->Add(this->cmbFileTypeClassID);
			this->groupBoxProperties->Controls->Add(this->lblBind);
			this->groupBoxProperties->Controls->Add(this->cmbBind);
			this->groupBoxProperties->Controls->Add(this->txtMIMEType);
			this->groupBoxProperties->Controls->Add(this->label3);
			this->groupBoxProperties->Controls->Add(this->cmbGenType);
			this->groupBoxProperties->Controls->Add(this->label6);
			this->groupBoxProperties->Controls->Add(this->label2);
			this->groupBoxProperties->Controls->Add(this->label1);
			this->groupBoxProperties->Controls->Add(this->txtTypeDesc);
			this->groupBoxProperties->Controls->Add(this->txtTypeID);
			this->groupBoxProperties->Location = System::Drawing::Point(272, 16);
			this->groupBoxProperties->Name = L"groupBoxProperties";
			this->groupBoxProperties->Size = System::Drawing::Size(272, 200);
			this->groupBoxProperties->TabIndex = 3;
			this->groupBoxProperties->TabStop = false;
			this->groupBoxProperties->Text = L"File Type Properties";
			// 
			// label4
			// 
			this->label4->Location = System::Drawing::Point(8, 144);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(80, 16);
			this->label4->TabIndex = 18;
			this->label4->Text = L"Class ID:";
			// 
			// cmbFileTypeClassID
			// 
			this->cmbFileTypeClassID->Location = System::Drawing::Point(96, 144);
			this->cmbFileTypeClassID->Name = L"cmbFileTypeClassID";
			this->cmbFileTypeClassID->Size = System::Drawing::Size(160, 21);
			this->cmbFileTypeClassID->TabIndex = 17;
			// 
			// lblBind
			// 
			this->lblBind->Location = System::Drawing::Point(8, 118);
			this->lblBind->Name = L"lblBind";
			this->lblBind->Size = System::Drawing::Size(80, 16);
			this->lblBind->TabIndex = 16;
			this->lblBind->Text = L"Bind:";
			// 
			// cmbBind
			// 
			this->cmbBind->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			array<System::Object^>^ __mcTemp__1 = gcnew array<System::Object^>(2);
			__mcTemp__1[0] = L"True";
			__mcTemp__1[1] = L"False";
			this->cmbBind->Items->AddRange(__mcTemp__1);
			this->cmbBind->Location = System::Drawing::Point(96, 118);
			this->cmbBind->Name = L"cmbBind";
			this->cmbBind->Size = System::Drawing::Size(160, 21);
			this->cmbBind->TabIndex = 15;
			this->cmbBind->SelectedIndexChanged += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::cmbBind_SelectedIndexChanged);
			// 
			// txtMIMEType
			// 
			this->txtMIMEType->Location = System::Drawing::Point(96, 94);
			this->txtMIMEType->Name = L"txtMIMEType";
			this->txtMIMEType->Size = System::Drawing::Size(160, 20);
			this->txtMIMEType->TabIndex = 14;
			this->txtMIMEType->Text = L"";
			// 
			// label3
			// 
			this->label3->Location = System::Drawing::Point(8, 94);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(80, 16);
			this->label3->TabIndex = 13;
			this->label3->Text = L"MIME Type:";
			// 
			// cmbGenType
			// 
			array<System::Object^>^ __mcTemp__2 = gcnew array<System::Object^>(9);
			__mcTemp__2[0] = L"Archive";
			__mcTemp__2[1] = L"Audio";
			__mcTemp__2[2] = L"Database";
			__mcTemp__2[3] = L"Document";
			__mcTemp__2[4] = L"Executable";
			__mcTemp__2[5] = L"Graphic";
			__mcTemp__2[6] = L"Source Code";
			__mcTemp__2[7] = L"Spreadsheet";
			__mcTemp__2[8] = L"Text";
			this->cmbGenType->Items->AddRange(__mcTemp__2);
			this->cmbGenType->Location = System::Drawing::Point(96, 70);
			this->cmbGenType->Name = L"cmbGenType";
			this->cmbGenType->Size = System::Drawing::Size(160, 21);
			this->cmbGenType->TabIndex = 12;
			// 
			// label6
			// 
			this->label6->Location = System::Drawing::Point(8, 70);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(80, 16);
			this->label6->TabIndex = 10;
			this->label6->Text = L"General Type:";
			// 
			// label2
			// 
			this->label2->Location = System::Drawing::Point(8, 46);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(80, 16);
			this->label2->TabIndex = 5;
			this->label2->Text = L"Description:";
			// 
			// label1
			// 
			this->label1->Location = System::Drawing::Point(8, 22);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(80, 16);
			this->label1->TabIndex = 4;
			this->label1->Text = L"Type ID:";
			// 
			// txtTypeDesc
			// 
			this->txtTypeDesc->Location = System::Drawing::Point(96, 46);
			this->txtTypeDesc->Name = L"txtTypeDesc";
			this->txtTypeDesc->Size = System::Drawing::Size(160, 20);
			this->txtTypeDesc->TabIndex = 2;
			this->txtTypeDesc->Text = L"";
			this->txtTypeDesc->TextChanged += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::FieldsChanged);
			// 
			// txtTypeID
			// 
			this->txtTypeID->Location = System::Drawing::Point(96, 22);
			this->txtTypeID->Name = L"txtTypeID";
			this->txtTypeID->Size = System::Drawing::Size(160, 20);
			this->txtTypeID->TabIndex = 1;
			this->txtTypeID->Text = L"";
			this->txtTypeID->TextChanged += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::FieldsChanged);
			// 
			// buttonNewType
			// 
			this->buttonNewType->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->buttonNewType->Location = System::Drawing::Point(16, 224);
			this->buttonNewType->Name = L"buttonNewType";
			this->buttonNewType->TabIndex = 2;
			this->buttonNewType->Text = L"New Type...";
			this->buttonNewType->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::FTMForm::buttonNewType_Click);
			// 
			// btnDeleteType
			// 
			this->btnDeleteType->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnDeleteType->Location = System::Drawing::Point(96, 224);
			this->btnDeleteType->Name = L"btnDeleteType";
			this->btnDeleteType->TabIndex = 4;
			this->btnDeleteType->Text = L"Delete";
			// 
			// FTMForm
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(5, 13);
			this->CancelButton = this->buttonCancel;
			this->ClientSize = System::Drawing::Size(554, 256);
			this->Controls->Add(this->btnDeleteType);
			this->Controls->Add(this->buttonNewType);
			this->Controls->Add(this->groupBoxProperties);
			this->Controls->Add(this->lstFileTypes);
			this->Controls->Add(this->buttonCancel);
			this->Controls->Add(this->buttonOK);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedSingle;
			this->Icon = (static_cast<System::Drawing::Icon ^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"FTMForm";
			this->ShowInTaskbar = false;
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"File Type Manager";
			this->groupBoxProperties->ResumeLayout(false);
			this->ResumeLayout(false);

		}		
	private: System::Void lstFileTypes_SelectedIndexChanged(System::Object ^  sender, System::EventArgs ^  e)
			 {				 
				// Record the last type if it was edited
				if(m_CurTypeMod)
				{
					m_CurFileType->update = true;
					m_CurFileType->description = txtTypeDesc->Text;
					m_CurFileType->id = txtTypeID->Text;

					if(cmbBind->Text->Equals(L"True"))
						m_CurFileType->bind = true;
					else if(cmbBind->Text->Equals(L"False"))
						m_CurFileType->bind = false;

					if(m_CurFileType->bind)
						m_CurFileType->fileTypeClassId = cmbFileTypeClassID->Text;
					else
						m_CurFileType->fileTypeClassId = L"";

					//m_FileTypes[m_CurTypeIndex] = m_CurFileType;
					if(m_CurTypeIndex < m_FileTypes->Length)
					{
						m_FileTypes[m_CurTypeIndex] = m_CurFileType;
					}
					else
					{
						m_NewTypes[m_CurTypeIndex - m_FileTypes->Length] = m_CurFileType;
					}
					//bool success = m_FileTypeMan->SetFileType(m_CurTypeIndex, m_CurFileType);
					//if(!success)
					//{
					//	// Display an error message
					//	MessageBox::Show(m_FileTypeMan->m_ErrorMsg, L"Error in File Type Data", MessageBoxButtons::OK, MessageBoxIcon::Error);
					//}
				}
				if(lstFileTypes->SelectedIndex < m_FileTypes->Length)
				{
					m_CurFileType = m_FileTypes[lstFileTypes->SelectedIndex];
				}
				else
				{
					int i = lstFileTypes->SelectedIndex - m_FileTypes->Length;
					m_CurFileType = static_cast<FILETYPE^>(m_NewTypes[i]);
				}
				m_CurTypeIndex = lstFileTypes->SelectedIndex;
				txtTypeID->Text = m_CurFileType->id;
				txtTypeDesc->Text = m_CurFileType->description;
				cmbBind->Text = m_CurFileType->bind.ToString();
				txtMIMEType->Text = m_CurFileType->mimeType;
				cmbFileTypeClassID->Text = m_CurFileType->fileTypeClassId;

				m_CurTypeMod = false;
				
			 }

	private: System::Void buttonCancel_Click(System::Object ^  sender, System::EventArgs ^  e)
			 {				
				 this->Close();
			 }

private: System::Void buttonNewType_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {

		 }

private: System::Void FieldsChanged(System::Object ^  sender, System::EventArgs ^  e)
		 {
			 m_CurTypeMod = true;
		 }
		
		 // Save changes to file type manager and Write the XML
private: System::Void buttonOK_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {
			this->Close();
		 }

private: System::Void cmbBind_SelectedIndexChanged(System::Object ^  sender, System::EventArgs ^  e)
		 {
			if(cmbBind->Text->Equals(L"True"))
				cmbFileTypeClassID->Enabled = true;
			else if(cmbBind->Text->Equals(L"False"))
				cmbFileTypeClassID->Enabled = false;
		 }

	};

};  // End of namespace