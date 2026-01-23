using System;
using System.Collections;
using System.Resources;
using System.Drawing;
using System.Reflection;
using System.IO;

using Ufex.API.Tree;

namespace Ufex.API;

public abstract class FileType
{
	public string m_DebugText;
	public string m_AppPath;
	public FileStream m_FileStream;
	private string filePath;

	private TreeNode rootTreeNode;

	private FileCheckInfo m_fileCheckInfo;
	
	private ArrayToNum arrayToNum;
	private DataFormatter dataFormatter;

	private ArrayList icons;
	
	private bool m_useIcons;

	private string m_appPath;

	private Logger log;

	public string FilePath
	{
		get { return this.filePath; }
		set { this.filePath = value; }
	}

	public FileStream FileInStream
	{
		get { return m_FileStream; }
	}

	public NumberFormat NumFormat
	{
		set { dataFormatter.SetNumFormat(value); }
	}

	public TreeNodeCollection TreeNodes
	{
		get { return rootTreeNode.Nodes; }
	}

	public FileCheckInfo FileCheck
	{
		get { return m_fileCheckInfo; }
	}

	protected DataFormatter NTS
	{
		get { return dataFormatter; }
	}

	protected ArrayToNum ATN
	{
		get { return arrayToNum; }
	}

	public String ApplicationPath
	{
		get { return m_appPath; }
	}

	public string Description { get; protected set; }

	public Boolean UseTreeViewIcons
	{
		get { return m_useIcons; }
		protected set { m_useIcons = value; }
	}

	// Determines whether the Technical View Tab is displayed
	public Boolean ShowTechnical { get; protected set; }

	public Boolean ShowGraphic { get; protected set; }

	public Boolean ShowFileCheck { get; protected set; }

	[Obsolete("Use Log instead")]
	protected Logger Debug
	{
		get { return log; }
	}

	protected Logger Log
	{
		get { return log; }
	}

	public FileType()
	{
		log = new Logger();
		m_DebugText = "";

		// Initialize FileCheckInfo
		m_fileCheckInfo = new FileCheckInfo();

		// Create the ArrayToNum instance
		arrayToNum = new ArrayToNum();

		// Create an instance of the NumToString Class
		dataFormatter = new DataFormatter();

		// Set the number format for the NumToString
		dataFormatter.NumFormat = NumberFormat.Default;

		icons = new ArrayList();

		m_useIcons = false;
		//LoadIcons();

		// By default these tabs are hidden
		ShowTechnical = false;
		ShowGraphic = false;
		ShowFileCheck = false;

		rootTreeNode = new TreeNode("ROOT");
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
		if (rootTreeNode != null)
		{
			//m_rootTreeNode.Nodes.Clear();
			rootTreeNode = null;
		}
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
		return icons.Add(ico); 
	}

	public int GetNumIcons() 
	{
		return icons.Count; 
	}

	public Icon GetIcon(int i) { 
		return (Icon)icons[i]; 
	}

	// Functions for interfacing with the ArrayToNum Class
	public void SetATNEndian(Endian endian) 
	{ 
		arrayToNum.DataEndian = endian == Endian.Little ? Endian.Little : Endian.Big; 
	}

	// ExceptionOut(Exception* e)
	//		Adds an exception to the DebugInfo
	protected void ExceptionOut(Exception e)
	{
		log.NewException(e);
	}

	protected void ExceptionOut(Exception e, string className)
	{
		log.NewException(e, className);
	}

	protected void ExceptionOut(Exception e, string className, string funcName)
	{
		log.NewException(e, className, funcName);
	}

	protected void DebugOut(string NewText)
	{
		if (!m_DebugText.Equals(""))
			m_DebugText = m_DebugText + "\r\n" + NewText;
		else
			m_DebugText = NewText;

		log.Info(NewText);
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
