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
	private FileStream _fileStream;
	private string _filePath;

	private TreeNode _rootTreeNode;

	private ValidationReport _validationReport;
	
	private ArrayToNum _arrayToNum;

	private DataFormatter _dataFormatter;

	private List<Ufex.API.Visual.Visual> _visuals;

	private Logger log;

	public string FilePath
	{
		get { return _filePath; }
		set { _filePath = value; }
	}

	/// <summary>
	/// The input file stream
	/// </summary>
	public FileStream FileInStream
	{
		get { return _fileStream; }
		set { _fileStream = value; }
	}

	public NumberFormat NumFormat
	{
		get { return _dataFormatter.NumFormat; }
		set { _dataFormatter.NumFormat = value; }
	}

	/// <summary>
	/// Tree nodes to display in the Structure Tab Tree View
	/// </summary>
	public TreeNodeCollection TreeNodes
	{
		get { return _rootTreeNode.Nodes; }
	}

	public ValidationReport ValidationReport
	{
		get { return _validationReport; }
	}

	public Ufex.API.Visual.Visual[] Visuals
	{
		get { return _visuals.ToArray(); }
	}

	protected List<Ufex.API.Visual.Visual> VisualsList
	{
		get { return _visuals; }
		protected set { _visuals = value; }
	}

	protected DataFormatter NTS
	{
		get { return _dataFormatter; }
	}

	protected ArrayToNum ATN
	{
		get { return _arrayToNum; }
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
		_validationReport = new ValidationReport();

		// Create the ArrayToNum instance
		_arrayToNum = new ArrayToNum();

		// Create an instance of the DataFormatter Class
		_dataFormatter = new DataFormatter();

		// Set the number format for the DataFormatter
		_dataFormatter.NumFormat = NumberFormat.Default;

		// By default these tabs are hidden
		ShowTechnical = false;
		ShowGraphic = false;
		ShowFileCheck = false;

		_rootTreeNode = new TreeNode("ROOT");
	}

	~FileType()
	{
		m_DebugText = null;

		// Free the file stream
		if (_fileStream != null)
		{
			_fileStream.Close();
			_fileStream = null;
		}

		// Delete the trees
		if (_rootTreeNode != null)
		{
			//m__rootTreeNode.Nodes.Clear();
			_rootTreeNode = null;
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
		_arrayToNum.DataEndian = endian == Endian.Little ? Endian.Little : Endian.Big; 
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
