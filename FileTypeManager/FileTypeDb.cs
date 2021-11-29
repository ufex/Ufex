using System;
using System.Data;
using System.Data.OleDb;

namespace UniversalFileExplorer
{

	public class FILETYPE
	{
		public string id;
		public string description;
		public string generalType;
		public string mimeType;
		public string extensions;
		public bool bind;
		public string fileTypeClassId;
		public string wikipedia;
		public string link;
		public string magic;
		public bool update;

		public string[] ToRowArray()
		{
			string[] columns = new string[10];
			columns[0] = id;
			columns[1] = description;
			columns[2] = generalType;
			columns[3] = mimeType;
			columns[4] = extensions;
			columns[5] = bind.ToString();
			columns[6] = fileTypeClassId;
			columns[7] = wikipedia;
			columns[8] = link;
			columns[9] = magic;
			return columns;
		}

	}


	/// <summary>
	/// Summary description for FileTypeDb.
	/// </summary>
	public class FileTypeDb : UFEDatabase
	{

		const string TABLE_NAME = "FileTypes";

		// The column names
		const string COL_ID = "FileTypeID";
		const string COL_DESCRIPTION = "Description";
		const string COL_GENTYPE = "GeneralType";
		const string COL_MIMETYPE = "MIMEType";
		const string COL_EXTENSIONS = "Extensions";
		const string COL_BIND = "Bind";
		const string COL_FTMODID = "FileTypeModID";
		const string COL_WIKIPEDIA = "Wikipedia";
		const string COL_LINK = "Link";

		// Queries
		const string QUERY_GET_ALL_TYPES = "SELECT * FROM " + TABLE_NAME;
		const string QUERY_GET_NUM_TYPES = "SELECT COUNT(" + COL_ID + ") FROM " + TABLE_NAME;
		const string QUERY_GET_TYPE_BY_ID = "SELECT * FROM " + TABLE_NAME + " WHERE " + COL_ID + "=\"{0}\"";
		const string QUERY_GET_TYPE_BY_EXT = "SELECT * FROM " + TABLE_NAME + " WHERE Extensions=\"{0}\" OR Extensions=\"%;{0}%\" OR Extensions=\"%{0};%\"";
		const string QUERY_ADD_RECORD = "INSERT INTO {0} (" + 
			COL_ID + ", " + COL_DESCRIPTION + ", " + COL_GENTYPE + "," + COL_MIMETYPE +
			", Extensions, Bind, FileTypeModID, Wikipedia, Link, Magic) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})";
		
		const string QUERY_DELETE_RECORD = "DELETE FROM " + TABLE_NAME + " WHERE " + COL_ID + " = {0}";


		bool gotOrdinals;

		// Column Ordinals
		int colID, colDecsription, colGenType, colMimeType, colExtensions,
			colBind, colFTModID, colWikipedia, colLink;


		public int Count
		{
			get { return GetNumFileTypes(); }
		}


		public FileTypeDb(OleDbConnection dbConn) : base(dbConn)
		{
			gotOrdinals = false;
			Debug = new UFEDebug("FileTypeDb");
		}

		public FILETYPE GetFileType(string fileTypeId)
		{
			if(fileTypeId == null)
				throw new NullReferenceException("FileTypeID cannot be null");

			if(fileTypeId.IndexOf("*") != -1)
				throw new Exception("Invalid FileTypeID");

			string getRecordCmdStr = String.Format(QUERY_GET_TYPE_BY_ID, fileTypeId);

			OleDbCommand cmd = new OleDbCommand(getRecordCmdStr, DbConn);
			OleDbDataReader dr = cmd.ExecuteReader();
		
			FILETYPE fileType;
			if(dr.Read())
			{
				fileType = FileTypeFromDataReader(dr);
			}
			else
			{
				// FileType ID was not found
				fileType = null;
			}
			dr.Close();
			return fileType;
		}

		public FILETYPE[] GetFileTypes()
		{
			// Get the number of file types in the database
			int numTypes = this.Count;
		
			// Create an array to hold the FILETYPE objects
			FILETYPE[] fileTypes = new FILETYPE[numTypes];

			// Create a query
			OleDbCommand cmd = new OleDbCommand(QUERY_GET_ALL_TYPES, DbConn);
		
			// Execute the query
			OleDbDataReader dr = cmd.ExecuteReader();
		
			// Read each record returned from the query
			for(int i = 0; i < numTypes && dr.Read(); i++)
			{
				fileTypes[i] = FileTypeFromDataReader(dr);
			}

			// Close the data reader
			dr.Close();

			// Return the fileTypes array
			return fileTypes;
		}	
		
		/// <summary>
		/// Gets all file types with the specified extension.
		/// </summary>
		/// <param name="extension">The extension to search for</param>
		/// <returns>An array of fileTypes with the specified extension.</returns>
		public FILETYPE[] GetFileTypesByExtension(string extension)
		{
			if(extension == null)
				throw new NullReferenceException("extension cannot be null");

			string getRecordCmdStr = String.Format(QUERY_GET_TYPE_BY_EXT, extension);
			OleDbCommand cmd = new OleDbCommand(getRecordCmdStr, DbConn);
			OleDbDataReader dr = null;
			FILETYPE[] fileTypes;
			try
			{
				dr = cmd.ExecuteReader();
			
				// Create an array to hold all the results
				fileTypes = new FILETYPE[dr.RecordsAffected];

				// Add all the file types to the array
				for(int i = 0; dr.Read(); i++)
					fileTypes[i] = FileTypeFromDataReader(dr);
			}
			catch(Exception e)
			{
				Debug.NewException(e, "Failed to get file types by extension");
				fileTypes = new FILETYPE[0];
			}
			finally
			{
				// Close the data reader
				dr.Close();
			}
			return fileTypes;
		}

		public bool AddFileType(FILETYPE fileType)
		{
			string[] rowValues = fileType.ToRowArray();
			string getRecordCmdStr = String.Format(QUERY_GET_TYPE_BY_EXT, rowValues);
			OleDbDataReader dr = null;
			try
			{
				OleDbCommand cmd = new OleDbCommand(getRecordCmdStr, DbConn);
				dr = cmd.ExecuteReader();
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				dr.Close();
			}
			return true;
		}

		public bool SetFileType(string fileTypeId, FILETYPE fileType)
		{

			return false;
		}

		/// <summary>
		/// Removes the file type from the database.
		/// </summary>
		/// <param name="fileTypeId">The file types id.</param>
		/// <returns>Returns true if the file type was removed, otherwise it returns false.</returns>
		public bool RemoveType(string fileTypeId)
		{
			if(fileTypeId == null)
				throw new NullReferenceException("FileTypeID cannot be null");

			if(fileTypeId.IndexOf("*") != -1)
				throw new Exception("Invalid FileTypeID");

			bool success = false;
			string delRecordCmdStr = String.Format(QUERY_DELETE_RECORD, fileTypeId);
			OleDbDataReader dr = null;
			try
			{
				OleDbCommand cmd = new OleDbCommand(delRecordCmdStr, DbConn);
				dr = cmd.ExecuteReader();
			
				if(dr.RecordsAffected == 1)
					success = true;
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				dr.Close();
			}

			return success;
		}

		/// <summary>
		/// Queries the database to get the number of file types.
		/// </summary>
		/// <returns>The number of file types.</returns>
		public int GetNumFileTypes()
		{
			OleDbCommand cmd = new OleDbCommand(QUERY_GET_NUM_TYPES, DbConn);
			OleDbDataReader dr = null;
			
			int numRecords = 0;
			
			try
			{
				dr = cmd.ExecuteReader();

				if(dr.Read())
					numRecords = dr.GetInt32(0);
				else
					numRecords = -1;
			}
			catch(Exception e)
			{
				Debug.NewException(e, "Failed to get number of types");
				numRecords = -1;
			}
			finally
			{
				dr.Close();
			}

			return numRecords;
		}

		private FILETYPE FileTypeFromDataReader(OleDbDataReader dr)
		{
			if(!gotOrdinals)
				SetOrdinals(dr);

			FILETYPE fileType = new FILETYPE();
		
			fileType.id = dr.GetString(colID);
		
			fileType.description = dr.GetString(colDecsription);
		
			if(!Convert.IsDBNull(dr.GetValue(colGenType)))
				fileType.generalType = dr.GetString(colGenType);
			else
				fileType.generalType = null;

			fileType.mimeType = GetString(dr, colMimeType);
			fileType.extensions = GetString(dr, colExtensions);
			fileType.bind = dr.GetBoolean(colBind);

			if(fileType.bind)
				fileType.fileTypeClassId = dr.GetString(colFTModID);
			else
				fileType.fileTypeClassId = null;

			fileType.wikipedia = GetString(dr, colWikipedia);
			fileType.link = GetString(dr, colLink);

			return fileType;
		}

		private void SetOrdinals(OleDbDataReader dr)
		{
			colID = dr.GetOrdinal(COL_ID);
			colDecsription = dr.GetOrdinal(COL_DESCRIPTION);
			colGenType = dr.GetOrdinal(COL_GENTYPE);
			colMimeType = dr.GetOrdinal(COL_MIMETYPE);
			colExtensions = dr.GetOrdinal(COL_EXTENSIONS);
			colBind = dr.GetOrdinal(COL_BIND);
			colFTModID = dr.GetOrdinal(COL_FTMODID);
			colWikipedia = dr.GetOrdinal(COL_WIKIPEDIA);
			colLink = dr.GetOrdinal(COL_LINK);
			gotOrdinals = true;
		}

		private string GetString(OleDbDataReader dr, int column)
		{
			if(!Convert.IsDBNull(dr.GetValue(column)))
				return dr.GetString(column);
			else
				return null;	
		}

	}
}
