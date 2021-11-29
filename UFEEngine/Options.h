#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

namespace UniversalFileExplorer
{
	/// <summary> 
	/// Summary for Options
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	private ref class Options : public System::Windows::Forms::Form
	{
	public: 
		Options()
		{
			InitializeComponent();
		}
        
	protected: 
		~Options()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Button ^  btnOK;
	private: System::Windows::Forms::Button ^  btnCancel;
	private: System::Windows::Forms::TabControl ^  tabControl;
	private: System::Windows::Forms::TabPage ^  tabPageGeneral;
	private: System::Windows::Forms::TabPage ^  tabPageTechView;
	private: System::Windows::Forms::TabPage ^  tabPageHexView;
	private: System::Windows::Forms::Label ^  label1;
	private: System::Windows::Forms::ComboBox ^  cmbDefNumFormat;
	private: System::Windows::Forms::ComboBox ^  cmbHexViewFont;
	private: System::Windows::Forms::Label ^  label2;




	private: System::Windows::Forms::Button ^  btnApply;
	private: System::Windows::Forms::ColorDialog ^  colorDialog;
	private: System::Windows::Forms::FontDialog ^  fontDialog;
	private: System::Windows::Forms::GroupBox ^  groupBox1;
	private: System::Windows::Forms::CheckBox ^  checkBox1;
	private: System::Windows::Forms::CheckBox ^  checkBox2;
	private: System::Windows::Forms::TabPage ^  tabPageFileTypeDB;
	private: System::Windows::Forms::GroupBox ^  groupBox2;
	private: System::Windows::Forms::ComboBox ^  comboBox1;
	private: System::Windows::Forms::Label ^  label3;
	private: System::Windows::Forms::TextBox ^  txtDataSource;
	private: System::Windows::Forms::Label ^  label4;



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
			this->btnOK = gcnew System::Windows::Forms::Button();
			this->btnCancel = gcnew System::Windows::Forms::Button();
			this->tabControl = gcnew System::Windows::Forms::TabControl();
			this->tabPageGeneral = gcnew System::Windows::Forms::TabPage();
			this->tabPageTechView = gcnew System::Windows::Forms::TabPage();
			this->groupBox1 = gcnew System::Windows::Forms::GroupBox();
			this->checkBox2 = gcnew System::Windows::Forms::CheckBox();
			this->checkBox1 = gcnew System::Windows::Forms::CheckBox();
			this->label1 = gcnew System::Windows::Forms::Label();
			this->cmbDefNumFormat = gcnew System::Windows::Forms::ComboBox();
			this->tabPageHexView = gcnew System::Windows::Forms::TabPage();
			this->label2 = gcnew System::Windows::Forms::Label();
			this->cmbHexViewFont = gcnew System::Windows::Forms::ComboBox();
			this->colorDialog = gcnew System::Windows::Forms::ColorDialog();
			this->btnApply = gcnew System::Windows::Forms::Button();
			this->fontDialog = gcnew System::Windows::Forms::FontDialog();
			this->tabPageFileTypeDB = gcnew System::Windows::Forms::TabPage();
			this->groupBox2 = gcnew System::Windows::Forms::GroupBox();
			this->comboBox1 = gcnew System::Windows::Forms::ComboBox();
			this->label3 = gcnew System::Windows::Forms::Label();
			this->txtDataSource = gcnew System::Windows::Forms::TextBox();
			this->label4 = gcnew System::Windows::Forms::Label();
			this->tabControl->SuspendLayout();
			this->tabPageTechView->SuspendLayout();
			this->groupBox1->SuspendLayout();
			this->tabPageHexView->SuspendLayout();
			this->tabPageFileTypeDB->SuspendLayout();
			this->groupBox2->SuspendLayout();
			this->SuspendLayout();
			// 
			// btnOK
			// 
			this->btnOK->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->btnOK->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnOK->Location = System::Drawing::Point(288, 304);
			this->btnOK->Name = L"btnOK";
			this->btnOK->TabIndex = 0;
			this->btnOK->Text = L"OK";
			// 
			// btnCancel
			// 
			this->btnCancel->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->btnCancel->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnCancel->Location = System::Drawing::Point(368, 304);
			this->btnCancel->Name = L"btnCancel";
			this->btnCancel->TabIndex = 1;
			this->btnCancel->Text = L"Cancel";
			// 
			// tabControl
			// 
			this->tabControl->Controls->Add(this->tabPageGeneral);
			this->tabControl->Controls->Add(this->tabPageTechView);
			this->tabControl->Controls->Add(this->tabPageHexView);
			this->tabControl->Controls->Add(this->tabPageFileTypeDB);
			this->tabControl->Location = System::Drawing::Point(9, 9);
			this->tabControl->Name = L"tabControl";
			this->tabControl->SelectedIndex = 0;
			this->tabControl->Size = System::Drawing::Size(512, 288);
			this->tabControl->TabIndex = 2;
			// 
			// tabPageGeneral
			// 
			this->tabPageGeneral->Location = System::Drawing::Point(4, 22);
			this->tabPageGeneral->Name = L"tabPageGeneral";
			this->tabPageGeneral->Size = System::Drawing::Size(504, 262);
			this->tabPageGeneral->TabIndex = 0;
			this->tabPageGeneral->Text = L"General";
			// 
			// tabPageTechView
			// 
			this->tabPageTechView->Controls->Add(this->groupBox1);
			this->tabPageTechView->Location = System::Drawing::Point(4, 22);
			this->tabPageTechView->Name = L"tabPageTechView";
			this->tabPageTechView->Size = System::Drawing::Size(504, 262);
			this->tabPageTechView->TabIndex = 1;
			this->tabPageTechView->Text = L"Technical View";
			// 
			// groupBox1
			// 
			this->groupBox1->Controls->Add(this->checkBox2);
			this->groupBox1->Controls->Add(this->checkBox1);
			this->groupBox1->Controls->Add(this->label1);
			this->groupBox1->Controls->Add(this->cmbDefNumFormat);
			this->groupBox1->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->groupBox1->Location = System::Drawing::Point(8, 8);
			this->groupBox1->Name = L"groupBox1";
			this->groupBox1->Size = System::Drawing::Size(312, 200);
			this->groupBox1->TabIndex = 5;
			this->groupBox1->TabStop = false;
			this->groupBox1->Text = L"Number Format";
			// 
			// checkBox2
			// 
			this->checkBox2->Location = System::Drawing::Point(8, 72);
			this->checkBox2->Name = L"checkBox2";
			this->checkBox2->Size = System::Drawing::Size(248, 16);
			this->checkBox2->TabIndex = 6;
			this->checkBox2->Text = L"Show leading 0\'s for hexidecimal numbers";
			// 
			// checkBox1
			// 
			this->checkBox1->Location = System::Drawing::Point(8, 48);
			this->checkBox1->Name = L"checkBox1";
			this->checkBox1->Size = System::Drawing::Size(248, 16);
			this->checkBox1->TabIndex = 5;
			this->checkBox1->Text = L"Prefix hexadecimal numbers with \"0x\"";
			// 
			// label1
			// 
			this->label1->Location = System::Drawing::Point(8, 16);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(128, 16);
			this->label1->TabIndex = 3;
			this->label1->Text = L"Default Number Format:";
			// 
			// cmbDefNumFormat
			// 
			array<System::Object^>^ __mcTemp__1 = gcnew array<System::Object^>(4);
			__mcTemp__1[0] = L"Hexadecimal";
			__mcTemp__1[1] = L"Decimal";
			__mcTemp__1[2] = L"Binary";
			__mcTemp__1[3] = L"ASCII";
			this->cmbDefNumFormat->Items->AddRange(__mcTemp__1);
			this->cmbDefNumFormat->Location = System::Drawing::Point(136, 16);
			this->cmbDefNumFormat->MaxDropDownItems = 4;
			this->cmbDefNumFormat->Name = L"cmbDefNumFormat";
			this->cmbDefNumFormat->Size = System::Drawing::Size(168, 21);
			this->cmbDefNumFormat->TabIndex = 4;
			// 
			// tabPageHexView
			// 
			this->tabPageHexView->Controls->Add(this->label2);
			this->tabPageHexView->Controls->Add(this->cmbHexViewFont);
			this->tabPageHexView->Location = System::Drawing::Point(4, 22);
			this->tabPageHexView->Name = L"tabPageHexView";
			this->tabPageHexView->Size = System::Drawing::Size(504, 262);
			this->tabPageHexView->TabIndex = 2;
			this->tabPageHexView->Text = L"Hex View";
			// 
			// label2
			// 
			this->label2->Location = System::Drawing::Point(8, 16);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(40, 16);
			this->label2->TabIndex = 1;
			this->label2->Text = L"Font:";
			// 
			// cmbHexViewFont
			// 
			array<System::Object^>^ __mcTemp__2 = gcnew array<System::Object^>(2);
			__mcTemp__2[0] = L"Courier New";
			__mcTemp__2[1] = L"MS Sans Serif";
			this->cmbHexViewFont->Items->AddRange(__mcTemp__2);
			this->cmbHexViewFont->Location = System::Drawing::Point(56, 16);
			this->cmbHexViewFont->Name = L"cmbHexViewFont";
			this->cmbHexViewFont->Size = System::Drawing::Size(176, 21);
			this->cmbHexViewFont->TabIndex = 0;
			// 
			// btnApply
			// 
			this->btnApply->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnApply->Location = System::Drawing::Point(448, 304);
			this->btnApply->Name = L"btnApply";
			this->btnApply->TabIndex = 3;
			this->btnApply->Text = L"Apply";
			// 
			// tabPageFileTypeDB
			// 
			this->tabPageFileTypeDB->Controls->Add(this->groupBox2);
			this->tabPageFileTypeDB->Location = System::Drawing::Point(4, 22);
			this->tabPageFileTypeDB->Name = L"tabPageFileTypeDB";
			this->tabPageFileTypeDB->Size = System::Drawing::Size(504, 262);
			this->tabPageFileTypeDB->TabIndex = 3;
			this->tabPageFileTypeDB->Text = L"File Type Database";
			// 
			// groupBox2
			// 
			this->groupBox2->Controls->Add(this->label4);
			this->groupBox2->Controls->Add(this->txtDataSource);
			this->groupBox2->Controls->Add(this->label3);
			this->groupBox2->Controls->Add(this->comboBox1);
			this->groupBox2->Location = System::Drawing::Point(8, 8);
			this->groupBox2->Name = L"groupBox2";
			this->groupBox2->Size = System::Drawing::Size(352, 184);
			this->groupBox2->TabIndex = 0;
			this->groupBox2->TabStop = false;
			this->groupBox2->Text = L"groupBox2";
			// 
			// comboBox1
			// 
			array<System::Object^>^ __mcTemp__3 = gcnew array<System::Object^>(5);
			__mcTemp__3[0] = L"Microsoft.Jet.OLEDB.4.0";
			__mcTemp__3[1] = L"sqloledb";
			__mcTemp__3[2] = L"SQLNCLI";
			__mcTemp__3[3] = L"MySQLProv";
			__mcTemp__3[4] = L"msdaora";
			this->comboBox1->Items->AddRange(__mcTemp__3);
			this->comboBox1->Location = System::Drawing::Point(80, 16);
			this->comboBox1->Name = L"comboBox1";
			this->comboBox1->Size = System::Drawing::Size(184, 21);
			this->comboBox1->TabIndex = 0;
			this->comboBox1->Text = L"Microsoft.Jet.OLEDB.4.0";
			// 
			// label3
			// 
			this->label3->Location = System::Drawing::Point(8, 21);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(64, 16);
			this->label3->TabIndex = 1;
			this->label3->Text = L"Provider:";
			// 
			// txtDataSource
			// 
			this->txtDataSource->Location = System::Drawing::Point(80, 48);
			this->txtDataSource->Name = L"txtDataSource";
			this->txtDataSource->Size = System::Drawing::Size(184, 20);
			this->txtDataSource->TabIndex = 2;
			this->txtDataSource->Text = L"FileTypes.mdb";
			// 
			// label4
			// 
			this->label4->Location = System::Drawing::Point(8, 52);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(72, 16);
			this->label4->TabIndex = 3;
			this->label4->Text = L"Data Source:";
			// 
			// Options
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(5, 13);
			this->CancelButton = this->btnCancel;
			this->ClientSize = System::Drawing::Size(536, 334);
			this->Controls->Add(this->btnApply);
			this->Controls->Add(this->tabControl);
			this->Controls->Add(this->btnCancel);
			this->Controls->Add(this->btnOK);
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"Options";
			this->ShowInTaskbar = false;
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
			this->Text = L"Options";
			this->tabControl->ResumeLayout(false);
			this->tabPageTechView->ResumeLayout(false);
			this->groupBox1->ResumeLayout(false);
			this->tabPageHexView->ResumeLayout(false);
			this->tabPageFileTypeDB->ResumeLayout(false);
			this->groupBox2->ResumeLayout(false);
			this->ResumeLayout(false);

		}		


		
	private: void SetControls();

	private: System::Void cmbBackColor_SelectedIndexChanged(System::Object ^  sender, System::EventArgs ^  e)
			 {
			 }

};
}