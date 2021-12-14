#pragma once
//#include "AssemblyInfo.cpp"

#include "./About.h"
#include "./FTMForm.h"
#include "./Options.h"
#include "./FileStreamSearch.h"
#include "./SearchDialog.h"

#ifdef GetObject
	#undef GetObject
#endif

// The Default settings for debug mode
#ifdef _DEBUG
	#define DEFAULT_DEBUG		true
#else
	#define DEFAULT_DEBUG		false
#endif

// Number Formats
#define NF_DEF		Ufex::API::NumberFormat::Default
#define NF_BIN		Ufex::API::NumberFormat::Binary
#define NF_OCT		Ufex::API::NumberFormat::Octal
#define NF_DEC		Ufex::API::NumberFormat::Decimal
#define NF_HEX		Ufex::API::NumberFormat::Hexadecimal
#define NF_ASC		Ufex::API::NumberFormat::Ascii
#define NF_UNI		Ufex::API::NumberFormat::Unicode


// Keyboard Codes
#define KEY_DELETE		0x002E

// String Constants
#define NEW_LINE		L"\r\n"
#define BLANK_STRING	L""

// Determines whether data stored on the clipboard remains when the application exits
#define SAVE_CLIPBOARD	true

#define OPEN_FILE_FILTER	L"All files (*.*)|*.*|Image Files(*.BMP;*.GIF;*.PNG;*.ICO;*.CUR)|*.BMP;*.GIF;*.PNG;*.ICO;*.CUR|Audio Files(*.MP3;*.WAV;*.WMA)|*.MP3;*.WAV;*.WMA"

namespace UniversalFileExplorer
{
	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::Drawing::Imaging;
	using namespace System::IO;

	using namespace Ufex::Controls;
	using namespace Ufex::API;

	/// <summary> 
	/// Summary for Form1
	///
	/// WARNING: If you change the name of this class, you will need to change the 
	///          'Resource File Name' property for the managed resource compiler tool 
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class Form1 : public System::Windows::Forms::Form
	{	
	public:
		Form1(void)
		{	
			InitializeComponent();
			AppInit(L"");
		}

	public:
		Form1(String^ strCommand, Settings^ aSettings)
		{	
			DebugOut(L"-Form1-");
			InitializeComponent();
			AppInit(strCommand);
			DebugOut(L"-End Form1-");
		}
  
	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~Form1()
		{
			if (components)
			{
				delete components;
			}
		}

	private: System::Windows::Forms::TreeView ^  tvFile;

	private: System::Windows::Forms::Label ^  label2;
	private: System::Windows::Forms::TextBox ^  txtFileType;
	private: System::Windows::Forms::TextBox ^  txtDebug;
	private: System::Windows::Forms::TextBox ^  textFormDebug;
	private: System::Windows::Forms::PictureBox ^  picFile;
	private: System::Windows::Forms::DataGrid ^  dataGrid1;
	
	private: System::Windows::Forms::TabControl ^  tabControlViews;
	
			 // Tab Pages
	private: System::Windows::Forms::TabPage ^  tabPageTechView;
	private: System::Windows::Forms::TabPage^  tabPageGraphicView;
	private: System::Windows::Forms::TabPage ^  tabPageDebug;
	private: System::Windows::Forms::TabPage ^  tabPageQuickInfo;
	private: System::Windows::Forms::TabPage ^  tabPageHex;	
	private: System::Windows::Forms::TabPage ^  tabFileCheck;
	private: System::Windows::Forms::TabControl ^  tabDebug;
	private: System::Windows::Forms::TabPage ^  tabForm;
	private: System::Windows::Forms::TabPage ^  tabFile;
	private: System::Windows::Forms::Splitter ^  splitterTreeData;
	private: System::Windows::Forms::Panel ^  panel1;
	private: System::Windows::Forms::StatusBar ^  statusBar;
	private: System::Windows::Forms::TextBox ^  textFileCheck;
	private: System::Windows::Forms::Button ^  btnClearDebug;
	private: System::Windows::Forms::Button ^  btnRefresh;
	private: System::Windows::Forms::ListView ^  listViewQuickInfo;
	private: Ufex::Controls::HexViewControl ^  hexView;
	private: System::Windows::Forms::TextBox ^  textFileSize;
	private: System::Windows::Forms::TextBox ^  textFileName;
	private: System::Windows::Forms::TextBox ^  textFileExt;
	private: System::Windows::Forms::TextBox ^  textFilePath;
	private: System::ComponentModel::IContainer ^  components;
	private: System::Windows::Forms::GroupBox^  grpFileAttributes;
	private: System::Windows::Forms::CheckBox ^  chkAttReadOnly;
	private: System::Windows::Forms::CheckBox ^  chkAttHidden;
	private: System::Windows::Forms::CheckBox ^  chkAttSystem;
	private: System::Windows::Forms::CheckBox ^  chkAttArchive;
	private: System::Windows::Forms::CheckBox ^  chkAttNormal;
	private: System::Windows::Forms::CheckBox ^  chkAttTemporary;
	private: System::Windows::Forms::CheckBox ^  chkAttSparseFile;
	private: System::Windows::Forms::CheckBox ^  chkAttEncrypted;
	private: System::Windows::Forms::GroupBox ^  groupBoxFileDates;
	private: System::Windows::Forms::Label ^  label1;
	private: System::Windows::Forms::Label ^  lblFileCreatedDate;
	private: System::Windows::Forms::Label ^  label4;
	private: System::Windows::Forms::Label ^  lblFileModifiedDate;
	private: System::Windows::Forms::Label ^  label3;
	private: System::Windows::Forms::Label ^  lblFileAccessedDate;
	private: System::Windows::Forms::Label ^  staticFileSize;
	private: System::Windows::Forms::Label ^  staticFileExtension;
	private: System::Windows::Forms::Label ^  staticFileName;
	private: System::Windows::Forms::Label ^  staticFilePath;
	private: System::Windows::Forms::OpenFileDialog ^  openFileDialog;
private: System::Windows::Forms::ToolStripContainer^  toolStripContainer1;
private: System::Windows::Forms::ToolStrip^  toolStripFile;
private: System::Windows::Forms::ToolStripButton^  tsbOpenFile;
private: System::Windows::Forms::ToolStripButton^  tsbCloseFile;
private: System::Windows::Forms::ToolStrip^  toolStripNumFormat;
private: System::Windows::Forms::ToolStripButton^  tsbDefault;
private: System::Windows::Forms::ToolStripButton^  tsbHexadecimal;
private: System::Windows::Forms::ToolStripButton^  tsbDecimal;
private: System::Windows::Forms::ToolStripButton^  tsbBinary;
private: System::Windows::Forms::ToolStripButton^  tsbAscii;
private: System::Windows::Forms::MenuStrip^  menuStrip1;
private: System::Windows::Forms::ToolStripMenuItem^  menuFile;
private: System::Windows::Forms::ToolStripMenuItem^  menuFileOpen;
private: System::Windows::Forms::ToolStripMenuItem^  menuFileClose;
private: System::Windows::Forms::ToolStripMenuItem^  menuFileExit;
private: System::Windows::Forms::ToolStripMenuItem^  menuEdit;
private: System::Windows::Forms::ToolStripMenuItem^  menuEditCut;
private: System::Windows::Forms::ToolStripMenuItem^  menuEditCopy;
private: System::Windows::Forms::ToolStripMenuItem^  menuEditPaste;
private: System::Windows::Forms::ToolStripMenuItem^  menuEditDelete;
private: System::Windows::Forms::ToolStripMenuItem^  viewToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  numberFormatToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  menuNFDefault;

private: System::Windows::Forms::ToolStripMenuItem^  menuNFHexadecimal;
private: System::Windows::Forms::ToolStripMenuItem^  menuNFDecimal;
private: System::Windows::Forms::ToolStripMenuItem^  menuNFBinary;
private: System::Windows::Forms::ToolStripMenuItem^  menuNFASCII;
private: System::Windows::Forms::ToolStripMenuItem^  searchToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  findToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  findNextToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  toolsToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  helpToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  unToolStripMenuItem;
private: System::Windows::Forms::ToolStripMenuItem^  menuToolsFTM;
private: System::Windows::Forms::ToolStripMenuItem^  menuToolsOptions;
private: System::Windows::Forms::ToolStripMenuItem^  aboutToolStripMenuItem;
private: System::Windows::Forms::Panel^  panelMain;
private: System::Windows::Forms::TableLayoutPanel^  tlpQuickInfo;
private: System::Windows::Forms::TableLayoutPanel^  tableLayoutPanel3;
private: System::Windows::Forms::TableLayoutPanel^  tableLayoutPanel4;
private: System::Windows::Forms::TableLayoutPanel^  tableLayoutPanel5;
private: System::Windows::Forms::ToolStrip^  toolStrip1;
private: System::Windows::Forms::ToolStripButton^  tsbImageActualSize;
private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator1;
private: System::Windows::Forms::ToolStripButton^  tsbTableWidth;

private: System::Windows::Forms::FlowLayoutPanel^  flowLayoutPanel2;
private: System::Windows::Forms::ImageList^  imageListTabs;
private: System::Windows::Forms::ImageList^  imageListTreeView;
private: System::Windows::Forms::ContextMenuStrip^  contextMenuStripTreeNodeRightClick;

private: System::Windows::Forms::ToolStripMenuItem^  menuTnViewFileData;
private: System::Windows::Forms::CheckBox ^  chkAttCompressed;


	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
 

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(Form1::typeid));
			this->tvFile = (gcnew System::Windows::Forms::TreeView());
			this->openFileDialog = (gcnew System::Windows::Forms::OpenFileDialog());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->txtFileType = (gcnew System::Windows::Forms::TextBox());
			this->picFile = (gcnew System::Windows::Forms::PictureBox());
			this->dataGrid1 = (gcnew System::Windows::Forms::DataGrid());
			this->txtDebug = (gcnew System::Windows::Forms::TextBox());
			this->tabControlViews = (gcnew System::Windows::Forms::TabControl());
			this->tabPageQuickInfo = (gcnew System::Windows::Forms::TabPage());
			this->tlpQuickInfo = (gcnew System::Windows::Forms::TableLayoutPanel());
			this->listViewQuickInfo = (gcnew System::Windows::Forms::ListView());
			this->tableLayoutPanel5 = (gcnew System::Windows::Forms::TableLayoutPanel());
			this->textFileSize = (gcnew System::Windows::Forms::TextBox());
			this->staticFileSize = (gcnew System::Windows::Forms::Label());
			this->staticFileName = (gcnew System::Windows::Forms::Label());
			this->textFileName = (gcnew System::Windows::Forms::TextBox());
			this->textFileExt = (gcnew System::Windows::Forms::TextBox());
			this->staticFileExtension = (gcnew System::Windows::Forms::Label());
			this->tableLayoutPanel3 = (gcnew System::Windows::Forms::TableLayoutPanel());
			this->flowLayoutPanel2 = (gcnew System::Windows::Forms::FlowLayoutPanel());
			this->grpFileAttributes = (gcnew System::Windows::Forms::GroupBox());
			this->chkAttCompressed = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttEncrypted = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttSparseFile = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttTemporary = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttNormal = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttArchive = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttSystem = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttHidden = (gcnew System::Windows::Forms::CheckBox());
			this->chkAttReadOnly = (gcnew System::Windows::Forms::CheckBox());
			this->groupBoxFileDates = (gcnew System::Windows::Forms::GroupBox());
			this->lblFileAccessedDate = (gcnew System::Windows::Forms::Label());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->lblFileModifiedDate = (gcnew System::Windows::Forms::Label());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->lblFileCreatedDate = (gcnew System::Windows::Forms::Label());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->tableLayoutPanel4 = (gcnew System::Windows::Forms::TableLayoutPanel());
			this->staticFilePath = (gcnew System::Windows::Forms::Label());
			this->textFilePath = (gcnew System::Windows::Forms::TextBox());
			this->tabPageTechView = (gcnew System::Windows::Forms::TabPage());
			this->panel1 = (gcnew System::Windows::Forms::Panel());
			this->splitterTreeData = (gcnew System::Windows::Forms::Splitter());
			this->tabPageGraphicView = (gcnew System::Windows::Forms::TabPage());
			this->tabFileCheck = (gcnew System::Windows::Forms::TabPage());
			this->textFileCheck = (gcnew System::Windows::Forms::TextBox());
			this->tabPageHex = (gcnew System::Windows::Forms::TabPage());
			this->hexView = (gcnew Ufex::Controls::HexViewControl());
			this->tabPageDebug = (gcnew System::Windows::Forms::TabPage());
			this->tabDebug = (gcnew System::Windows::Forms::TabControl());
			this->tabForm = (gcnew System::Windows::Forms::TabPage());
			this->btnClearDebug = (gcnew System::Windows::Forms::Button());
			this->textFormDebug = (gcnew System::Windows::Forms::TextBox());
			this->tabFile = (gcnew System::Windows::Forms::TabPage());
			this->btnRefresh = (gcnew System::Windows::Forms::Button());
			this->imageListTabs = (gcnew System::Windows::Forms::ImageList(this->components));
			this->statusBar = (gcnew System::Windows::Forms::StatusBar());
			this->toolStripContainer1 = (gcnew System::Windows::Forms::ToolStripContainer());
			this->panelMain = (gcnew System::Windows::Forms::Panel());
			this->menuStrip1 = (gcnew System::Windows::Forms::MenuStrip());
			this->menuFile = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuFileOpen = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuFileClose = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuFileExit = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuEdit = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuEditCut = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuEditCopy = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuEditPaste = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuEditDelete = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->viewToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->numberFormatToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuNFDefault = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuNFHexadecimal = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuNFDecimal = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuNFBinary = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuNFASCII = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->searchToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->findToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->findNextToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolsToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuToolsFTM = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuToolsOptions = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->helpToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->unToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->aboutToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripFile = (gcnew System::Windows::Forms::ToolStrip());
			this->tsbOpenFile = (gcnew System::Windows::Forms::ToolStripButton());
			this->tsbCloseFile = (gcnew System::Windows::Forms::ToolStripButton());
			this->toolStripNumFormat = (gcnew System::Windows::Forms::ToolStrip());
			this->tsbDefault = (gcnew System::Windows::Forms::ToolStripButton());
			this->tsbHexadecimal = (gcnew System::Windows::Forms::ToolStripButton());
			this->tsbDecimal = (gcnew System::Windows::Forms::ToolStripButton());
			this->tsbBinary = (gcnew System::Windows::Forms::ToolStripButton());
			this->tsbAscii = (gcnew System::Windows::Forms::ToolStripButton());
			this->toolStripSeparator1 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->tsbTableWidth = (gcnew System::Windows::Forms::ToolStripButton());
			this->toolStrip1 = (gcnew System::Windows::Forms::ToolStrip());
			this->tsbImageActualSize = (gcnew System::Windows::Forms::ToolStripButton());
			this->imageListTreeView = (gcnew System::Windows::Forms::ImageList(this->components));
			this->contextMenuStripTreeNodeRightClick = (gcnew System::Windows::Forms::ContextMenuStrip(this->components));
			this->menuTnViewFileData = (gcnew System::Windows::Forms::ToolStripMenuItem());
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->picFile))->BeginInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->dataGrid1))->BeginInit();
			this->tabControlViews->SuspendLayout();
			this->tabPageQuickInfo->SuspendLayout();
			this->tlpQuickInfo->SuspendLayout();
			this->tableLayoutPanel5->SuspendLayout();
			this->tableLayoutPanel3->SuspendLayout();
			this->flowLayoutPanel2->SuspendLayout();
			this->grpFileAttributes->SuspendLayout();
			this->groupBoxFileDates->SuspendLayout();
			this->tableLayoutPanel4->SuspendLayout();
			this->tabPageTechView->SuspendLayout();
			this->panel1->SuspendLayout();
			this->tabPageGraphicView->SuspendLayout();
			this->tabFileCheck->SuspendLayout();
			this->tabPageHex->SuspendLayout();
			this->tabPageDebug->SuspendLayout();
			this->tabDebug->SuspendLayout();
			this->tabForm->SuspendLayout();
			this->tabFile->SuspendLayout();
			this->toolStripContainer1->ContentPanel->SuspendLayout();
			this->toolStripContainer1->TopToolStripPanel->SuspendLayout();
			this->toolStripContainer1->SuspendLayout();
			this->panelMain->SuspendLayout();
			this->menuStrip1->SuspendLayout();
			this->toolStripFile->SuspendLayout();
			this->toolStripNumFormat->SuspendLayout();
			this->toolStrip1->SuspendLayout();
			this->contextMenuStripTreeNodeRightClick->SuspendLayout();
			this->SuspendLayout();
			// 
			// tvFile
			// 
			this->tvFile->Dock = System::Windows::Forms::DockStyle::Left;
			this->tvFile->Location = System::Drawing::Point(3, 3);
			this->tvFile->Name = L"tvFile";
			this->tvFile->Size = System::Drawing::Size(256, 444);
			this->tvFile->TabIndex = 5;
			this->tvFile->AfterSelect += gcnew System::Windows::Forms::TreeViewEventHandler(this, &Form1::tvFile_AfterSelect);
			this->tvFile->KeyPress += gcnew System::Windows::Forms::KeyPressEventHandler(this, &Form1::tvFile_KeyPress);
			this->tvFile->NodeMouseClick += gcnew System::Windows::Forms::TreeNodeMouseClickEventHandler(this, &Form1::tvFile_NodeMouseClick);
			this->tvFile->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &Form1::tvFile_KeyDown);
			// 
			// label2
			// 
			this->label2->Dock = System::Windows::Forms::DockStyle::Fill;
			this->label2->Location = System::Drawing::Point(3, 0);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(63, 27);
			this->label2->TabIndex = 4;
			this->label2->Text = L"File Type:";
			this->label2->TextAlign = System::Drawing::ContentAlignment::MiddleLeft;
			// 
			// txtFileType
			// 
			this->txtFileType->Dock = System::Windows::Forms::DockStyle::Top;
			this->txtFileType->Location = System::Drawing::Point(72, 3);
			this->txtFileType->Name = L"txtFileType";
			this->txtFileType->ReadOnly = true;
			this->txtFileType->Size = System::Drawing::Size(585, 20);
			this->txtFileType->TabIndex = 2;
			// 
			// picFile
			// 
			this->picFile->Dock = System::Windows::Forms::DockStyle::Fill;
			this->picFile->Location = System::Drawing::Point(3, 3);
			this->picFile->Name = L"picFile";
			this->picFile->Size = System::Drawing::Size(666, 444);
			this->picFile->TabIndex = 1;
			this->picFile->TabStop = false;
			// 
			// dataGrid1
			// 
			this->dataGrid1->AlternatingBackColor = System::Drawing::Color::GhostWhite;
			this->dataGrid1->BackgroundColor = System::Drawing::SystemColors::Window;
			this->dataGrid1->BorderStyle = System::Windows::Forms::BorderStyle::None;
			this->dataGrid1->CaptionForeColor = System::Drawing::SystemColors::ActiveCaptionText;
			this->dataGrid1->CaptionVisible = false;
			this->dataGrid1->DataMember = L"";
			this->dataGrid1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->dataGrid1->FlatMode = true;
			this->dataGrid1->Font = (gcnew System::Drawing::Font(L"Tahoma", 8));
			this->dataGrid1->ForeColor = System::Drawing::Color::MidnightBlue;
			this->dataGrid1->GridLineColor = System::Drawing::Color::RoyalBlue;
			this->dataGrid1->HeaderBackColor = System::Drawing::SystemColors::Desktop;
			this->dataGrid1->HeaderFont = (gcnew System::Drawing::Font(L"Tahoma", 8, System::Drawing::FontStyle::Bold));
			this->dataGrid1->HeaderForeColor = System::Drawing::Color::LavenderBlush;
			this->dataGrid1->LinkColor = System::Drawing::Color::Teal;
			this->dataGrid1->Location = System::Drawing::Point(6, 6);
			this->dataGrid1->Name = L"dataGrid1";
			this->dataGrid1->ParentRowsBackColor = System::Drawing::Color::Lavender;
			this->dataGrid1->ParentRowsForeColor = System::Drawing::Color::MidnightBlue;
			this->dataGrid1->PreferredColumnWidth = 200;
			this->dataGrid1->ReadOnly = true;
			this->dataGrid1->RowHeadersVisible = false;
			this->dataGrid1->SelectionBackColor = System::Drawing::Color::Teal;
			this->dataGrid1->SelectionForeColor = System::Drawing::Color::PaleGreen;
			this->dataGrid1->Size = System::Drawing::Size(390, 432);
			this->dataGrid1->TabIndex = 6;
			// 
			// txtDebug
			// 
			this->txtDebug->Location = System::Drawing::Point(8, 8);
			this->txtDebug->MaxLength = 65535;
			this->txtDebug->Multiline = true;
			this->txtDebug->Name = L"txtDebug";
			this->txtDebug->ReadOnly = true;
			this->txtDebug->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->txtDebug->Size = System::Drawing::Size(611, 288);
			this->txtDebug->TabIndex = 15;
			this->txtDebug->Text = L"Debug Info Goes Here";
			// 
			// tabControlViews
			// 
			this->tabControlViews->Controls->Add(this->tabPageQuickInfo);
			this->tabControlViews->Controls->Add(this->tabPageTechView);
			this->tabControlViews->Controls->Add(this->tabPageGraphicView);
			this->tabControlViews->Controls->Add(this->tabFileCheck);
			this->tabControlViews->Controls->Add(this->tabPageHex);
			this->tabControlViews->Controls->Add(this->tabPageDebug);
			this->tabControlViews->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tabControlViews->HotTrack = true;
			this->tabControlViews->ImageList = this->imageListTabs;
			this->tabControlViews->Location = System::Drawing::Point(3, 3);
			this->tabControlViews->Name = L"tabControlViews";
			this->tabControlViews->SelectedIndex = 0;
			this->tabControlViews->Size = System::Drawing::Size(680, 502);
			this->tabControlViews->TabIndex = 4;
			this->tabControlViews->SelectedIndexChanged += gcnew System::EventHandler(this, &Form1::tabControlViews_SelectedIndexChanged);
			// 
			// tabPageQuickInfo
			// 
			this->tabPageQuickInfo->Controls->Add(this->tlpQuickInfo);
			this->tabPageQuickInfo->ImageKey = L"Information.png";
			this->tabPageQuickInfo->Location = System::Drawing::Point(4, 23);
			this->tabPageQuickInfo->Margin = System::Windows::Forms::Padding(6);
			this->tabPageQuickInfo->Name = L"tabPageQuickInfo";
			this->tabPageQuickInfo->Padding = System::Windows::Forms::Padding(3);
			this->tabPageQuickInfo->Size = System::Drawing::Size(672, 475);
			this->tabPageQuickInfo->TabIndex = 2;
			this->tabPageQuickInfo->Text = L"Quick Info";
			this->tabPageQuickInfo->ToolTipText = L"Quick Info";
			this->tabPageQuickInfo->UseVisualStyleBackColor = true;
			// 
			// tlpQuickInfo
			// 
			this->tlpQuickInfo->ColumnCount = 1;
			this->tlpQuickInfo->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 100)));
			this->tlpQuickInfo->Controls->Add(this->listViewQuickInfo, 0, 3);
			this->tlpQuickInfo->Controls->Add(this->tableLayoutPanel5, 0, 1);
			this->tlpQuickInfo->Controls->Add(this->tableLayoutPanel3, 0, 2);
			this->tlpQuickInfo->Controls->Add(this->tableLayoutPanel4, 0, 0);
			this->tlpQuickInfo->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tlpQuickInfo->Location = System::Drawing::Point(3, 3);
			this->tlpQuickInfo->Margin = System::Windows::Forms::Padding(0);
			this->tlpQuickInfo->Name = L"tlpQuickInfo";
			this->tlpQuickInfo->RowCount = 4;
			this->tlpQuickInfo->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Absolute, 57)));
			this->tlpQuickInfo->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Absolute, 29)));
			this->tlpQuickInfo->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Absolute, 98)));
			this->tlpQuickInfo->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Percent, 100)));
			this->tlpQuickInfo->Size = System::Drawing::Size(666, 469);
			this->tlpQuickInfo->TabIndex = 30;
			// 
			// listViewQuickInfo
			// 
			this->listViewQuickInfo->Dock = System::Windows::Forms::DockStyle::Fill;
			this->listViewQuickInfo->GridLines = true;
			this->listViewQuickInfo->LabelEdit = true;
			this->listViewQuickInfo->Location = System::Drawing::Point(3, 187);
			this->listViewQuickInfo->Name = L"listViewQuickInfo";
			this->listViewQuickInfo->Size = System::Drawing::Size(660, 279);
			this->listViewQuickInfo->TabIndex = 18;
			this->listViewQuickInfo->UseCompatibleStateImageBehavior = false;
			this->listViewQuickInfo->View = System::Windows::Forms::View::Details;
			this->listViewQuickInfo->SelectedIndexChanged += gcnew System::EventHandler(this, &Form1::listViewQuickInfo_SelectedIndexChanged);
			// 
			// tableLayoutPanel5
			// 
			this->tableLayoutPanel5->ColumnCount = 6;
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Absolute, 
				69)));
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 
				53.18923F)));
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Absolute, 
				88)));
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 
				18.19072F)));
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Absolute, 
				64)));
			this->tableLayoutPanel5->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 
				28.62006F)));
			this->tableLayoutPanel5->Controls->Add(this->textFileSize, 5, 0);
			this->tableLayoutPanel5->Controls->Add(this->staticFileSize, 4, 0);
			this->tableLayoutPanel5->Controls->Add(this->staticFileName, 0, 0);
			this->tableLayoutPanel5->Controls->Add(this->textFileName, 1, 0);
			this->tableLayoutPanel5->Controls->Add(this->textFileExt, 3, 0);
			this->tableLayoutPanel5->Controls->Add(this->staticFileExtension, 2, 0);
			this->tableLayoutPanel5->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tableLayoutPanel5->Location = System::Drawing::Point(3, 60);
			this->tableLayoutPanel5->Name = L"tableLayoutPanel5";
			this->tableLayoutPanel5->RowCount = 1;
			this->tableLayoutPanel5->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Percent, 100)));
			this->tableLayoutPanel5->Size = System::Drawing::Size(660, 23);
			this->tableLayoutPanel5->TabIndex = 24;
			// 
			// textFileSize
			// 
			this->textFileSize->Dock = System::Windows::Forms::DockStyle::Top;
			this->textFileSize->Location = System::Drawing::Point(536, 3);
			this->textFileSize->Name = L"textFileSize";
			this->textFileSize->ReadOnly = true;
			this->textFileSize->Size = System::Drawing::Size(121, 20);
			this->textFileSize->TabIndex = 19;
			// 
			// staticFileSize
			// 
			this->staticFileSize->Dock = System::Windows::Forms::DockStyle::Fill;
			this->staticFileSize->Location = System::Drawing::Point(472, 0);
			this->staticFileSize->Name = L"staticFileSize";
			this->staticFileSize->Size = System::Drawing::Size(58, 23);
			this->staticFileSize->TabIndex = 26;
			this->staticFileSize->Text = L"File Size:";
			this->staticFileSize->TextAlign = System::Drawing::ContentAlignment::MiddleLeft;
			// 
			// staticFileName
			// 
			this->staticFileName->Dock = System::Windows::Forms::DockStyle::Fill;
			this->staticFileName->Location = System::Drawing::Point(3, 0);
			this->staticFileName->Name = L"staticFileName";
			this->staticFileName->Size = System::Drawing::Size(63, 23);
			this->staticFileName->TabIndex = 24;
			this->staticFileName->Text = L"File Name:";
			this->staticFileName->TextAlign = System::Drawing::ContentAlignment::MiddleLeft;
			// 
			// textFileName
			// 
			this->textFileName->Dock = System::Windows::Forms::DockStyle::Top;
			this->textFileName->Location = System::Drawing::Point(72, 3);
			this->textFileName->Name = L"textFileName";
			this->textFileName->ReadOnly = true;
			this->textFileName->Size = System::Drawing::Size(227, 20);
			this->textFileName->TabIndex = 20;
			// 
			// textFileExt
			// 
			this->textFileExt->Dock = System::Windows::Forms::DockStyle::Top;
			this->textFileExt->Location = System::Drawing::Point(393, 3);
			this->textFileExt->Name = L"textFileExt";
			this->textFileExt->ReadOnly = true;
			this->textFileExt->Size = System::Drawing::Size(73, 20);
			this->textFileExt->TabIndex = 21;
			// 
			// staticFileExtension
			// 
			this->staticFileExtension->Dock = System::Windows::Forms::DockStyle::Fill;
			this->staticFileExtension->Location = System::Drawing::Point(305, 0);
			this->staticFileExtension->Name = L"staticFileExtension";
			this->staticFileExtension->Size = System::Drawing::Size(82, 23);
			this->staticFileExtension->TabIndex = 25;
			this->staticFileExtension->Text = L"File Extension:";
			this->staticFileExtension->TextAlign = System::Drawing::ContentAlignment::MiddleLeft;
			// 
			// tableLayoutPanel3
			// 
			this->tableLayoutPanel3->ColumnCount = 1;
			this->tableLayoutPanel3->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 
				100)));
			this->tableLayoutPanel3->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Absolute, 
				20)));
			this->tableLayoutPanel3->Controls->Add(this->flowLayoutPanel2, 0, 0);
			this->tableLayoutPanel3->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tableLayoutPanel3->Location = System::Drawing::Point(3, 89);
			this->tableLayoutPanel3->Name = L"tableLayoutPanel3";
			this->tableLayoutPanel3->Padding = System::Windows::Forms::Padding(3);
			this->tableLayoutPanel3->RowCount = 1;
			this->tableLayoutPanel3->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Percent, 100)));
			this->tableLayoutPanel3->Size = System::Drawing::Size(660, 92);
			this->tableLayoutPanel3->TabIndex = 19;
			// 
			// flowLayoutPanel2
			// 
			this->flowLayoutPanel2->Controls->Add(this->grpFileAttributes);
			this->flowLayoutPanel2->Controls->Add(this->groupBoxFileDates);
			this->flowLayoutPanel2->Dock = System::Windows::Forms::DockStyle::Fill;
			this->flowLayoutPanel2->Location = System::Drawing::Point(6, 6);
			this->flowLayoutPanel2->Name = L"flowLayoutPanel2";
			this->flowLayoutPanel2->Size = System::Drawing::Size(648, 80);
			this->flowLayoutPanel2->TabIndex = 0;
			// 
			// grpFileAttributes
			// 
			this->grpFileAttributes->Controls->Add(this->chkAttCompressed);
			this->grpFileAttributes->Controls->Add(this->chkAttEncrypted);
			this->grpFileAttributes->Controls->Add(this->chkAttSparseFile);
			this->grpFileAttributes->Controls->Add(this->chkAttTemporary);
			this->grpFileAttributes->Controls->Add(this->chkAttNormal);
			this->grpFileAttributes->Controls->Add(this->chkAttArchive);
			this->grpFileAttributes->Controls->Add(this->chkAttSystem);
			this->grpFileAttributes->Controls->Add(this->chkAttHidden);
			this->grpFileAttributes->Controls->Add(this->chkAttReadOnly);
			this->grpFileAttributes->Location = System::Drawing::Point(3, 3);
			this->grpFileAttributes->Name = L"grpFileAttributes";
			this->grpFileAttributes->Size = System::Drawing::Size(296, 72);
			this->grpFileAttributes->TabIndex = 28;
			this->grpFileAttributes->TabStop = false;
			this->grpFileAttributes->Text = L"Attributes";
			// 
			// chkAttCompressed
			// 
			this->chkAttCompressed->AutoCheck = false;
			this->chkAttCompressed->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttCompressed->Location = System::Drawing::Point(104, 48);
			this->chkAttCompressed->Name = L"chkAttCompressed";
			this->chkAttCompressed->Size = System::Drawing::Size(88, 16);
			this->chkAttCompressed->TabIndex = 37;
			this->chkAttCompressed->Text = L"Compressed";
			// 
			// chkAttEncrypted
			// 
			this->chkAttEncrypted->AutoCheck = false;
			this->chkAttEncrypted->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttEncrypted->Location = System::Drawing::Point(6, 48);
			this->chkAttEncrypted->Name = L"chkAttEncrypted";
			this->chkAttEncrypted->Size = System::Drawing::Size(88, 16);
			this->chkAttEncrypted->TabIndex = 36;
			this->chkAttEncrypted->Text = L"Encrypted";
			// 
			// chkAttSparseFile
			// 
			this->chkAttSparseFile->AutoCheck = false;
			this->chkAttSparseFile->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttSparseFile->Location = System::Drawing::Point(200, 48);
			this->chkAttSparseFile->Name = L"chkAttSparseFile";
			this->chkAttSparseFile->Size = System::Drawing::Size(88, 16);
			this->chkAttSparseFile->TabIndex = 35;
			this->chkAttSparseFile->Text = L"Sparse File";
			// 
			// chkAttTemporary
			// 
			this->chkAttTemporary->AutoCheck = false;
			this->chkAttTemporary->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttTemporary->Location = System::Drawing::Point(104, 32);
			this->chkAttTemporary->Name = L"chkAttTemporary";
			this->chkAttTemporary->Size = System::Drawing::Size(88, 16);
			this->chkAttTemporary->TabIndex = 34;
			this->chkAttTemporary->Text = L"Temporary";
			// 
			// chkAttNormal
			// 
			this->chkAttNormal->AutoCheck = false;
			this->chkAttNormal->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttNormal->Location = System::Drawing::Point(6, 30);
			this->chkAttNormal->Name = L"chkAttNormal";
			this->chkAttNormal->Size = System::Drawing::Size(88, 16);
			this->chkAttNormal->TabIndex = 33;
			this->chkAttNormal->Text = L"Normal";
			// 
			// chkAttArchive
			// 
			this->chkAttArchive->AutoCheck = false;
			this->chkAttArchive->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttArchive->Location = System::Drawing::Point(200, 16);
			this->chkAttArchive->Name = L"chkAttArchive";
			this->chkAttArchive->Size = System::Drawing::Size(88, 16);
			this->chkAttArchive->TabIndex = 31;
			this->chkAttArchive->Text = L"Archive";
			// 
			// chkAttSystem
			// 
			this->chkAttSystem->AutoCheck = false;
			this->chkAttSystem->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttSystem->Location = System::Drawing::Point(200, 32);
			this->chkAttSystem->Name = L"chkAttSystem";
			this->chkAttSystem->Size = System::Drawing::Size(88, 16);
			this->chkAttSystem->TabIndex = 29;
			this->chkAttSystem->Text = L"System";
			// 
			// chkAttHidden
			// 
			this->chkAttHidden->AutoCheck = false;
			this->chkAttHidden->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttHidden->Location = System::Drawing::Point(104, 14);
			this->chkAttHidden->Name = L"chkAttHidden";
			this->chkAttHidden->Size = System::Drawing::Size(88, 16);
			this->chkAttHidden->TabIndex = 28;
			this->chkAttHidden->Text = L"Hidden";
			// 
			// chkAttReadOnly
			// 
			this->chkAttReadOnly->AutoCheck = false;
			this->chkAttReadOnly->FlatStyle = System::Windows::Forms::FlatStyle::Popup;
			this->chkAttReadOnly->Location = System::Drawing::Point(6, 14);
			this->chkAttReadOnly->Name = L"chkAttReadOnly";
			this->chkAttReadOnly->Size = System::Drawing::Size(88, 16);
			this->chkAttReadOnly->TabIndex = 27;
			this->chkAttReadOnly->Text = L"Read-only";
			// 
			// groupBoxFileDates
			// 
			this->groupBoxFileDates->Controls->Add(this->lblFileAccessedDate);
			this->groupBoxFileDates->Controls->Add(this->label3);
			this->groupBoxFileDates->Controls->Add(this->lblFileModifiedDate);
			this->groupBoxFileDates->Controls->Add(this->label4);
			this->groupBoxFileDates->Controls->Add(this->lblFileCreatedDate);
			this->groupBoxFileDates->Controls->Add(this->label1);
			this->groupBoxFileDates->Location = System::Drawing::Point(305, 3);
			this->groupBoxFileDates->Name = L"groupBoxFileDates";
			this->groupBoxFileDates->Size = System::Drawing::Size(233, 72);
			this->groupBoxFileDates->TabIndex = 29;
			this->groupBoxFileDates->TabStop = false;
			this->groupBoxFileDates->Text = L"Timestamps";
			// 
			// lblFileAccessedDate
			// 
			this->lblFileAccessedDate->AutoEllipsis = true;
			this->lblFileAccessedDate->Location = System::Drawing::Point(76, 48);
			this->lblFileAccessedDate->Name = L"lblFileAccessedDate";
			this->lblFileAccessedDate->Size = System::Drawing::Size(141, 16);
			this->lblFileAccessedDate->TabIndex = 5;
			// 
			// label3
			// 
			this->label3->Location = System::Drawing::Point(6, 48);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(64, 16);
			this->label3->TabIndex = 4;
			this->label3->Text = L"Accessed:";
			// 
			// lblFileModifiedDate
			// 
			this->lblFileModifiedDate->AutoEllipsis = true;
			this->lblFileModifiedDate->Location = System::Drawing::Point(76, 32);
			this->lblFileModifiedDate->Name = L"lblFileModifiedDate";
			this->lblFileModifiedDate->Size = System::Drawing::Size(141, 16);
			this->lblFileModifiedDate->TabIndex = 3;
			// 
			// label4
			// 
			this->label4->Location = System::Drawing::Point(6, 32);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(64, 16);
			this->label4->TabIndex = 2;
			this->label4->Text = L"Modified:";
			// 
			// lblFileCreatedDate
			// 
			this->lblFileCreatedDate->AutoEllipsis = true;
			this->lblFileCreatedDate->Cursor = System::Windows::Forms::Cursors::Default;
			this->lblFileCreatedDate->Location = System::Drawing::Point(76, 16);
			this->lblFileCreatedDate->Name = L"lblFileCreatedDate";
			this->lblFileCreatedDate->Size = System::Drawing::Size(141, 16);
			this->lblFileCreatedDate->TabIndex = 1;
			// 
			// label1
			// 
			this->label1->ImageKey = L"(none)";
			this->label1->Location = System::Drawing::Point(6, 16);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(64, 16);
			this->label1->TabIndex = 0;
			this->label1->Text = L"Created:";
			// 
			// tableLayoutPanel4
			// 
			this->tableLayoutPanel4->ColumnCount = 2;
			this->tableLayoutPanel4->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Absolute, 
				69)));
			this->tableLayoutPanel4->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent, 
				100)));
			this->tableLayoutPanel4->Controls->Add(this->txtFileType, 1, 0);
			this->tableLayoutPanel4->Controls->Add(this->staticFilePath, 0, 1);
			this->tableLayoutPanel4->Controls->Add(this->label2, 0, 0);
			this->tableLayoutPanel4->Controls->Add(this->textFilePath, 1, 1);
			this->tableLayoutPanel4->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tableLayoutPanel4->Location = System::Drawing::Point(3, 3);
			this->tableLayoutPanel4->Name = L"tableLayoutPanel4";
			this->tableLayoutPanel4->RowCount = 2;
			this->tableLayoutPanel4->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Absolute, 27)));
			this->tableLayoutPanel4->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Absolute, 23)));
			this->tableLayoutPanel4->Size = System::Drawing::Size(660, 51);
			this->tableLayoutPanel4->TabIndex = 20;
			// 
			// staticFilePath
			// 
			this->staticFilePath->Dock = System::Windows::Forms::DockStyle::Fill;
			this->staticFilePath->Location = System::Drawing::Point(3, 27);
			this->staticFilePath->Name = L"staticFilePath";
			this->staticFilePath->Size = System::Drawing::Size(63, 24);
			this->staticFilePath->TabIndex = 23;
			this->staticFilePath->Text = L"File Path:";
			this->staticFilePath->TextAlign = System::Drawing::ContentAlignment::MiddleLeft;
			// 
			// textFilePath
			// 
			this->textFilePath->Dock = System::Windows::Forms::DockStyle::Top;
			this->textFilePath->Location = System::Drawing::Point(72, 30);
			this->textFilePath->Name = L"textFilePath";
			this->textFilePath->ReadOnly = true;
			this->textFilePath->Size = System::Drawing::Size(585, 20);
			this->textFilePath->TabIndex = 22;
			// 
			// tabPageTechView
			// 
			this->tabPageTechView->Controls->Add(this->panel1);
			this->tabPageTechView->Controls->Add(this->splitterTreeData);
			this->tabPageTechView->Controls->Add(this->tvFile);
			this->tabPageTechView->ImageKey = L"Control_TreeView.png";
			this->tabPageTechView->Location = System::Drawing::Point(4, 23);
			this->tabPageTechView->Name = L"tabPageTechView";
			this->tabPageTechView->Padding = System::Windows::Forms::Padding(3);
			this->tabPageTechView->Size = System::Drawing::Size(672, 450);
			this->tabPageTechView->TabIndex = 0;
			this->tabPageTechView->Text = L"Technical";
			this->tabPageTechView->ToolTipText = L"Technical File Data";
			this->tabPageTechView->UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this->panel1->Controls->Add(this->dataGrid1);
			this->panel1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->panel1->Location = System::Drawing::Point(267, 3);
			this->panel1->Name = L"panel1";
			this->panel1->Padding = System::Windows::Forms::Padding(6);
			this->panel1->Size = System::Drawing::Size(402, 444);
			this->panel1->TabIndex = 8;
			// 
			// splitterTreeData
			// 
			this->splitterTreeData->Cursor = System::Windows::Forms::Cursors::VSplit;
			this->splitterTreeData->Location = System::Drawing::Point(259, 3);
			this->splitterTreeData->Name = L"splitterTreeData";
			this->splitterTreeData->Size = System::Drawing::Size(8, 444);
			this->splitterTreeData->TabIndex = 7;
			this->splitterTreeData->TabStop = false;
			this->splitterTreeData->SplitterMoved += gcnew System::Windows::Forms::SplitterEventHandler(this, &Form1::splitterTreeData_Moved);
			// 
			// tabPageGraphicView
			// 
			this->tabPageGraphicView->Controls->Add(this->picFile);
			this->tabPageGraphicView->ImageKey = L"InsertPictureHS.png";
			this->tabPageGraphicView->Location = System::Drawing::Point(4, 23);
			this->tabPageGraphicView->Name = L"tabPageGraphicView";
			this->tabPageGraphicView->Padding = System::Windows::Forms::Padding(3);
			this->tabPageGraphicView->Size = System::Drawing::Size(672, 450);
			this->tabPageGraphicView->TabIndex = 1;
			this->tabPageGraphicView->Text = L"Graphic";
			this->tabPageGraphicView->ToolTipText = L"Image View";
			this->tabPageGraphicView->UseVisualStyleBackColor = true;
			// 
			// tabFileCheck
			// 
			this->tabFileCheck->Controls->Add(this->textFileCheck);
			this->tabFileCheck->ImageKey = L"TaskHS.png";
			this->tabFileCheck->Location = System::Drawing::Point(4, 23);
			this->tabFileCheck->Name = L"tabFileCheck";
			this->tabFileCheck->Padding = System::Windows::Forms::Padding(3);
			this->tabFileCheck->Size = System::Drawing::Size(672, 450);
			this->tabFileCheck->TabIndex = 6;
			this->tabFileCheck->Text = L"File Check";
			this->tabFileCheck->ToolTipText = L"File Checker Information";
			this->tabFileCheck->UseVisualStyleBackColor = true;
			// 
			// textFileCheck
			// 
			this->textFileCheck->Dock = System::Windows::Forms::DockStyle::Fill;
			this->textFileCheck->Font = (gcnew System::Drawing::Font(L"Courier New", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(0)));
			this->textFileCheck->Location = System::Drawing::Point(3, 3);
			this->textFileCheck->Multiline = true;
			this->textFileCheck->Name = L"textFileCheck";
			this->textFileCheck->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->textFileCheck->Size = System::Drawing::Size(666, 444);
			this->textFileCheck->TabIndex = 0;
			// 
			// tabPageHex
			// 
			this->tabPageHex->Controls->Add(this->hexView);
			this->tabPageHex->ImageKey = L"TableHS.png";
			this->tabPageHex->Location = System::Drawing::Point(4, 23);
			this->tabPageHex->Name = L"tabPageHex";
			this->tabPageHex->Size = System::Drawing::Size(672, 450);
			this->tabPageHex->TabIndex = 7;
			this->tabPageHex->Text = L"Hex";
			this->tabPageHex->ToolTipText = L"Hex View";
			this->tabPageHex->UseVisualStyleBackColor = true;
			// 
			// hexView
			// 
			this->hexView->BufferSize = 2048;
			this->hexView->DebugMode = true;
			this->hexView->Dock = System::Windows::Forms::DockStyle::Fill;
			this->hexView->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8.25F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(0)));
			this->hexView->HexCaps = true;
			this->hexView->Location = System::Drawing::Point(0, 0);
			this->hexView->Name = L"hexView";
			this->hexView->NumCols = 16;
			this->hexView->NumRows = 16;
			this->hexView->Size = System::Drawing::Size(672, 450);
			this->hexView->TabIndex = 0;
			// 
			// tabPageDebug
			// 
			this->tabPageDebug->Controls->Add(this->tabDebug);
			this->tabPageDebug->Location = System::Drawing::Point(4, 23);
			this->tabPageDebug->Name = L"tabPageDebug";
			this->tabPageDebug->Padding = System::Windows::Forms::Padding(3);
			this->tabPageDebug->Size = System::Drawing::Size(672, 450);
			this->tabPageDebug->TabIndex = 5;
			this->tabPageDebug->Text = L"Debug";
			this->tabPageDebug->UseVisualStyleBackColor = true;
			// 
			// tabDebug
			// 
			this->tabDebug->Controls->Add(this->tabForm);
			this->tabDebug->Controls->Add(this->tabFile);
			this->tabDebug->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tabDebug->Location = System::Drawing::Point(3, 3);
			this->tabDebug->Name = L"tabDebug";
			this->tabDebug->SelectedIndex = 0;
			this->tabDebug->Size = System::Drawing::Size(666, 444);
			this->tabDebug->TabIndex = 18;
			// 
			// tabForm
			// 
			this->tabForm->Controls->Add(this->btnClearDebug);
			this->tabForm->Controls->Add(this->textFormDebug);
			this->tabForm->Location = System::Drawing::Point(4, 22);
			this->tabForm->Name = L"tabForm";
			this->tabForm->Size = System::Drawing::Size(658, 443);
			this->tabForm->TabIndex = 0;
			this->tabForm->Text = L"Form";
			this->tabForm->UseVisualStyleBackColor = true;
			// 
			// btnClearDebug
			// 
			this->btnClearDebug->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnClearDebug->Location = System::Drawing::Point(544, 304);
			this->btnClearDebug->Name = L"btnClearDebug";
			this->btnClearDebug->Size = System::Drawing::Size(75, 23);
			this->btnClearDebug->TabIndex = 17;
			this->btnClearDebug->Text = L"Clear";
			this->btnClearDebug->Click += gcnew System::EventHandler(this, &Form1::btnClearDebug_Click);
			// 
			// textFormDebug
			// 
			this->textFormDebug->Location = System::Drawing::Point(8, 8);
			this->textFormDebug->MaxLength = 65535;
			this->textFormDebug->Multiline = true;
			this->textFormDebug->Name = L"textFormDebug";
			this->textFormDebug->ReadOnly = true;
			this->textFormDebug->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->textFormDebug->Size = System::Drawing::Size(616, 288);
			this->textFormDebug->TabIndex = 16;
			this->textFormDebug->Text = L"Debug Info Goes Here";
			// 
			// tabFile
			// 
			this->tabFile->Controls->Add(this->btnRefresh);
			this->tabFile->Controls->Add(this->txtDebug);
			this->tabFile->Location = System::Drawing::Point(4, 22);
			this->tabFile->Name = L"tabFile";
			this->tabFile->Size = System::Drawing::Size(658, 443);
			this->tabFile->TabIndex = 1;
			this->tabFile->Text = L"File Data";
			this->tabFile->UseVisualStyleBackColor = true;
			// 
			// btnRefresh
			// 
			this->btnRefresh->FlatStyle = System::Windows::Forms::FlatStyle::System;
			this->btnRefresh->Location = System::Drawing::Point(544, 304);
			this->btnRefresh->Name = L"btnRefresh";
			this->btnRefresh->Size = System::Drawing::Size(75, 23);
			this->btnRefresh->TabIndex = 16;
			this->btnRefresh->Text = L"Refresh";
			this->btnRefresh->Click += gcnew System::EventHandler(this, &Form1::btnRefresh_Click);
			// 
			// imageListTabs
			// 
			this->imageListTabs->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageListTabs.ImageStream")));
			this->imageListTabs->TransparentColor = System::Drawing::Color::Transparent;
			this->imageListTabs->Images->SetKeyName(0, L"Information.png");
			this->imageListTabs->Images->SetKeyName(1, L"Control_TreeView.png");
			this->imageListTabs->Images->SetKeyName(2, L"InsertPictureHS.png");
			this->imageListTabs->Images->SetKeyName(3, L"TaskHS.png");
			this->imageListTabs->Images->SetKeyName(4, L"TableHS.png");
			this->imageListTabs->Images->SetKeyName(5, L"Warning.png");
			this->imageListTabs->Images->SetKeyName(6, L"Error.png");
			// 
			// statusBar
			// 
			this->statusBar->Location = System::Drawing::Point(0, 505);
			this->statusBar->Name = L"statusBar";
			this->statusBar->Size = System::Drawing::Size(686, 20);
			this->statusBar->TabIndex = 5;
			// 
			// toolStripContainer1
			// 
			this->toolStripContainer1->BottomToolStripPanelVisible = false;
			// 
			// toolStripContainer1.ContentPanel
			// 
			this->toolStripContainer1->ContentPanel->AutoScroll = true;
			this->toolStripContainer1->ContentPanel->Controls->Add(this->statusBar);
			this->toolStripContainer1->ContentPanel->Controls->Add(this->panelMain);
			this->toolStripContainer1->ContentPanel->Size = System::Drawing::Size(686, 525);
			this->toolStripContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->toolStripContainer1->LeftToolStripPanelVisible = false;
			this->toolStripContainer1->Location = System::Drawing::Point(0, 0);
			this->toolStripContainer1->Name = L"toolStripContainer1";
			this->toolStripContainer1->RightToolStripPanelVisible = false;
			this->toolStripContainer1->Size = System::Drawing::Size(686, 574);
			this->toolStripContainer1->TabIndex = 7;
			this->toolStripContainer1->Text = L"toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this->toolStripContainer1->TopToolStripPanel->Controls->Add(this->menuStrip1);
			this->toolStripContainer1->TopToolStripPanel->Controls->Add(this->toolStripFile);
			this->toolStripContainer1->TopToolStripPanel->Controls->Add(this->toolStripNumFormat);
			this->toolStripContainer1->TopToolStripPanel->Controls->Add(this->toolStrip1);
			// 
			// panelMain
			// 
			this->panelMain->Controls->Add(this->tabControlViews);
			this->panelMain->Dock = System::Windows::Forms::DockStyle::Fill;
			this->panelMain->Location = System::Drawing::Point(0, 0);
			this->panelMain->Name = L"panelMain";
			this->panelMain->Padding = System::Windows::Forms::Padding(3, 3, 3, 20);
			this->panelMain->Size = System::Drawing::Size(686, 525);
			this->panelMain->TabIndex = 6;
			// 
			// menuStrip1
			// 
			this->menuStrip1->Dock = System::Windows::Forms::DockStyle::None;
			this->menuStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(6) {this->menuFile, this->menuEdit, 
				this->viewToolStripMenuItem, this->searchToolStripMenuItem, this->toolsToolStripMenuItem, this->helpToolStripMenuItem});
			this->menuStrip1->Location = System::Drawing::Point(0, 0);
			this->menuStrip1->Name = L"menuStrip1";
			this->menuStrip1->Size = System::Drawing::Size(686, 24);
			this->menuStrip1->TabIndex = 8;
			this->menuStrip1->Text = L"menuStrip1";
			// 
			// menuFile
			// 
			this->menuFile->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(3) {this->menuFileOpen, 
				this->menuFileClose, this->menuFileExit});
			this->menuFile->Name = L"menuFile";
			this->menuFile->Size = System::Drawing::Size(35, 20);
			this->menuFile->Text = L"File";
			// 
			// menuFileOpen
			// 
			this->menuFileOpen->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuFileOpen.Image")));
			this->menuFileOpen->Name = L"menuFileOpen";
			this->menuFileOpen->Size = System::Drawing::Size(111, 22);
			this->menuFileOpen->Text = L"Open";
			this->menuFileOpen->Click += gcnew System::EventHandler(this, &Form1::menuFileOpen_Click);
			// 
			// menuFileClose
			// 
			this->menuFileClose->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuFileClose.Image")));
			this->menuFileClose->Name = L"menuFileClose";
			this->menuFileClose->Size = System::Drawing::Size(111, 22);
			this->menuFileClose->Text = L"Close";
			this->menuFileClose->Click += gcnew System::EventHandler(this, &Form1::menuFileClose_Click);
			// 
			// menuFileExit
			// 
			this->menuFileExit->Name = L"menuFileExit";
			this->menuFileExit->Size = System::Drawing::Size(111, 22);
			this->menuFileExit->Text = L"Exit";
			this->menuFileExit->Click += gcnew System::EventHandler(this, &Form1::menuFileExit_Click);
			// 
			// menuEdit
			// 
			this->menuEdit->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(4) {this->menuEditCut, 
				this->menuEditCopy, this->menuEditPaste, this->menuEditDelete});
			this->menuEdit->Name = L"menuEdit";
			this->menuEdit->Size = System::Drawing::Size(37, 20);
			this->menuEdit->Text = L"Edit";
			// 
			// menuEditCut
			// 
			this->menuEditCut->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuEditCut.Image")));
			this->menuEditCut->Name = L"menuEditCut";
			this->menuEditCut->Size = System::Drawing::Size(116, 22);
			this->menuEditCut->Text = L"Cut";
			this->menuEditCut->Click += gcnew System::EventHandler(this, &Form1::menuEditCut_Click);
			// 
			// menuEditCopy
			// 
			this->menuEditCopy->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuEditCopy.Image")));
			this->menuEditCopy->Name = L"menuEditCopy";
			this->menuEditCopy->Size = System::Drawing::Size(116, 22);
			this->menuEditCopy->Text = L"Copy";
			this->menuEditCopy->Click += gcnew System::EventHandler(this, &Form1::menuEditCopy_Click);
			// 
			// menuEditPaste
			// 
			this->menuEditPaste->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuEditPaste.Image")));
			this->menuEditPaste->Name = L"menuEditPaste";
			this->menuEditPaste->Size = System::Drawing::Size(116, 22);
			this->menuEditPaste->Text = L"Paste";
			this->menuEditPaste->Click += gcnew System::EventHandler(this, &Form1::menuEditPaste_Click);
			// 
			// menuEditDelete
			// 
			this->menuEditDelete->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuEditDelete.Image")));
			this->menuEditDelete->Name = L"menuEditDelete";
			this->menuEditDelete->Size = System::Drawing::Size(116, 22);
			this->menuEditDelete->Text = L"Delete";
			// 
			// viewToolStripMenuItem
			// 
			this->viewToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->numberFormatToolStripMenuItem});
			this->viewToolStripMenuItem->Name = L"viewToolStripMenuItem";
			this->viewToolStripMenuItem->Size = System::Drawing::Size(41, 20);
			this->viewToolStripMenuItem->Text = L"View";
			// 
			// numberFormatToolStripMenuItem
			// 
			this->numberFormatToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(5) {this->menuNFDefault, 
				this->menuNFHexadecimal, this->menuNFDecimal, this->menuNFBinary, this->menuNFASCII});
			this->numberFormatToolStripMenuItem->Name = L"numberFormatToolStripMenuItem";
			this->numberFormatToolStripMenuItem->Size = System::Drawing::Size(159, 22);
			this->numberFormatToolStripMenuItem->Text = L"Number Format";
			// 
			// menuNFDefault
			// 
			this->menuNFDefault->CheckOnClick = true;
			this->menuNFDefault->Name = L"menuNFDefault";
			this->menuNFDefault->Size = System::Drawing::Size(145, 22);
			this->menuNFDefault->Text = L"Default";
			this->menuNFDefault->Click += gcnew System::EventHandler(this, &Form1::menuNFDefault_Click);
			// 
			// menuNFHexadecimal
			// 
			this->menuNFHexadecimal->CheckOnClick = true;
			this->menuNFHexadecimal->Name = L"menuNFHexadecimal";
			this->menuNFHexadecimal->Size = System::Drawing::Size(145, 22);
			this->menuNFHexadecimal->Text = L"Hexadecimal";
			this->menuNFHexadecimal->Click += gcnew System::EventHandler(this, &Form1::menuNFHexidecimal_Click);
			// 
			// menuNFDecimal
			// 
			this->menuNFDecimal->CheckOnClick = true;
			this->menuNFDecimal->Name = L"menuNFDecimal";
			this->menuNFDecimal->Size = System::Drawing::Size(145, 22);
			this->menuNFDecimal->Text = L"Decimal";
			this->menuNFDecimal->Click += gcnew System::EventHandler(this, &Form1::menuNFDecimal_Click);
			// 
			// menuNFBinary
			// 
			this->menuNFBinary->CheckOnClick = true;
			this->menuNFBinary->Name = L"menuNFBinary";
			this->menuNFBinary->Size = System::Drawing::Size(145, 22);
			this->menuNFBinary->Text = L"Binary";
			this->menuNFBinary->Click += gcnew System::EventHandler(this, &Form1::menuNFBinary_Click);
			// 
			// menuNFASCII
			// 
			this->menuNFASCII->CheckOnClick = true;
			this->menuNFASCII->Name = L"menuNFASCII";
			this->menuNFASCII->Size = System::Drawing::Size(145, 22);
			this->menuNFASCII->Text = L"ASCII";
			this->menuNFASCII->Click += gcnew System::EventHandler(this, &Form1::menuNFASCII_Click);
			// 
			// searchToolStripMenuItem
			// 
			this->searchToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->findToolStripMenuItem, 
				this->findNextToolStripMenuItem});
			this->searchToolStripMenuItem->Name = L"searchToolStripMenuItem";
			this->searchToolStripMenuItem->Size = System::Drawing::Size(52, 20);
			this->searchToolStripMenuItem->Text = L"Search";
			// 
			// findToolStripMenuItem
			// 
			this->findToolStripMenuItem->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"findToolStripMenuItem.Image")));
			this->findToolStripMenuItem->Name = L"findToolStripMenuItem";
			this->findToolStripMenuItem->Size = System::Drawing::Size(152, 22);
			this->findToolStripMenuItem->Text = L"Find";
			this->findToolStripMenuItem->Click += gcnew System::EventHandler(this, &Form1::menuSearchFind_Click);
			// 
			// findNextToolStripMenuItem
			// 
			this->findNextToolStripMenuItem->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"findNextToolStripMenuItem.Image")));
			this->findNextToolStripMenuItem->Name = L"findNextToolStripMenuItem";
			this->findNextToolStripMenuItem->Size = System::Drawing::Size(152, 22);
			this->findNextToolStripMenuItem->Text = L"Find Next";
			this->findNextToolStripMenuItem->Click += gcnew System::EventHandler(this, &Form1::menuSearchFindNext_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this->toolsToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->menuToolsFTM, 
				this->menuToolsOptions});
			this->toolsToolStripMenuItem->Name = L"toolsToolStripMenuItem";
			this->toolsToolStripMenuItem->Size = System::Drawing::Size(44, 20);
			this->toolsToolStripMenuItem->Text = L"Tools";
			// 
			// menuToolsFTM
			// 
			this->menuToolsFTM->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuToolsFTM.Image")));
			this->menuToolsFTM->Name = L"menuToolsFTM";
			this->menuToolsFTM->Size = System::Drawing::Size(173, 22);
			this->menuToolsFTM->Text = L"File Type Manager";
			this->menuToolsFTM->Click += gcnew System::EventHandler(this, &Form1::menuToolsFTM_Click);
			// 
			// menuToolsOptions
			// 
			this->menuToolsOptions->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"menuToolsOptions.Image")));
			this->menuToolsOptions->Name = L"menuToolsOptions";
			this->menuToolsOptions->Size = System::Drawing::Size(173, 22);
			this->menuToolsOptions->Text = L"Options";
			this->menuToolsOptions->Click += gcnew System::EventHandler(this, &Form1::menuToolsOptions_Click);
			// 
			// helpToolStripMenuItem
			// 
			this->helpToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->unToolStripMenuItem, 
				this->aboutToolStripMenuItem});
			this->helpToolStripMenuItem->Name = L"helpToolStripMenuItem";
			this->helpToolStripMenuItem->Size = System::Drawing::Size(40, 20);
			this->helpToolStripMenuItem->Text = L"Help";
			// 
			// unToolStripMenuItem
			// 
			this->unToolStripMenuItem->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"unToolStripMenuItem.Image")));
			this->unToolStripMenuItem->Name = L"unToolStripMenuItem";
			this->unToolStripMenuItem->Size = System::Drawing::Size(215, 22);
			this->unToolStripMenuItem->Text = L"Universal File Explorer Help";
			// 
			// aboutToolStripMenuItem
			// 
			this->aboutToolStripMenuItem->Name = L"aboutToolStripMenuItem";
			this->aboutToolStripMenuItem->Size = System::Drawing::Size(215, 22);
			this->aboutToolStripMenuItem->Text = L"About";
			this->aboutToolStripMenuItem->Click += gcnew System::EventHandler(this, &Form1::menuHelpAbout_Click);
			// 
			// toolStripFile
			// 
			this->toolStripFile->Dock = System::Windows::Forms::DockStyle::None;
			this->toolStripFile->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {this->tsbOpenFile, this->tsbCloseFile});
			this->toolStripFile->Location = System::Drawing::Point(3, 24);
			this->toolStripFile->Name = L"toolStripFile";
			this->toolStripFile->Size = System::Drawing::Size(58, 25);
			this->toolStripFile->TabIndex = 0;
			// 
			// tsbOpenFile
			// 
			this->tsbOpenFile->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbOpenFile->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbOpenFile.Image")));
			this->tsbOpenFile->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbOpenFile->Name = L"tsbOpenFile";
			this->tsbOpenFile->Size = System::Drawing::Size(23, 22);
			this->tsbOpenFile->Text = L"toolStripButton1";
			this->tsbOpenFile->Click += gcnew System::EventHandler(this, &Form1::menuFileOpen_Click);
			// 
			// tsbCloseFile
			// 
			this->tsbCloseFile->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbCloseFile->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbCloseFile.Image")));
			this->tsbCloseFile->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbCloseFile->Name = L"tsbCloseFile";
			this->tsbCloseFile->Size = System::Drawing::Size(23, 22);
			this->tsbCloseFile->Text = L"toolStripButton1";
			this->tsbCloseFile->Click += gcnew System::EventHandler(this, &Form1::menuFileClose_Click);
			// 
			// toolStripNumFormat
			// 
			this->toolStripNumFormat->Dock = System::Windows::Forms::DockStyle::None;
			this->toolStripNumFormat->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(7) {this->tsbDefault, 
				this->tsbHexadecimal, this->tsbDecimal, this->tsbBinary, this->tsbAscii, this->toolStripSeparator1, this->tsbTableWidth});
			this->toolStripNumFormat->Location = System::Drawing::Point(61, 24);
			this->toolStripNumFormat->Name = L"toolStripNumFormat";
			this->toolStripNumFormat->Size = System::Drawing::Size(156, 25);
			this->toolStripNumFormat->TabIndex = 1;
			// 
			// tsbDefault
			// 
			this->tsbDefault->CheckOnClick = true;
			this->tsbDefault->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbDefault->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbDefault.Image")));
			this->tsbDefault->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbDefault->Name = L"tsbDefault";
			this->tsbDefault->Size = System::Drawing::Size(23, 22);
			this->tsbDefault->ToolTipText = L"Default";
			this->tsbDefault->Click += gcnew System::EventHandler(this, &Form1::menuNFDefault_Click);
			// 
			// tsbHexadecimal
			// 
			this->tsbHexadecimal->CheckOnClick = true;
			this->tsbHexadecimal->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbHexadecimal->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbHexadecimal.Image")));
			this->tsbHexadecimal->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbHexadecimal->Name = L"tsbHexadecimal";
			this->tsbHexadecimal->Size = System::Drawing::Size(23, 22);
			this->tsbHexadecimal->Text = L"Hexadecimal";
			this->tsbHexadecimal->Click += gcnew System::EventHandler(this, &Form1::menuNFHexidecimal_Click);
			// 
			// tsbDecimal
			// 
			this->tsbDecimal->CheckOnClick = true;
			this->tsbDecimal->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbDecimal->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbDecimal.Image")));
			this->tsbDecimal->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbDecimal->Name = L"tsbDecimal";
			this->tsbDecimal->Size = System::Drawing::Size(23, 22);
			this->tsbDecimal->Text = L"Decimal";
			this->tsbDecimal->Click += gcnew System::EventHandler(this, &Form1::menuNFDecimal_Click);
			// 
			// tsbBinary
			// 
			this->tsbBinary->CheckOnClick = true;
			this->tsbBinary->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbBinary->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbBinary.Image")));
			this->tsbBinary->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbBinary->Name = L"tsbBinary";
			this->tsbBinary->Size = System::Drawing::Size(23, 22);
			this->tsbBinary->ToolTipText = L"Binary";
			this->tsbBinary->Click += gcnew System::EventHandler(this, &Form1::menuNFBinary_Click);
			// 
			// tsbAscii
			// 
			this->tsbAscii->CheckOnClick = true;
			this->tsbAscii->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbAscii->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbAscii.Image")));
			this->tsbAscii->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbAscii->Name = L"tsbAscii";
			this->tsbAscii->Size = System::Drawing::Size(23, 22);
			this->tsbAscii->ToolTipText = L"ASCII";
			this->tsbAscii->Click += gcnew System::EventHandler(this, &Form1::menuNFASCII_Click);
			// 
			// toolStripSeparator1
			// 
			this->toolStripSeparator1->Name = L"toolStripSeparator1";
			this->toolStripSeparator1->Size = System::Drawing::Size(6, 25);
			// 
			// tsbTableWidth
			// 
			this->tsbTableWidth->CheckOnClick = true;
			this->tsbTableWidth->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbTableWidth->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbTableWidth.Image")));
			this->tsbTableWidth->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbTableWidth->Name = L"tsbTableWidth";
			this->tsbTableWidth->Size = System::Drawing::Size(23, 22);
			this->tsbTableWidth->Text = L"Fit table data to window";
			this->tsbTableWidth->ToolTipText = L"Fit table data to window";
			this->tsbTableWidth->Click += gcnew System::EventHandler(this, &Form1::tsbTableWidth_Click);
			// 
			// toolStrip1
			// 
			this->toolStrip1->Dock = System::Windows::Forms::DockStyle::None;
			this->toolStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->tsbImageActualSize});
			this->toolStrip1->Location = System::Drawing::Point(217, 24);
			this->toolStrip1->Name = L"toolStrip1";
			this->toolStrip1->Size = System::Drawing::Size(35, 25);
			this->toolStrip1->TabIndex = 9;
			// 
			// tsbImageActualSize
			// 
			this->tsbImageActualSize->CheckOnClick = true;
			this->tsbImageActualSize->DisplayStyle = System::Windows::Forms::ToolStripItemDisplayStyle::Image;
			this->tsbImageActualSize->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"tsbImageActualSize.Image")));
			this->tsbImageActualSize->ImageTransparentColor = System::Drawing::Color::Magenta;
			this->tsbImageActualSize->Name = L"tsbImageActualSize";
			this->tsbImageActualSize->Size = System::Drawing::Size(23, 22);
			this->tsbImageActualSize->Text = L"toolStripButton1";
			this->tsbImageActualSize->Click += gcnew System::EventHandler(this, &Form1::tsbImageActualSize_Click);
			// 
			// imageListTreeView
			// 
			this->imageListTreeView->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"imageListTreeView.ImageStream")));
			this->imageListTreeView->TransparentColor = System::Drawing::Color::Transparent;
			this->imageListTreeView->Images->SetKeyName(0, L"Null.png");
			this->imageListTreeView->Images->SetKeyName(1, L"Section.png");
			this->imageListTreeView->Images->SetKeyName(2, L"Properties.png");
			this->imageListTreeView->Images->SetKeyName(3, L"Header.png");
			this->imageListTreeView->Images->SetKeyName(4, L"Comment.png");
			this->imageListTreeView->Images->SetKeyName(5, L"Text.png");
			this->imageListTreeView->Images->SetKeyName(6, L"Table.png");
			this->imageListTreeView->Images->SetKeyName(7, L"Image.png");
			this->imageListTreeView->Images->SetKeyName(8, L"FolderOpen.png");
			this->imageListTreeView->Images->SetKeyName(9, L"FolderClosed.png");
			this->imageListTreeView->Images->SetKeyName(10, L"Document.png");
			this->imageListTreeView->Images->SetKeyName(11, L"Object.png");
			this->imageListTreeView->Images->SetKeyName(12, L"Gear.png");
			this->imageListTreeView->Images->SetKeyName(13, L"Binary.png");
			this->imageListTreeView->Images->SetKeyName(14, L"Information.png");
			this->imageListTreeView->Images->SetKeyName(15, L"Palette.png");
			// 
			// contextMenuStripTreeNodeRightClick
			// 
			this->contextMenuStripTreeNodeRightClick->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->menuTnViewFileData});
			this->contextMenuStripTreeNodeRightClick->Name = L"contextMenuStripTreeNodeRightClick";
			this->contextMenuStripTreeNodeRightClick->Size = System::Drawing::Size(153, 26);
			// 
			// menuTnViewFileData
			// 
			this->menuTnViewFileData->Name = L"menuTnViewFileData";
			this->menuTnViewFileData->Size = System::Drawing::Size(152, 22);
			this->menuTnViewFileData->Text = L"View File Data";
			this->menuTnViewFileData->Click += gcnew System::EventHandler(this, &Form1::menuTnViewFileData_Click);
			// 
			// Form1
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(686, 574);
			this->Controls->Add(this->toolStripContainer1);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MainMenuStrip = this->menuStrip1;
			this->Name = L"Form1";
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"Universal File Explorer";
			this->Load += gcnew System::EventHandler(this, &Form1::Form1_Load);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->picFile))->EndInit();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->dataGrid1))->EndInit();
			this->tabControlViews->ResumeLayout(false);
			this->tabPageQuickInfo->ResumeLayout(false);
			this->tlpQuickInfo->ResumeLayout(false);
			this->tableLayoutPanel5->ResumeLayout(false);
			this->tableLayoutPanel5->PerformLayout();
			this->tableLayoutPanel3->ResumeLayout(false);
			this->flowLayoutPanel2->ResumeLayout(false);
			this->grpFileAttributes->ResumeLayout(false);
			this->groupBoxFileDates->ResumeLayout(false);
			this->tableLayoutPanel4->ResumeLayout(false);
			this->tableLayoutPanel4->PerformLayout();
			this->tabPageTechView->ResumeLayout(false);
			this->panel1->ResumeLayout(false);
			this->tabPageGraphicView->ResumeLayout(false);
			this->tabFileCheck->ResumeLayout(false);
			this->tabFileCheck->PerformLayout();
			this->tabPageHex->ResumeLayout(false);
			this->tabPageDebug->ResumeLayout(false);
			this->tabDebug->ResumeLayout(false);
			this->tabForm->ResumeLayout(false);
			this->tabForm->PerformLayout();
			this->tabFile->ResumeLayout(false);
			this->tabFile->PerformLayout();
			this->toolStripContainer1->ContentPanel->ResumeLayout(false);
			this->toolStripContainer1->TopToolStripPanel->ResumeLayout(false);
			this->toolStripContainer1->TopToolStripPanel->PerformLayout();
			this->toolStripContainer1->ResumeLayout(false);
			this->toolStripContainer1->PerformLayout();
			this->panelMain->ResumeLayout(false);
			this->menuStrip1->ResumeLayout(false);
			this->menuStrip1->PerformLayout();
			this->toolStripFile->ResumeLayout(false);
			this->toolStripFile->PerformLayout();
			this->toolStripNumFormat->ResumeLayout(false);
			this->toolStripNumFormat->PerformLayout();
			this->toolStrip1->ResumeLayout(false);
			this->toolStrip1->PerformLayout();
			this->contextMenuStripTreeNodeRightClick->ResumeLayout(false);
			this->ResumeLayout(false);

		}	
	private: 
		System::Void Form1_Load(System::Object^ sender, System::EventArgs^  e)
		{
				debugMode = DEFAULT_DEBUG;

				DebugOut(L"-Application Vars-");
				DebugOut(L"ProductName: ", Application::ProductName);
				DebugOut(L"ProductVersion: ", Application::ProductVersion);
				DebugOut(L"CompanyName: ", Application::CompanyName);
				DebugOut(L"CurrentCulture: ", Application::CurrentCulture->ToString());
				DebugOut(L"ExecutablePath: ", Application::ExecutablePath);
				DebugOut(L"CommonAppDataPath: ", Application::CommonAppDataPath);
				//DebugOut(L"CommonAppDataRegistry: ", Application::CommonAppDataRegistry->ToString());
				DebugOut(L"UserAppDataPath: ", Application::UserAppDataPath);
				DebugOut(L"UserAppDataRegistry: ", Application::UserAppDataRegistry->ToString());

				//DebugOut( String::Concat( L"Loaded ModuleL", Assembly::GetLoadedModules ) );
				
				// This should fix the bug where the horizontal 
				// scrollbar always appears in the TreeView
				tabControlViews->SelectedTab = tabPageTechView;
				//tabControlViews->set_SelectedTab(tabPageQuickInfo);

				if(m_openFileAfterLoad)
				{
					OpenFile(m_filePath);
					m_openFileAfterLoad = false;
				}
				
				if(!m_fileIsOpen)
				{
					HideShowTabs(true, true, false, false, false);
				}
				//tvFile->Refresh();
				HideLoadDialog();
			 }


/********************************************

	My Variables

********************************************/


// Parent Node
	public:	 Object^ pApp;

// Command Line Params
	private: bool debugMode;

	private: UFEDebug^ m_debug;

 // Variable for the file data
	private: Ufex::API::FileType^ m_fileData;
			 
			 FileTypeManager^ m_fileTypeMan;

			 Ufex::API::DataFormatter^ m_nts;

 // Variable for the file stream
	private: FileStream^ m_fileStream;
			 String^ m_filePath;

	private: bool m_fileIsOpen;			// true if the file stream is open
			 bool m_fileLoaded;			// true if there is a file loaded in UFE
			 bool m_fileDataUsed;		// true if the file type is known and has been processed and stored in memory
			 bool m_imageAvailable;		// true if the file that is open is an image
			 bool m_imageLoaded;		// true if an image is loaded in the image box
			 bool m_keepFileOpen;		// true if the FileStream should not be closed
			 bool m_openFileAfterLoad;	// true if a file was passed in the cmd line args
			 bool hexbug1;
	
	private: bool FormattedViewSelected;
			 bool m_closingFile;			// true if a file is being closed

	private: Ufex::API::NumberFormat m_numFormat;

	private: bool m_techViewTableExpand;	// Determines how the columns fit in the window

	private: int size_Form1_W;
			 int size_Form1_H;
			 int size_tabControlViews_W;
			 int size_tabControlViews_H;
			 int size_tvFile_W;
			 int size_tvFile_H;
			 int size_dataGrid1_W;
			 int size_dataGrid1_H;
			 int size_picFile_W;
			 int size_picFile_H;
			 int size_dataGridQI_W;
			 int size_dataGridQI_H;
	private: double splitPercent;
	private: FileStreamSearch^ m_fileStreamSearch;		// nullptr If no search has been run

///////////////////////////////////////////////////////////
// The function defs for these functions are in Form1.cpp//
///////////////////////////////////////////////////////////

	// AppInit - Initializes the application
	private: void AppInit(String^ cmdLineParam);

	private: void HideLoadDialog();

	// ResizeAll - Resizes all the objects on the form
	private: void ResizeAll();

	// OpenFile
	private: int OpenFile(String^ file);
	private: void OpenFileFromDialog();

	private: void CloseFile(bool hideUnusedTabs);

	private: void ExitUFE(bool closeFile);

	private: void ChangeNumFormat(Ufex::API::NumberFormat gcnewNF);

			 // Show Dialogs
	private: void ShowFileTypeMan();
			 void ShowOptions();
			 void ShowAbout();

			 // Initialization Functions
	private: void InitControlSizes();
			 void InitFTM();
			 void InitDebug(String^ cmdLineParam);
			 void InitDataGrid();
			 void InitAppSettings();

	private: void ClearTreeView(TreeView^ tv) { tv->BeginUpdate(); tv->Nodes->Clear(); tv->EndUpdate(); };
			 inline void ClearTextBox(TextBox^ txt) { txt->Text = BLANK_STRING; };

	private: inline void SetStatusBar(String^ text) { statusBar->Text = text; };

			 // Debugging functions
	private: void DebugOut(String^ t1);
			 void DebugOut(String^ t1, String^ t2);
			 void DebugOut(String^ t1, String^ t2, String^ t3);
			 void ExceptionOut(Exception^ e);

	// Functions called by the openFile function
	private: void SetQuickInfoTextBoxes();
			 void LoadFileCheckInfo();
			 void LoadTreeNodeIcons(bool hasIcons);
			 void LoadQuickInfoTable();
			 void SelectFirstNodeInTreeView();
			 void HideShowTabs(bool showQuickInfo, bool showHex, bool showTechnical, 
							 bool showImage, bool showFileCheck);
			 void HideShowTab(bool show, TabPage^ tp);

			 // Technical View Functions
	private:
			void RefreshTechTableData();


private: System::Void menuFileOpen_Click(System::Object^ sender, System::EventArgs^ e)
		 {	OpenFileFromDialog();	}
	
		// Code for when a TreeItem is Selected
private: System::Void tvFile_AfterSelect(System::Object ^  sender, System::Windows::Forms::TreeViewEventArgs ^  e)
		 {
			// This prevents loading table data for each node in the 
			//		tree as nodes are removed when the file is closed
			if(m_closingFile)
				return;

			RefreshTechTableData();
		 }

private: System::Void menuFile_Click(System::Object^ sender, System::EventArgs^ e)
		 {}

private: System::Void menuHelpAbout_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ShowAbout();	}

private: System::Void menuToolsOptions_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ShowOptions();	}

private: System::Void menuToolsFTM_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ShowFileTypeMan();	}

private: System::Void menuFileExit_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ExitUFE(true);	}

private: System::Void menuFileClose_Click(System::Object^ sender, System::EventArgs^ e)
		 {	CloseFile(true);	}

private: System::Void menuNFHexidecimal_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ChangeNumFormat(NF_HEX);	}

private: System::Void menuNFDecimal_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ChangeNumFormat(NF_DEC);	}
	
private: System::Void menuNFBinary_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ChangeNumFormat(NF_BIN);	}

private: System::Void menuNFASCII_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ChangeNumFormat(NF_ASC);	}

private: System::Void menuNFDefault_Click(System::Object^ sender, System::EventArgs^ e)
		 {	ChangeNumFormat(NF_DEF);	}

private: System::Void Form1_Resize(System::Object^ sender, System::EventArgs^ e)
		 {	ResizeAll();	}

private: System::Void tvFile_KeyPress(System::Object^  sender, System::Windows::Forms::KeyPressEventArgs^ e)
		 {

		 }

private: System::Void tvFile_KeyDown(System::Object ^  sender, System::Windows::Forms::KeyEventArgs ^  e)
		 {

		 }

private: System::Void tabControlViews_SelectedIndexChanged(System::Object ^  sender, System::EventArgs ^  e)
		 {
			//DebugOut(String::Concat(L"Tab Control Clicked: ", tabControlViews->SelectedIndex.ToString()));
			
			// Unload the Image
			
			/*
			if(picFile->Image != nullptr && tabControlViews->SelectedIndex != TAB_FORMATTED)
			{
				picFile->Image->Dispose();
				picFile->Image = nullptr;
				ImageLoaded = false;
				if(!KeepFileOpen)
				{
					fs->Close();
					FileIsOpen = false;
				}
				DebugOut(L"Unloaded Image");
			}
			*/

			// Perform the actions for the tab that was clicked
			if(tabControlViews->SelectedTab == nullptr)
			{
				DebugOut(L"!! tabControlViews_SelectedIndexChanged - Error nullptr Tab");
				return;
			}
			else if(tabControlViews->SelectedTab->Equals(tabPageTechView))
			{	
				// Code for when technical view is clicked

			}
			else if(tabControlViews->SelectedTab->Equals(tabPageGraphicView))
			{
				if(m_imageAvailable && !m_imageLoaded)
				{
					try
					{	
						picFile->Image = m_fileData->GetImage();
						m_imageLoaded = true;
						DebugOut(L"Image Loaded");
					}
					catch(Exception^ e)
					{
						ExceptionOut(e);
						DebugOut(String::Concat(L"Error drawing Image: ", e->ToString()));
						//m_fileStream->Close();
						m_fileIsOpen = false;
						m_imageLoaded = false;
					}
				}
			}
			else if(tabControlViews->SelectedTab->Equals(tabPageHex))
			{
				// Redraw the hex data 
				//(Fixes the bug where no data is 
				// drawn the first time the tab is clicked)
				hexView->RedrawData();
			}
		 }
		
private: System::Void splitterTreeData_Moved(System::Object ^  sender, System::Windows::Forms::SplitterEventArgs^  e)
		{			
			splitPercent = (double)tvFile->Width / (double)tabControlViews->Width;
			return;
		}


	private: System::Void menuEditCopy_Click(System::Object ^  sender, System::EventArgs ^  e)
			 {
			    if(tabControlViews->SelectedTab->Equals(nullptr))
				{
					return;
				}
				else if(tabControlViews->SelectedTab->Equals(tabPageTechView))
				{	
					if(tvFile->ContainsFocus)
					{
						Clipboard::SetDataObject(tvFile->SelectedNode->Text, SAVE_CLIPBOARD);
					}
					else if(dataGrid1->ContainsFocus && dataGrid1->DataSource != nullptr)
					{
						Clipboard::SetDataObject(dataGrid1[dataGrid1->CurrentCell]->ToString(), SAVE_CLIPBOARD);
					}
				}
				else if(tabControlViews->SelectedTab->Equals(tabPageQuickInfo))
				{
					if(textFilePath->ContainsFocus)
					{
						Clipboard::SetDataObject(textFilePath->Text, SAVE_CLIPBOARD);
					}
					else if(textFileName->ContainsFocus)
					{
						Clipboard::SetDataObject(textFileName->Text, SAVE_CLIPBOARD);
					}
				}
			 }

private: System::Void menuEditCut_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {
		 }

private: System::Void menuEditPaste_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {
		 }

private: 
		System::Void menuEdit_Click(System::Object ^  sender, System::EventArgs ^  e)
		{
			menuEditCopy->Enabled = false;
			menuEditCut->Enabled = false;
			menuEditPaste->Enabled = false;

			if(tabControlViews->SelectedTab->Equals(nullptr))
			{
				return;
			}
			else if(tabControlViews->SelectedTab->Equals(tabPageTechView))
			{	
				if(tvFile->ContainsFocus)
					menuEditCopy->Enabled = true;
				else if(dataGrid1->ContainsFocus && dataGrid1->DataSource != nullptr)
					menuEditCopy->Enabled = true;
			}
			else if(tabControlViews->SelectedTab->Equals(tabPageQuickInfo))
			{
				if(textFilePath->ContainsFocus || textFileName->ContainsFocus || textFileExt->ContainsFocus || textFileSize->ContainsFocus)
					menuEditCopy->Enabled = true;
				else if(listViewQuickInfo->ContainsFocus)
					menuEditCopy->Enabled = true;
			}
		}

private: System::Void btnRefresh_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {									}

private: System::Void btnClearDebug_Click(System::Object ^  sender, System::EventArgs^  e) 
		 {	ClearTextBox(textFormDebug);	}

private: System::Void menuSearchFind_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {
			 m_fileStreamSearch = gcnew FileStreamSearch(m_fileStream);
			 SearchDialog^ frmSearch = gcnew SearchDialog();
			 frmSearch->m_fileStreamSearch = m_fileStreamSearch;
			 frmSearch->hexView = hexView;
			 frmSearch->Show();
		 }

private: System::Void menuSearchFindNext_Click(System::Object ^  sender, System::EventArgs ^  e)
		 {
			 if(m_fileStreamSearch == nullptr)
				 return;

			 Int64 nextPos = m_fileStreamSearch->FindNext();
			 if(nextPos == -1)
				 MessageBox::Show(L"The specified search data was not found");
			 else
			 {
				hexView->GotoPosition(nextPos);
				hexView->Highlight(nextPos, nextPos + m_fileStreamSearch->GetSearchDataLen());
			 }
		 }

	private: System::Void Form1_Closing(System::Object ^  sender, System::ComponentModel::CancelEventArgs^  e)
			 {  ExitUFE(true); }

	private: System::Void listViewQuickInfo_SelectedIndexChanged(System::Object^  sender, System::EventArgs^  e) 
			 {
			 
			 }

private: System::Void tsbImageActualSize_Click(System::Object^  sender, System::EventArgs^  e) 
		 {
			 if(tsbImageActualSize->Checked)
				picFile->SizeMode = PictureBoxSizeMode::Normal;
			 else
				 picFile->SizeMode = PictureBoxSizeMode::StretchImage;
		 }

		 ///
		 ///
		 ///
private: System::Void tvFile_NodeMouseClick(System::Object^  sender, System::Windows::Forms::TreeNodeMouseClickEventArgs^  e) 
		 {
			 if(e->Button == ::MouseButtons::Right)
			 {
				 if(e->Node->Tag->GetType() == TreeNodeTag::typeid)
				 {
					TreeNodeTag^ tnTag = static_cast<TreeNodeTag^>(e->Node->Tag);
					
					e->Node->ContextMenuStrip = contextMenuStripTreeNodeRightClick;
					e->Node->ContextMenuStrip->Tag = tnTag;
					if(tnTag->FileRegion != nullptr)
						e->Node->ContextMenuStrip->Items[L"menuTnViewFileData"]->Enabled = true;
					else
						e->Node->ContextMenuStrip->Items[L"menuTnViewFileData"]->Enabled = false;

				 }

			 }
		 }

		/// <summary>
		/// Displays the hex view tab page and selects 
		/// the file data specified by the tree nodes tag.
		/// </summary>
private: System::Void menuTnViewFileData_Click(System::Object^  sender, System::EventArgs^  e) 
		 {
			 try
			 {
				tabControlViews->SelectedTab = tabPageHex;
				TreeNodeTag^ tnTag = static_cast<TreeNodeTag^>(contextMenuStripTreeNodeRightClick->Tag);
				hexView->Highlight(tnTag->FileRegion->StartPosition, tnTag->FileRegion->EndPosition);
				hexView->GotoPosition(tnTag->FileRegion->StartPosition);
			 }
			 catch(Exception^ e)
			 {
				ExceptionOut(e);
			 }
		 }


private: System::Void tsbTableWidth_Click(System::Object^  sender, System::EventArgs^  e) 
		 { m_techViewTableExpand = tsbTableWidth->Checked; }


	}; // end of Class

}; // end of Namespace


