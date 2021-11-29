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
	/// Summary for LoadingDialog
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class LoadingDialog : public System::Windows::Forms::Form
	{
	public: 
		LoadingDialog(void)
		{
			InitializeComponent();
		}
        
	protected: 
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~LoadingDialog()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Label ^  lblLoadingUFE;
	private: System::Windows::Forms::Label ^  lblLoading;

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
			this->lblLoadingUFE = gcnew System::Windows::Forms::Label();
			this->lblLoading = gcnew System::Windows::Forms::Label();
			this->SuspendLayout();
			// 
			// lblLoadingUFE
			// 
			this->lblLoadingUFE->Font = gcnew System::Drawing::Font(L"Microsoft Sans Serif", 18, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, (System::Byte)0);
			this->lblLoadingUFE->Location = System::Drawing::Point(8, 8);
			this->lblLoadingUFE->Name = L"lblLoadingUFE";
			this->lblLoadingUFE->Size = System::Drawing::Size(320, 48);
			this->lblLoadingUFE->TabIndex = 0;
			this->lblLoadingUFE->Text = L"Loading UFE....";
			// 
			// lblLoading
			// 
			this->lblLoading->Location = System::Drawing::Point(8, 144);
			this->lblLoading->Name = L"lblLoading";
			this->lblLoading->Size = System::Drawing::Size(184, 16);
			this->lblLoading->TabIndex = 1;
			// 
			// LoadingDialog
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(5, 13);
			this->ClientSize = System::Drawing::Size(432, 166);
			this->ControlBox = false;
			this->Controls->Add(this->lblLoading);
			this->Controls->Add(this->lblLoadingUFE);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::None;
			this->Name = L"LoadingDialog";
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"Loading UFE......";
			this->ResumeLayout(false);

		}		


	public:void SetLoading(String^ text)
		{
			lblLoading->Text = text;
		}

	public:	void CloseDialog()
		{
			this->Close();
		}

	};
}