using System;
using System.Collections;
using System.Resources;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using UniversalFileExplorer;
using System.IO;

namespace Ufex.API
{
	public abstract class FileType
	{
		public String m_DebugText;
		public String m_AppPath;
		public FileStream m_FileStream;
		private String m_filePath;

		private TreeNode m_rootTreeNode;

		private String m_description;

		private NumberFormat m_numFormat;

		private FileCheckInfo m_fileCheckInfo;
		
		private ArrayToNum m_atn;
		private DataFormatter m_nts;

		private ArrayList m_Icons;
		
		private bool m_useIcons;

		private String m_appPath;

		// Tabs to be displayed
		private bool m_showTechnical;
		private bool m_showGraphic;
		private bool m_showFileCheck;

		// 
		private Logger m_debug;

		public FileType()
		{
			m_debug = new Logger();
			m_DebugText = "";

			// Initialize FileCheckInfo
			m_fileCheckInfo = new FileCheckInfo();

			// Create the ArrayToNum instance
			m_atn = new ArrayToNum();

			// Create an instance of the NumToString Class
			m_nts = new DataFormatter();

			// Set the number format for the NumToString
			m_nts.NumFormat = NumberFormat.Default;

			m_Icons = new ArrayList();

			m_useIcons = false;
			//LoadIcons();

			// By default these tabs are hidden
			m_showTechnical = false;
			m_showGraphic = false;
			m_showFileCheck = false;

			m_rootTreeNode = new TreeNode("ROOT");
		}

		~FileType()
		{
			m_DebugText = null;

			// Free the file stream
			if (m_FileStream != null)
			{
				m_FileStream.Close();
				m_FileStream = null;
			}

			// Delete the trees
			if (m_rootTreeNode != null)
			{
				//m_rootTreeNode.Nodes.Clear();
				m_rootTreeNode = null;
			}
		}

		public String FilePath 
		{
			get { return this.m_filePath; }
			set { this.m_filePath = value; }
		}

		public FileStream FileInStream {
			get { return m_FileStream; }
		}

		public NumberFormat NumFormat 
		{
			set { m_nts.SetNumFormat(value); }
		}

		public TreeNodeCollection TreeNodes 
		{
			get { return m_rootTreeNode.Nodes; }
		}

		public FileCheckInfo FileCheck 
		{
			get { return m_fileCheckInfo; }
		}

		protected DataFormatter NTS 
		{
			get { return m_nts; }
		}

		protected ArrayToNum ATN 
		{
			get { return m_atn; }
		}

		public String ApplicationPath 
		{
			get { return m_appPath; }
		}

		public String Description 
		{
			get { return m_description; }
			protected set { m_description = value; }
		}

		public Boolean UseTreeViewIcons 
		{
			get { return m_useIcons; }
			protected set { m_useIcons = value; }
		}
		
		// Determines whether the Technical View Tab is displayed
		public Boolean ShowTechnical 
		{
			get { return m_showTechnical; }
			protected set { m_showTechnical = value; }
		}
		
		public Boolean ShowGraphic 
		{
			get { return m_showGraphic; }
			protected set { m_showGraphic = value; }
		}

		public Boolean ShowFileCheck 
		{
			get { return m_showFileCheck; }
			protected set { m_showFileCheck = value; }
		}

		[Obsolete("Use Log instead")]
		protected Logger Debug 
		{
			get { return m_debug; }
		}

		protected Logger Log
		{
			get { return m_debug; }
		}

		abstract public bool ProcessFile();

		// Returns the data table that corresponds to the treenode
		virtual public Ufex.API.Tables.TableData GetData(TreeNode tn)
		{
			return null;
		}

		// Returns the Quick Info for the file
		virtual public Ufex.API.Tables.QuickInfoTableData GetQuickInfo()
		{
			return null;
		}

		// Returns the File's Creator
		virtual public String GetFileCreator()
		{
			return "";
		}

		// Returns an image representation of the file
		virtual public Image GetImage()
		{
			return null;
		}

		// Add an icon - returns the icon id number
		public int AddIcon(Icon ico) 
		{ 
			return m_Icons.Add(ico); 
		}

		public int GetNumIcons() 
		{
			return m_Icons.Count; 
		}

		public Icon GetIcon(int i) { 
			return (Icon)m_Icons[i]; 
		}

		// Functions for interfacing with the ArrayToNum Class
		public void SetATNEndian(Endian endian) 
		{ 
			m_atn.DataEndian = endian == Endian.Little ? UniversalFileExplorer.Endian.Little : UniversalFileExplorer.Endian.Big; 
		}

		// ExceptionOut(Exception* e)
		//		Adds an exception to the DebugInfo
		protected void ExceptionOut(Exception e)
		{
			m_debug.NewException(e);
		}

		protected void ExceptionOut(Exception e, String className)
		{
			m_debug.NewException(e, className);
		}

		protected void ExceptionOut(Exception e, String className, String funcName)
		{
			m_debug.NewException(e, className, funcName);
		}

		protected void DebugOut(String NewText)
		{
			if (!m_DebugText.Equals(""))
				m_DebugText = m_DebugText + "\r\n" + NewText;
			else
				m_DebugText = NewText;

			m_debug.Info(NewText);
		}

		private void LoadIcons()
		{
			string[] iconNames = {
				"Null.ico",
				"Circle.ico",
				"Square.ico",
				"Table.ico",
				"Properties.ico",
				"Text.ico",
				"Script.ico",
				"Book.ico",
				"Objects.ico",
				"FolderOpen.ico",
				"FolderClosed.ico"
			};

			ResourceManager resourceManager = new ResourceManager("FileType.ResourceFiles", Assembly.GetExecutingAssembly());
			try
			{
				for (int i = 0; i < iconNames.Length; i++)
					AddIcon((Icon)resourceManager.GetObject(iconNames[i]));
			}
			catch (Exception e)
			{
				this.DebugOut(e.Message);
				m_useIcons = false;
			}
		}

	}
}
