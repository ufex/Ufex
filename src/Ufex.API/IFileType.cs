using System.IO;
using Ufex.API.Validation;
using Ufex.API.Format;
using Ufex.API.Tree;
using Microsoft.Extensions.Logging;

namespace Ufex.API;

public interface IFileType
{
	public string FilePath { get; set; }
	public FileStream FileInStream { get; set; }
	public NumberFormat NumFormat {	get; set; }
	public Ufex.API.Tables.QuickInfoTableData QuickInfoTable { get;	}
	public TreeNodeCollection TreeNodes { get; }
	public Ufex.API.Visual.Visual[] Visuals { get; }

	/// <summary>
	/// Logger instance for the FileType. Defaults to NullLogger.Instance.
	/// The application should set this to an appropriate logger instance.
	/// </summary>
	public ILogger Logger { get; set; }

	public bool ProcessFile();
}