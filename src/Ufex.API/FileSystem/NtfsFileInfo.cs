using System;
using System.IO;

namespace Ufex.API.FileSystem;
/// <summary>
/// Metadata and attributes for NTFS files.
/// </summary>
public class NtfsFileInfo : Ufex.API.FileInfo
{
	#region $STANDARD_INFORMATION
	public bool ReadOnly;
	public bool Hidden;
	public bool System;
	public bool Archive;
	public bool Normal;

	public bool Temporary;
	public bool SparseFile;
	public bool ReparsePoint;
	public bool Compressed;
	public bool Offline;
	public bool NotContentIndexed;
	public bool Encrypted;

	#endregion
	
	public DateTime CreatedAt
	{
		get; protected set;
	}

	public DateTime ModifiedAt
	{
		get; protected set;
	}

	public DateTime AccessedAt
	{
		get; protected set;
	}

	public NtfsFileInfo(string filePath) : base(filePath, "NTFS")
	{
		System.IO.FileInfo fInfo = new System.IO.FileInfo(filePath);

		// DOS Attributes
		ReadOnly = fInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
		Hidden = fInfo.Attributes.HasFlag(FileAttributes.Hidden);
		System = fInfo.Attributes.HasFlag(FileAttributes.System);
		Archive = fInfo.Attributes.HasFlag(FileAttributes.Archive);
		Normal = fInfo.Attributes.HasFlag(FileAttributes.Normal);

		// Additional Attributes
		Temporary = fInfo.Attributes.HasFlag(FileAttributes.Temporary);
		SparseFile = fInfo.Attributes.HasFlag(FileAttributes.SparseFile);
		ReparsePoint = fInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
		Compressed = fInfo.Attributes.HasFlag(FileAttributes.Compressed);
		Offline = fInfo.Attributes.HasFlag(FileAttributes.Offline);
		NotContentIndexed = fInfo.Attributes.HasFlag(FileAttributes.NotContentIndexed);
		Encrypted = fInfo.Attributes.HasFlag(FileAttributes.Encrypted);

		// Timestamps
		CreatedAt = fInfo.CreationTime;
		ModifiedAt = fInfo.LastWriteTime;
		AccessedAt = fInfo.LastAccessTime;
	}

}
