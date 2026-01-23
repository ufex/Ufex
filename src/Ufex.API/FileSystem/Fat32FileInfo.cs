using System;
using System.IO;

namespace Ufex.API.FileSystem;

/// <summary>
/// Metadata and attributes for FAT32 files.
/// </summary>
public class Fat32FileInfo : Ufex.API.FileInfo
{
	public bool ReadOnly;
	public bool Hidden;
	public bool System;
	public bool Directory;
	public bool Archive;

	public DateTime CreatedAt
	{
		get; protected set;
	}

	public DateTime ModifiedAt
	{
		get; protected set;
	}

	public DateOnly AccessedAt
	{
		get; protected set;
	}

	public Fat32FileInfo(string filePath) : base(filePath, "FAT32")
	{
		System.IO.FileInfo fInfo = new System.IO.FileInfo(filePath);

		// Flags
		ReadOnly = fInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
		Hidden = fInfo.Attributes.HasFlag(FileAttributes.Hidden);
		System = fInfo.Attributes.HasFlag(FileAttributes.System);
		Directory = fInfo.Attributes.HasFlag(FileAttributes.Directory);
		Archive = fInfo.Attributes.HasFlag(FileAttributes.Archive);

		// Timestamps
		CreatedAt = fInfo.CreationTime;
		ModifiedAt = fInfo.LastWriteTime;
		AccessedAt = DateOnly.FromDateTime(fInfo.LastAccessTime);
	}
}
