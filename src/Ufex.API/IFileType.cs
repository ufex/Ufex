using System.IO;
using Ufex.API.Validation;
using Ufex.API.Format;
using Ufex.API.Tree;

namespace Ufex.API;

public interface IFileType
{
	public string FilePath { get; set; }
	public FileStream FileInStream { get; set; }
	public NumberFormat NumFormat {	get; set; }
	public Ufex.API.Tables.QuickInfoTableData QuickInfoTable { get;	}
	public TreeNodeCollection TreeNodes { get; }
	public Ufex.API.Visual.Visual[] Visuals { get; }

	public bool ProcessFile();
}