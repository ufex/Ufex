using System;
using System.Data;
using System.Data.OleDb;

namespace UniversalFileExplorer
{

	public class FILETYPE_CLASS
	{
		public string classID;
		public string assemblyPath;
		public string fullTypeName;
	}


	/// <summary>
	/// Summary description for FileTypeClassesDb.
	/// </summary>
	public class FileTypeClassesDb : UFEDatabase
	{
		const string TABLE_NAME = "FileTypeClasses";

		// Columns
		const string COL_ID = "ClassID";
		
		// Queries
		const string QUERY_GET_ALL_CLASSES = "SELECT * FROM " + TABLE_NAME;
		const string QUERY_GET_NUM_CLASSES = "SELECT COUNT(" + COL_ID + ") FROM " + TABLE_NAME;
		const string QUERY_GET_CLASS_BY_ID = "SELECT * FROM " + TABLE_NAME + " WHERE " + COL_ID + "=\"{0}\"";

		public int Count
		{
			get { return GetNumFileTypeClasses(); }
		}

		public FileTypeClassesDb(OleDbConnection dbConn) : base(dbConn)
		{

		}

		public FILETYPE_CLASS GetFileTypeClass(string classId)
		{
			string getRecordCmdStr = String.Format(QUERY_GET_CLASS_BY_ID, classId);

			OleDbCommand cmd = new OleDbCommand(getRecordCmdStr, DbConn);
			OleDbDataReader dr = cmd.ExecuteReader();
		
			FILETYPE_CLASS fileTypeClass;
			if(dr.Read())
			{
				fileTypeClass = FileTypeClassFromDataReader(dr);
			}
			else
			{
				// FileType ID was not found
				fileTypeClass = null;
			}
			dr.Close();
			return fileTypeClass;
		}

		public bool Exists(string classId)
		{
			return false;
		}

		/// <summary>
		/// Queries the database to get the number of file type classes.
		/// </summary>
		/// <returns>The number of file type classes.</returns>
		public int GetNumFileTypeClasses()
		{
			OleDbCommand cmd = new OleDbCommand(QUERY_GET_NUM_CLASSES, DbConn);
			OleDbDataReader dr = cmd.ExecuteReader();
			
			int numRecords = 0;

			if(dr.Read())
				numRecords = dr.GetInt32(0);
			else
				numRecords = -1;
		
			dr.Close();

			return numRecords;
		}		
		
		private FILETYPE_CLASS FileTypeClassFromDataReader(OleDbDataReader dr)
		{
			FILETYPE_CLASS fileTypeClass = new FILETYPE_CLASS();
			fileTypeClass.classID = dr.GetString(0);
			fileTypeClass.assemblyPath = dr.GetString(1);
			fileTypeClass.fullTypeName = dr.GetString(2);
			return fileTypeClass;
		}
	}
}
