using System;
using System.Collections.Generic;
using System.Resources;
using System.Drawing;
using System.Reflection;
using System.IO;
using Ufex.API.Tree;
using Ufex.API.Validation;
using System.Diagnostics;

namespace Ufex.API;

/// <summary>
/// Base class for all File Types
/// </summary>
public abstract class FileType
{
	public string m_DebugText;
	public string m_AppPath;
	public FileStream m_FileStream;
	private string filePath;

	private TreeNode rootTreeNode;

	private ValidationReport validationReport;
	
	private ArrayToNum arrayToNum;

	private DataFormatter dataFormatter;

	private List<Ufex.API.Visual.Visual> visuals;

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
		set { dataFormatter.NumFormat = value; }
	}

	public TreeNodeCollection TreeNodes
	{
		get { return rootTreeNode.Nodes; }
	}

	public ValidationReport ValidationReport
	{
		get { return validationReport; }
	}

	public Ufex.API.Visual.Visual[] Visuals
	{
		get { return visuals.ToArray(); }
	}

	protected DataFormatter NTS
	{
		get { return dataFormatter; }
	}

	protected ArrayToNum ATN
	{
		get { return arrayToNum; }
	}

	public string Description { get; protected set; }

	/// <summary>
	/// Determines whether the Structure View Tab is displayed
	/// </summary>
	public Boolean ShowTechnical { get; protected set; }

	/// <summary>
	/// Determines whether the Preview View Tab is displayed
	/// </summary>
	public Boolean ShowGraphic { get; protected set; }

	/// <summary>
	/// Determines whether the Validation View Tab is displayed
	/// </summary>
	public Boolean ShowFileCheck { get; protected set; }

	/// <summary>
	/// Logger instance for the FileType
	/// </summary>
	protected Logger Log
	{
		get { return log; }
	}

	public FileType()
	{
		log = new Logger("FileTypeInstance.log");
		m_DebugText = "";

		// Initialize ValidationReport
		validationReport = new ValidationReport();

		// Create the ArrayToNum instance
		arrayToNum = new ArrayToNum();

		// Create an instance of the DataFormatter Class
		dataFormatter = new DataFormatter();

		// Set the number format for the DataFormatter
		dataFormatter.NumFormat = NumberFormat.Default;

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
	virtual public Ufex.API.Tables.TableData? GetData(TreeNode tn)
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

}
