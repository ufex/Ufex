// Form1.cpp
//
//
//

#include "stdafx.h"
#include "Form1.h"
#include "./UniversalFileExplorerApp.h"

#undef MessageBox

// DataTable Default Settings
#define DT_ROW_HEADERS_VISIBLE		false
#define DT_ALTERNATING_BACK_COLOR	SystemColors::Window
#define DT_BACK_COLOR				SystemColors::Window
#define DT_FORE_COLOR				Color::Black
#define DT_GRID_LINE_COLOR			Color::RoyalBlue
#define DT_HEADER_BACK_COLOR		Color::MidnightBlue
#define DT_HEADER_FORE_COLOR		Color::LavenderBlush
#define DT_LINK_COLOR				Color::Teal
#define DT_SELECTION_BACK_COLOR		Color::Teal
#define DT_SELECTION_FORE_COLOR		Color::PaleGreen


namespace UniversalFileExplorer
{
	void Form1::AppInit(String^ cmdLineParam)
	{	
		m_debug = gcnew UFEDebug(L"Form1.log");

		DebugOut(L"-AppInit(", cmdLineParam, L")-");

		ProfessionalColorTable^ pct = gcnew ProfessionalColorTable();
		pct->UseSystemColors = true;
		ToolStripProfessionalRenderer^ tspr = gcnew ToolStripProfessionalRenderer(pct);
		ToolStripManager::Renderer = tspr;

		picFile->Image = imageListTabs->Images[0];


		m_fileIsOpen = false;
		m_fileLoaded = false;
		hexbug1 = true;
		m_nts = gcnew Ufex::API::DataFormatter("en-US");

		m_techViewTableExpand = false;
		
		// Initialize the control sizes
		InitControlSizes();

		// Initialize the file type manager
		InitFTM();

		// Initialize the debug
		InitDebug(cmdLineParam);

		InitDataGrid();

		// Sets any settings needed from Settings class
		InitAppSettings();
		openFileDialog->Filter = OPEN_FILE_FILTER;
		DebugOut(String::Concat(L"Command Line Params = \"", cmdLineParam, L"\""));
//		NumFormat = Settings::GetSetting(L"NumFormat";
		//DebugOut("----------");
		DebugOut(cmdLineParam->IndexOf(L"\"").ToString());
		
		m_openFileAfterLoad = false;

		if(cmdLineParam->IndexOf(L"\"") >= 0)
		{
			String^ FilePath = cmdLineParam->Substring(cmdLineParam->IndexOf(L"\""), cmdLineParam->Length);
			
			FilePath = FilePath->Remove(0,1);
			if(FilePath->IndexOf(L"\"") >= 0)
			{				
				FilePath = FilePath->Remove(FilePath->IndexOf(L"\""), FilePath->Length - FilePath->IndexOf(L"\""));
				DebugOut(FilePath);
				m_fileLoaded = true;
				m_fileIsOpen = true;
				m_fileDataUsed = false;
				m_imageAvailable = false;
				m_filePath = FilePath;
				m_openFileAfterLoad = true;

				if(!m_fileDataUsed)
				{
					DebugOut(L"AppInit1: FileDataUsed = false");
				}

			}
		}
		else if(!cmdLineParam->Equals(L"") && File::Exists(cmdLineParam))
		{
			String^ FilePath = cmdLineParam;
			DebugOut(FilePath);
			m_fileLoaded = true;
			m_fileIsOpen = true;
			m_fileDataUsed = false;
			m_imageAvailable = false;
			
			m_openFileAfterLoad = true;
			m_filePath = FilePath;
			if(!m_fileDataUsed)
			{
				DebugOut(L"AppInit2: FileDataUsed = false");
			}
		}
		else
		{
			
		}
		
		DebugOut(L"-End AppInit-");
	}

	int Form1::CheckRunMode()
	{
		return 0;
	}

	void Form1::HideLoadDialog()
	{
		(static_cast<UniversalFileExplorerApp^>(pApp))->CloseLoadDialog();
	}

	/************************************************
		Open a file

	*************************************************/
	int Form1::OpenFile(String^ file)
	{
		if(file == nullptr)
		{
			DebugOut(L"!!openFile(NULL) was called");
			return -1;
		}
		//UFETimer* time1 = gcnew UFETimer();

		//time1->Start();

		// Set the File Path
		m_filePath = file;

		// Set the path in the title bar
		this->Text = String::Concat(L"Universal File Explorer - ", m_filePath);

		// Start up the file manager
		SetStatusBar(L"Opening file - Identifying file type");
		
		// Try to identify the file type
		FILETYPE^ fileType = m_fileTypeMan->GetFileType(m_filePath);

		if(fileType != nullptr)
		{
			// Set the file type description text box
			txtFileType->Text = fileType->Description;
		}
		else
			txtFileType->Text = L"Unknown file type";

		// Open the file stream
		SetStatusBar(L"Opening file - Opening File Stream");
		try
		{
			m_fileStream = gcnew FileStream(m_filePath, FileMode::Open, FileAccess::Read);
			m_fileIsOpen = true;
		}
		catch(Exception^ e)
		{					
			MessageBox::Show(String::Concat(L"An error occured while opening the specified file: \r\n", e->Message), 
					L"Unable to open file", MessageBoxButtons::OK, MessageBoxIcon::Error);
			ExceptionOut(e);
			DebugOut(String::Concat(L"Error: ", e->Message));
			m_fileIsOpen = false;
			return -1;
		}

		// Set the Text boxes on the quick info page
		SetQuickInfoTextBoxes();

		// Load HexView
		SetStatusBar(L"Opening file - Loading HexView");
		hexView->LoadFileStream(m_fileStream);


		// Redraw the hex data 
		//	Fixes the bug where no data is drawn 
		//  if the you are on the hex view tab the 
		//	first time you open a file
		if(hexbug1 && tabControlViews->SelectedTab->Equals(tabPageHex))
		{
			hexView->RedrawData();
			hexbug1 = false;
		}

		/* If the type is known load the module specified 
			in the types bind field */
		if(fileType != nullptr)
		{
			// If there is a binding: load the module
			array<FILETYPE_CLASS^>^ fileTypeClasses = m_fileTypeMan->GetFileTypeClassesByFileType(fileType->ID);
			if(fileTypeClasses->Length > 0)
			{
				FILETYPE_CLASS^ fileTypeClass = fileTypeClasses[0];
				try
				{
					m_fileData = m_fileTypeMan->GetNewClassInstance(fileTypeClass->ID);
				}
				catch(Exception^ e)
				{
					MessageBox::Show(e->Message, L"Error Loading Module", MessageBoxButtons::OK, MessageBoxIcon::Error);
					DebugOut(L"!!Error Loading Module: ", fileTypeClass->ID);
					ExceptionOut(e);
					statusBar->Text = L"";
					return -1;
				}

				// If the class could not be loaded
				if(m_fileData == nullptr)
				{
					DebugOut(String::Concat(L"Failed to load type: ", fileTypeClass->ID));
					SetStatusBar(L"");
					return -1;
				}
				
				m_fileData->m_AppPath = Path::GetDirectoryName(Application::ExecutablePath);
				
				// The position is set to 0 prior to calling process file
				m_fileStream->Position = 0;
				m_fileData->m_FileStream = m_fileStream;

				SetStatusBar(L"Opening file - Processing file data");
				
				//MessageBox::Show(L"Before Process File", 
				//	L"Before process file", MessageBoxButtons::OK, MessageBoxIcon::Error);

				try
				{
					bool success = m_fileData->ProcessFile();
				}
//				catch(FileCorruptException* e)
//				{
//					MessageBox::Show(e->Message, L"File is corrupt", MessageBoxButtons::OK, MessageBoxIcon::Error);
//					DebugOut(L"FileCorruptException");
//				}
				catch(Exception^ e)
				{
					//e->ToString();
					String^ errMsg;
					//if(debugMode)
						errMsg = String::Concat(e->Message, NEW_LINE, e->StackTrace, NEW_LINE, e->Source, NEW_LINE, e->TargetSite);
					//else
					//	errMsg = e->Message;

					MessageBox::Show(errMsg, L"Error processing file", MessageBoxButtons::OK, MessageBoxIcon::Error);
				}
				//MessageBox::Show(L"After Process File", 
				//	L"After process file", MessageBoxButtons::OK, MessageBoxIcon::Error);

				m_fileStream = m_fileData->m_FileStream;
				
				hexView->fileStream = m_fileStream;
				
				// Hide/Show the tabs
				HideShowTabs(true, true, m_fileData->ShowTechnical, m_fileData->ShowGraphic, m_fileData->ShowFileCheck);

				m_fileDataUsed = true;
				
				m_imageAvailable = true;

				m_fileData->NumFormat = m_numFormat;
				
				// Load File Check Info
				LoadFileCheckInfo();

				// Load the images for the TreeNodes
				LoadTreeNodeIcons(m_fileData->UseTreeViewIcons);

				// Load the TreeNodes
				SetStatusBar(L"Opening file - Adding TreeNode'L");

				// Start Update of tree, this will stop refreshing the tree until EndUpdate is called
				tvFile->BeginUpdate();
				for(int jt = 0; jt < m_fileData->TreeNodes->Count; jt++)
				{
					tvFile->Nodes->Add(m_fileData->TreeNodes[jt]);
				}			

				// Set the Quick Info
				SetStatusBar(L"Opening file - Loading QuickInfo");
				LoadQuickInfoTable();
				
				try
				{	
					SetStatusBar(L"Opening file - Loading Image");
					picFile->Image = m_fileData->GetImage();
					m_imageLoaded = true;
					DebugOut(L"Image Loaded");
				}
				catch(Exception^ e)
				{
					ExceptionOut(e);
					DebugOut(String::Concat(L"Error drawing Image: ", e->ToString()));
					m_fileStream->Close();
					m_fileIsOpen = false;
					m_imageLoaded = false;
				}
			
				txtDebug->Text = m_fileData->m_DebugText;

				//if(m_fileData->m_KeepOpen)
				//{
				m_fileIsOpen = true;
				m_keepFileOpen = true;
				//}
				//else
				//{
				//	fs->Close();
				//	KeepFileOpen = false;
				//	FileIsOpen = false;
				//}
			}
			else	// bind is set to false for this type
			{
				HideShowTabs(true, true, false, false, false);
			}
			m_fileLoaded = true;
		}
		else
		{
			HideShowTabs(true, true, false, false, false);
		}

		// End the TreeView Update
		tvFile->EndUpdate();

		// Select the first node in the tree if there are any
		SelectFirstNodeInTreeView();

		//time1->Stop();
		//statusBar->Text = String::Concat(L"Finished loading file in ", time1->GetTime().ToString(), L" mL"));
		//DebugOut(String::Concat(L"Finished loading file in ", time1->GetTime().ToString(), L" mL"));

		// Reset Status bar text
		if(!debugMode)
			SetStatusBar(L"");

		return 0;
	}

	void Form1::OpenFileFromDialog()
	{
		// Show the Open File Dialog and get the file path
		if(openFileDialog->ShowDialog() == Windows::Forms::DialogResult::OK)
		{
			CloseFile(false);
			m_fileData = nullptr;
			m_fileLoaded = true;
			m_fileIsOpen = true;
			m_fileDataUsed = false;
			m_imageAvailable = false;
			OpenFile(openFileDialog->FileName);
		}
	}

	void Form1::CloseFile(bool hideUnusedTabs)
	{
		statusBar->Text = L"Closing file";
		m_closingFile = true;
		DebugOut(L"closeFile()");
		
		if(m_fileLoaded)
		{		
			if(hideUnusedTabs)
				HideShowTabs(true, true, false, false, false);
		
			// Set the path on the quick info page
			textFilePath->Text = BLANK_STRING;

			// Set the file name on the quick info page
			textFileName->Text = BLANK_STRING;

			// Set the file extension on the quick info page
			textFileExt->Text = BLANK_STRING;

			textFileSize->Text = BLANK_STRING;
			
			// Set all the checkboxes to false
			chkAttReadOnly->Checked = false;
			chkAttHidden->Checked = false;
			chkAttSystem->Checked = false;
			chkAttArchive->Checked = false;
			chkAttNormal->Checked = false;
			chkAttTemporary->Checked = false;
			chkAttSparseFile->Checked = false;
			chkAttEncrypted->Checked = false;
			chkAttCompressed->Checked = false;

			// Delete any existing nodes from the tree
			ClearTreeView(tvFile);

			// Clear Image in PictureBox  (Release Memory)
			if(picFile->Image != nullptr)
			{
				picFile->Image = nullptr;
				m_imageLoaded = false;
			}
		
			// Clear Quick Info Grid
			listViewQuickInfo->Items->Clear();
			listViewQuickInfo->Columns->Clear();
			
			// Clear DataGrid
			dataGrid1->DataSource = nullptr;

			// Unload HexView
			hexView->UnloadFile();
			hexView->Refresh();
			
			// Close the FileStream if it's open
			if(m_fileStream != nullptr)
				m_fileStream->Close();

			if(m_fileStreamSearch != nullptr)
				m_fileStreamSearch = nullptr;

			m_fileIsOpen = false;
			
			// Remove the m_fileData from memory
			if(m_fileData != nullptr)
			{
				m_fileData->~FileType();					
				delete m_fileData;
			}
			m_fileData = nullptr;
			m_fileLoaded = false;
			
			// Clear Text Boxes
			txtDebug->Text = BLANK_STRING;
			txtFileType->Text = BLANK_STRING;
			textFileCheck->Text = BLANK_STRING;

			this->Text = L"Universal File Explorer";

			statusBar->Text = L"Performing Garbage Collection";
			System::GC::Collect();
		}
		m_closingFile = false;
		statusBar->Text = L"";
	}

	void Form1::ExitUFE(bool closeFile)
	{
		if(closeFile)
			CloseFile(false);
		
		if(m_fileTypeMan != nullptr)
		{
			try
			{
				delete m_fileTypeMan;
			}
			catch(...)
			{

			}
		}

		Application::Exit();
	}


	void Form1::ResizeAll()
	{

		return;

		if(!debugMode)
			statusBar->Text = L"Resizing Form";
		
		//DebugOut(L"Form1_Resize");
		int newFormW = this->Width;
		int newFormH = this->Height;

		// Adjust the size of the control view
		tabControlViews->Width = newFormW - (size_Form1_W - size_tabControlViews_W);
		tabControlViews->Height = newFormH - (size_Form1_H - size_tabControlViews_H);
/*
		// Adjust the size of the File Check Text Box
		textFileCheck->set_Width(tabControlViews->Width - 30);
		textFileCheck->set_Height(tabControlViews->Height - 50);

		// Adjust the graphic control
		picFile->set_Width(tabControlViews->Width - 30);
		picFile->set_Height(tabControlViews->Height - 50);

		// Move splitter to the middle (or die trying)
		tvFile->set_Width((int)Math::Round((splitPercent * (double)tabControlViews->Width)));
		tvFile->set_Height(tabControlViews->Height - (size_tabControlViews_H - size_tvFile_H));
		dataGrid1->set_Height(tabControlViews->Height - (size_tabControlViews_H - size_dataGrid1_H));
		
		// The hard Part that always gets fucked up
		panel1->set_Width(tabControlViews->Width - dataGrid1->get_Right());
		dataGrid1->set_Width(panel1->Width - 15);
		
		if(debugMode)
		{
			String^ s1 = String::Concat(L"tab.W = ", (tabControlViews->Width).ToString(), L", ");
			String^ s2 = String::Concat(L"dat.W = ", (dataGrid1->Width).ToString(), L", ");
			String^ s3 = String::Concat(L"dat.R = ", (dataGrid1->get_Right()).ToString(), L", ");
			String^ s4 = String::Concat(L"pan.W = ", (panel1->Width).ToString(), L", ");
			String^ s5 = String::Concat(L"pan.R = ", (panel1->get_Right()).ToString(), L", ");
			String^ s6 = String::Concat(L"trv.W = ", (tvFile->Width).ToString(), L", ");
			String^ s7 = String::Concat(L"trv.R = ", (tvFile->get_Right()).ToString(), L" ");
			statusBar->Text = String::Concat(L"Resize: ", s1, s2, s3, s4, s5, s6, s7);
		}
		else
		{
			statusBar->Text = L"";
		}
		*/
		return;
	}

	void Form1::ChangeNumFormat(Ufex::API::NumberFormat newNF)
	{	
		menuNFDefault->Checked = false;
		menuNFBinary->Checked = false;
		menuNFDecimal->Checked = false;
		menuNFHexadecimal->Checked = false;
		menuNFASCII->Checked = false;

		tsbDefault->Checked = false;
		tsbDecimal->Checked = false;
		tsbHexadecimal->Checked = false;
		tsbBinary->Checked = false;
		tsbAscii->Checked = false;
		
		switch(newNF)
		{
		case Ufex::API::NumberFormat::Default:
			menuNFDefault->Checked = true;
			tsbDefault->Checked = true;
			break;
		
		case Ufex::API::NumberFormat::Hexadecimal:
			menuNFHexadecimal->Checked = true;
			tsbHexadecimal->Checked = true;
			break;

		case Ufex::API::NumberFormat::Decimal:
			menuNFDecimal->Checked = true;
			tsbDecimal->Checked = true;
			break;

		case Ufex::API::NumberFormat::Binary:
			menuNFBinary->Checked = true;
			tsbBinary->Checked = true;
			break;

		case Ufex::API::NumberFormat::Ascii:
			menuNFASCII->Checked = true;
			tsbAscii->Checked = true;
			break;

		}

		// Set the Number Format at the Form-Level
		m_numFormat = newNF;


		if(m_fileLoaded && m_fileData != nullptr)
		{
			// Set the Number Format for the m_fileData
			m_fileData->NumFormat = m_numFormat;
			RefreshTechTableData();
		}
	}

	// Refreshes the technical view table data
	//
	//
	//
	void Form1::RefreshTechTableData()
	{
		if(m_fileData == nullptr)
			return;

		if(tvFile->SelectedNode == nullptr)
			return;

		Ufex::API::Tables::TableData^ myData;
		try
		{
			myData = m_fileData->GetData(tvFile->SelectedNode);
		}
		catch(Exception^ ex)
		{
			ExceptionOut(ex);
		}
		
		if(myData != nullptr)
		{
			dataGrid1->TableStyles->Clear();
			dataGrid1->TableStyles->Add(myData->GetDataGridTableStyle(dataGrid1->Width));
			m_nts->NumFormat = m_numFormat;
			dataGrid1->DataSource = myData->GetDataTable(m_nts);
		}
		else
		{
			dataGrid1->DataSource = nullptr;
		}
		
		try
		{
			// Resize the columns to fit if needed
			if(!m_techViewTableExpand)
				GUIUtil::ResizeColumnsToFit(dataGrid1);
			
			dataGrid1->Refresh();
		}
		catch(Exception^ e)
		{
			ExceptionOut(e);
		}
	}
	
	void Form1::ShowFileTypeMan()
	{
		//this->Enabled = false;
		FTMForm^ frmFTM = gcnew FTMForm(m_fileTypeMan);
		frmFTM->ShowDialog(this);
		//this->Enabled = true;
	}

	/// <summary>
	/// Displays the Options dialog.
	/// </summary>
	void Form1::ShowOptions()
	{
		Options^ frmOptions = gcnew Options();
		frmOptions->ShowDialog(this);
		CheckRunMode();
	}

	void Form1::ShowAbout()
	{
		About^ frmAbout = gcnew About();
		frmAbout->m_BuildType = (static_cast<UniversalFileExplorerApp^>(pApp))->gBT();
		frmAbout->m_DebugMode = debugMode;
		frmAbout->ShowDialog(this);
		CheckRunMode();
	}

	void Form1::SetQuickInfoTextBoxes()
	{
		// Set the path on the quick info page
		textFilePath->Text = m_filePath;

		// Set the file name on the quick info page
		textFileName->Text = Path::GetFileName(m_filePath);

		// Set the file extension on the quick info page
		textFileExt->Text = Path::GetExtension(m_filePath);

		// Set the file attributes check boxes
		IO::FileAttributes fa = File::GetAttributes(m_filePath);
		
		chkAttReadOnly->Checked =	((int)(fa & IO::FileAttributes::ReadOnly) >= 1);
		chkAttHidden->Checked =		((int)(fa & IO::FileAttributes::Hidden) >= 1);
		chkAttSystem->Checked =		((int)(fa & IO::FileAttributes::System) >= 1);
		chkAttArchive->Checked =	((int)(fa & IO::FileAttributes::Archive) >= 1);
		chkAttNormal->Checked =		((int)(fa & IO::FileAttributes::Normal) >= 1);
		chkAttTemporary->Checked =	((int)(fa & IO::FileAttributes::Temporary) >= 1);
		chkAttSparseFile->Checked = ((int)(fa & IO::FileAttributes::SparseFile) >= 1);
		chkAttEncrypted->Checked =	((int)(fa & IO::FileAttributes::Encrypted) >= 1);
		chkAttCompressed->Checked = ((int)(fa & IO::FileAttributes::Compressed) >= 1);
	
		// Set the file size on the quick info page
	
		NumberFormatInfo^ nfi = gcnew NumberFormatInfo();
		nfi->NumberGroupSeparator = L",";
		nfi->NumberDecimalDigits = 0;
		if(m_fileStream != nullptr)
			textFileSize->Text = m_fileStream->Length.ToString(L"N", nfi);

		// Set the file dates
		lblFileCreatedDate->Text = File::GetCreationTime(m_filePath).ToString();
		lblFileModifiedDate->Text = File::GetLastWriteTime(m_filePath).ToString();
		lblFileAccessedDate->Text = File::GetLastAccessTime(m_filePath).ToString();
	}

	void Form1::LoadFileCheckInfo()
	{
		Ufex::API::FileCheckInfo^ fci = m_fileData->FileCheck;
		array<String^>^ fileCheckInfoText = fci->GetInfo();
		if(fileCheckInfoText != nullptr)
		{
			textFileCheck->Lines = fileCheckInfoText;

			if(fci->HasErrors())
				tabFileCheck->ImageKey = L"Error.png";
			else if(fci->HasWarnings())
				tabFileCheck->ImageKey = L"Warning.png";
			else
				tabFileCheck->ImageKey = L"TaskHS.png";
		}
		else
		{
			textFileCheck->Text = L"";
			DebugOut(L"Error: FileCheckInfo is nullptr");
		}
	}


	void Form1::LoadTreeNodeIcons(bool hasIcons)
	{
		if(hasIcons)
			tvFile->ImageList = imageListTreeView;
		else
			tvFile->ImageList = nullptr;
	}

	void Form1::LoadQuickInfoTable()
	{
		Ufex::API::Tables::TableData^ quickInfoTD = m_fileData->GetQuickInfo();
		if(quickInfoTD != nullptr)
		{
			quickInfoTD->GetColumnHeaderCollection(listViewQuickInfo);
			quickInfoTD->GetListViewItemCollection(listViewQuickInfo, nullptr);
		}
		quickInfoTD = nullptr;
	}

	void Form1::SelectFirstNodeInTreeView()
	{
		// Select the first node in the tree if there are any
		if(tvFile->Nodes->Count > 0)
		{
			TreeNode^ firstNode = tvFile->Nodes[0];
			tvFile->SelectedNode = firstNode;
			firstNode = nullptr;
		}
	}

	
	void Form1::InitControlSizes()
	{	
		size_Form1_W = this->Width;
		size_Form1_H = this->Height;

		size_tabControlViews_W = tabControlViews->Width;
		size_tabControlViews_H = tabControlViews->Height;
		size_tvFile_W = tvFile->Width;
		size_tvFile_H = tvFile->Height;
		size_dataGrid1_W = dataGrid1->Width;
		size_dataGrid1_H = dataGrid1->Height;
		size_picFile_W = picFile->Width;
		size_picFile_H = picFile->Height;
		splitPercent = (double)tvFile->Width / (double)tabControlViews->Width;
		return;
	}

	void Form1::InitFTM()
	{
		try
		{
			// Initialize File Type Manager
			m_fileTypeMan = gcnew FileTypeManager(Path::GetDirectoryName(Application::ExecutablePath));
		}
		catch(Exception^ e)
		{
			MessageBox::Show(e->Message, L"Error initializing File Type Manager", MessageBoxButtons::OK, MessageBoxIcon::Error);
		}
	}

	void Form1::InitDebug(String^ cmdLineParam)
	{
		// Process Command Line Params
		if(cmdLineParam->IndexOf(L"/DEBUG") != -1 || cmdLineParam->IndexOf(L"\"DEBUG\"") != -1)
			debugMode = true;
		else
			debugMode = DEFAULT_DEBUG;

		tabPageDebug->Visible = debugMode;
		tabPageDebug->Enabled = debugMode;
		
		if(!debugMode)
			tabControlViews->TabPages->Remove(tabPageDebug);
	
		hexView->DebugMode = debugMode;

	}
	
	void Form1::InitDataGrid()
	{
		DataGridTableStyle^ dgts = gcnew DataGridTableStyle();
		dgts->MappingName = L"UFE_TABLE";
		dgts->RowHeadersVisible = DT_ROW_HEADERS_VISIBLE;
		dgts->AlternatingBackColor = DT_ALTERNATING_BACK_COLOR;
		dgts->BackColor = DT_BACK_COLOR;
		dgts->ForeColor = DT_FORE_COLOR;
		dgts->GridLineColor = DT_GRID_LINE_COLOR;
		dgts->HeaderBackColor = DT_HEADER_BACK_COLOR;
		dgts->HeaderForeColor = DT_HEADER_FORE_COLOR;
		dgts->LinkColor = DT_LINK_COLOR;
		dgts->SelectionBackColor = DT_SELECTION_BACK_COLOR;
		dgts->SelectionForeColor = DT_SELECTION_FORE_COLOR;
		dataGrid1->TableStyles->Add(dgts);
	}

	void Form1::InitAppSettings()
	{
		// Set the default number format
		String^ defNumFormat = Settings::GetSetting(L"NumFormat", L"Default");

		if(defNumFormat == nullptr)
			ChangeNumFormat(NF_DEF);
		else
		{
			defNumFormat = defNumFormat->ToUpper();
			if(defNumFormat->Equals(L"HEX"))
				ChangeNumFormat(NF_HEX);
			else if(defNumFormat->Equals(L"DEC"))
				ChangeNumFormat(NF_DEC);
			else if(defNumFormat->Equals(L"BIN"))
				ChangeNumFormat(NF_BIN);
			else if(defNumFormat->Equals(L"ASC"))
				ChangeNumFormat(NF_ASC);
			else
				ChangeNumFormat(NF_DEF);
		}
		

	}

	void Form1::HideShowTabs(bool showQuickInfo, bool showHex, bool showTechnical, 
							 bool showImage, bool showFileCheck)
	{
		TabPage^ selectedTab = tabControlViews->SelectedTab;

		HideShowTab(showQuickInfo, tabPageQuickInfo);
		HideShowTab(showHex, tabPageHex);
		HideShowTab(showTechnical, tabPageTechView);
		HideShowTab(showImage, tabPageGraphicView);
		HideShowTab(showFileCheck, tabFileCheck);
		
		if(selectedTab == nullptr)
			tabControlViews->SelectedTab = tabPageQuickInfo;
		else if(tabControlViews->TabPages->Contains(selectedTab))
			tabControlViews->SelectedTab = selectedTab;
		else
			tabControlViews->SelectedTab = tabPageQuickInfo;
	}

	void Form1::HideShowTab(bool show, TabPage^ tp)
	{
		if(!show && tabControlViews->TabPages->Contains(tp))
		{
//			bool tc = false;
//			if(tabControlViews->SelectedTab->Equals(tp))
//			{
//				tc = true;
//				tabControlViews->set_SelectedTab(tabPageQuickInfo);
//			}
			tabControlViews->TabPages->Remove(tp);
//			if(tc)
//			{
//				tabControlViews->set_SelectedTab(tabPageQuickInfo);
//			}
		}
		else if(show && !tabControlViews->TabPages->Contains(tp))
		{
			tabControlViews->TabPages->Add(tp);
		}
	}

	void Form1::DebugOut(String^ t1)
	{
		DebugOut(t1, BLANK_STRING, BLANK_STRING);
	}

	void Form1::DebugOut(String^ t1, String^ t2)
	{
		DebugOut(t1, t2, L"");
	}

	void Form1::DebugOut(String^ t1, String^ t2, String^ t3)
	{
		if(debugMode)
		{
			if(!textFormDebug->Text->Equals(BLANK_STRING))
				textFormDebug->Text = String::Concat(textFormDebug->Text, NEW_LINE, String::Concat(t1, t2, t3));
			else
				textFormDebug->Text = String::Concat(t1, t2, t3);
		}
		if(m_debug != nullptr)
			m_debug->NewInfo(String::Concat(t1, t2, t3));
	}

	
	void Form1::ExceptionOut(Exception^ e)
	{
		if(m_debug != nullptr)
			m_debug->NewException(e);
	}

};
