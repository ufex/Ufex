using System;
using System.Collections.Generic;
using System.Resources;
using System.Drawing;
using System.Reflection;
using System.IO;
using Ufex.API.Tree;
using Ufex.API.Validation;
using Ufex.API.Format;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ufex.API;

/// <summary>
/// Base class for all File Types
/// </summary>
public abstract class FileType : IFileType
{
	public string m_DebugText;
	public string m_AppPath;
	private FileStream _fileStream;
	private string _filePath;

	private Ufex.API.Tables.QuickInfoTableData _quickInfoTableData;

	private TreeNode _rootTreeNode;

	private ValidationReport _validationReport;
	
	private ArrayToNum _arrayToNum;

	private DataFormatter _dataFormatter;

	private List<Ufex.API.Visual.Visual> _visuals;

	private ILogger _logger;

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

	public Ufex.API.Tables.QuickInfoTableData QuickInfoTable
	{
		get { return _quickInfoTableData; }
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
		set { _visuals = value; }
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
	/// Determines whether the Visual Tab is displayed
	/// </summary>
	public bool EnableVisual { get; protected set; }

	/// <summary>
	/// Determines whether the Structure Tab is displayed
	/// </summary>
	public bool EnableStructure { get; protected set; }

	/// <summary>
	/// Determines whether the Validation View Tab is displayed
	/// </summary>
	public bool EnableValidation { get; protected set; }

	/// <summary>
	/// Determines whether the Structure View Tab is displayed
	/// </summary>
	[Obsolete("Use EnableStructure instead")]
	public Boolean ShowTechnical { get => EnableStructure; protected set { EnableStructure = value; } }

	/// <summary>
	/// Determines whether the Preview View Tab is displayed
	/// </summary>
	[Obsolete("Use EnableVisual instead")]
	public Boolean ShowGraphic { get => EnableVisual; protected set { EnableVisual = value; } }

	/// <summary>
	/// Determines whether the Validation View Tab is displayed
	/// </summary>
	[Obsolete("Use EnableValidation instead")]
	public Boolean ShowFileCheck { get => EnableValidation; protected set { EnableValidation = value; } }

	/// <summary>
	/// Logger instance for the FileType. Defaults to NullLogger.Instance.
	/// The application should set this to an appropriate logger instance.
	/// </summary>
	public ILogger Logger
	{
		get { return _logger; }
		set { _logger = value ?? NullLogger.Instance; }
	}

	/// <summary>
	/// Logger instance for the FileType (for backwards compatibility with derived classes).
	/// </summary>
	//[Obsolete("Use the Logger property instead")]
	protected Ufex.API.Logger Log
	{
		get { return (Logger)_logger; }
	}

	public FileType()
	{
		_logger = NullLogger.Instance;
		
		// By default these tabs are hidden
		ShowTechnical = false;
		ShowGraphic = false;
		ShowFileCheck = false;

		// Initialize the QuickInfoTableData
		_quickInfoTableData = new Ufex.API.Tables.QuickInfoTableData();

		// Initialize ValidationReport
		_validationReport = new ValidationReport();

		// Initialize the Visuals list
		_visuals = new List<Ufex.API.Visual.Visual>();

		// Create the ArrayToNum instance
		_arrayToNum = new ArrayToNum();

		// Create an instance of the DataFormatter Class
		_dataFormatter = new DataFormatter();

		// Set the number format for the DataFormatter
		_dataFormatter.NumFormat = NumberFormat.Default;

		_rootTreeNode = new TreeNode("ROOT");
	}

	abstract public bool ProcessFile();


	// Functions for interfacing with the ArrayToNum Class
	public void SetATNEndian(Endian endian) 
	{ 
		_arrayToNum.DataEndian = endian == Endian.Little ? Endian.Little : Endian.Big; 
	}
}
