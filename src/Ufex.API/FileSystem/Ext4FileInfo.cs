namespace Ufex.API.FileSystem;

class Ext4FileInfo : FileInfo
{
	public bool Immutable;
	public bool AppendOnly;
	public bool NoDump;
	public bool NoAtime;

	public Ext4FileInfo(string filePath) : base(filePath, "EXT4")
	{
		// TODO: Placeholder implementation
		Immutable = false;
		AppendOnly = false;
		NoDump = false;
		NoAtime = false;
	}
}
