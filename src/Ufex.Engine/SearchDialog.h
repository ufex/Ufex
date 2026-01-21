#pragma once

#include ".\filestreamsearch.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace UniversalFileExplorer
{
	/// <summary> 
	/// Summary for SearchDialog
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class SearchDialog : public System::Windows::Forms::Form
	{
	public: 
		SearchDialog(void)
		{
			InitializeComponent();
		}
        
	protected: 
		~SearchDialog()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::ComboBox^  cmbSearchText;
	private: System::Windows::Forms::GroupBox^  groupBox1;
	private: System::Windows::Forms::Button^  btnFindNext;

	public: FileStreamSearch^ m_fileStreamSearch;
	public: Ufex::Controls::HexViewControl^  hexView;
	private: System::Windows::Forms::RadioButton^  radText;
	private: System::Windows::Forms::RadioButton^  radHex;
	private: System::Windows::Forms::Button^  btnCancel;

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
			this->cmbSearchText = gcnew System::Windows::Forms::ComboBox();
			this->groupBox1 = gcnew System::Windows::Forms::GroupBox();
			this->radHex = gcnew System::Windows::Forms::RadioButton();
			this->radText = gcnew System::Windows::Forms::RadioButton();
			this->btnFindNext = gcnew System::Windows::Forms::Button();
			this->btnCancel = gcnew System::Windows::Forms::Button();
			this->groupBox1->SuspendLayout();
			this->SuspendLayout();
			// 
			// cmbSearchText
			// 
			this->cmbSearchText->Location = System::Drawing::Point(16, 48);
			this->cmbSearchText->Name = L"cmbSearchText";
			this->cmbSearchText->Size = System::Drawing::Size(272, 21);
			this->cmbSearchText->TabIndex = 0;
			// 
			// groupBox1
			// 
			this->groupBox1->Controls->Add(this->radHex);
			this->groupBox1->Controls->Add(this->radText);
			this->groupBox1->Controls->Add(this->cmbSearchText);
			this->groupBox1->Location = System::Drawing::Point(8, 8);
			this->groupBox1->Name = L"groupBox1";
			this->groupBox1->Size = System::Drawing::Size(328, 128);
			this->groupBox1->TabIndex = 1;
			this->groupBox1->TabStop = false;
			this->groupBox1->Text = L"Find";
			// 
			// radHex
			// 
			this->radHex->Location = System::Drawing::Point(80, 24);
			this->radHex->Name = L"radHex";
			this->radHex->Size = System::Drawing::Size(48, 16);
			this->radHex->TabIndex = 2;
			this->radHex->Text = L"Hex";
			// 
			// radText
			// 
			this->radText->Location = System::Drawing::Point(16, 24);
			this->radText->Name = L"radText";
			this->radText->Size = System::Drawing::Size(48, 16);
			this->radText->TabIndex = 1;
			this->radText->Text = L"Text";
			// 
			// btnFindNext
			// 
			this->btnFindNext->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnFindNext->Location = System::Drawing::Point(8, 144);
			this->btnFindNext->Name = L"btnFindNext";
			this->btnFindNext->TabIndex = 2;
			this->btnFindNext->Text = L"Find...";
			this->btnFindNext->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::SearchDialog::btnFindNext_Click);
			// 
			// btnCancel
			// 
			this->btnCancel->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnCancel->Location = System::Drawing::Point(256, 144);
			this->btnCancel->Name = L"btnCancel";
			this->btnCancel->TabIndex = 3;
			this->btnCancel->Text = L"Cancel";
			this->btnCancel->Click += gcnew System::EventHandler(this, &UniversalFileExplorer::SearchDialog::btnCancel_Click);
			// 
			// SearchDialog
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(5, 13);
			this->ClientSize = System::Drawing::Size(344, 174);
			this->Controls->Add(this->btnCancel);
			this->Controls->Add(this->btnFindNext);
			this->Controls->Add(this->groupBox1);
			this->Name = L"SearchDialog";
			this->Text = L"Search...";
			this->groupBox1->ResumeLayout(false);
			this->ResumeLayout(false);

		}		
	private: System::Void btnFindNext_Click(System::Object^  sender, System::EventArgs^  e)
			 {
				 int len = cmbSearchText->Text->Length;

				 m_fileStreamSearch->NewSearch(cmbSearchText->Text);

				 Int64 pos = m_fileStreamSearch->FindNext();

				 if(pos == -1)
				 {

				 }
				 else
				 {
					hexView->GotoPosition(pos);
					hexView->Highlight(pos, pos + len);
				 }

			 }

	private: System::Void btnCancel_Click(System::Object^  sender, System::EventArgs^  e)
			 {
				 this->Close();
			 }

};
}