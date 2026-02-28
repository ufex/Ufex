using System;
using System.Collections;
using System.IO;

namespace Ufex.API;

public class FileInfo
{
	
	public string FileSystem { get; protected set; }

	public string FilePath { get; protected set; }

	public string FileName
	{
		get { return System.IO.Path.GetFileName(FilePath); } 
	}

	public string FileExtension
	{
		get { return System.IO.Path.GetExtension(FilePath); } 
	}

	public string FileSize
	{
		get
		{
			System.IO.FileInfo fi = new System.IO.FileInfo(FilePath);
			return fi.Length.ToString();
		}
	}

	public static FileInfo FromFile(string filePath)
	{
		switch(GetFileSystem(filePath))
		{
			case "FAT32":
				return new Ufex.API.FileSystem.Fat32FileInfo(filePath);
			case "EXT4":
				return new Ufex.API.FileSystem.Ext4FileInfo(filePath);
			case "NTFS":
				return new Ufex.API.FileSystem.NtfsFileInfo(filePath);
			default:
				return null;
		}
	}

	protected FileInfo(string filePath, string fileSystem)
	{
		FilePath = filePath;
		FileSystem = fileSystem;
	}

	public static string GetFileSystem(string filePath)
	{
		// Get the root of the path (e.g., "C:\" or "D:\")
		string root = Path.GetPathRoot(filePath);

		// Check if root is null or empty (e.g., relative paths)
		if (string.IsNullOrEmpty(root))
		{
				throw new ArgumentException("Path must be absolute to determine drive root.");
		}

		// Create DriveInfo instance for that root
		DriveInfo drive = new DriveInfo(root);

		// Return the format (NTFS, FAT32, exFAT, etc.)
		return drive.DriveFormat;
	}
}
