using System;
using System.Data;
using System.Data.OleDb;

namespace UniversalFileExplorer
{
	public class ID_LIB
	{
		public int libId;
		public string assemblyPath;
		public string fullTypeName;
	}


	/// <summary>
	/// Summary description for IDLibsDb.
	/// </summary>
	public class IDLibsDb : UFEDatabase
	{
		const string TABLE_NAME = "IDLibs";
		
		// Queries
		const string QUERY_GET_ALL_LIBS = "SELECT * FROM " + TABLE_NAME;
		const string QUERY_GET_NUM_LIBS = "SELECT COUNT(*) FROM " + TABLE_NAME;
		
		public int Count
		{
			get { return GetNumIDLibs(); }
		}

		public IDLibsDb(OleDbConnection dbConn) : base(dbConn)
		{

		}

		public ID_LIB[] GetIDLibs()
		{
			// Get the number of file types in the database
			int numLibs = this.Count;
		
			// Create a query
			OleDbCommand cmd = new OleDbCommand(QUERY_GET_ALL_LIBS, DbConn);
			
			// Create an array to hold the ID_LIB objects
			ID_LIB[] idLibs = new ID_LIB[numLibs];		
			
			// Execute the query
			OleDbDataReader dr = cmd.ExecuteReader();
		
			// Read each record returned from the query
			for(int i = 0; i < numLibs && dr.Read(); i++)
				idLibs[i] = IDLibFromDataReader(dr);

			// Close the data reader
			dr.Close();

			// Return the fileTypes array
			return idLibs;
		}

		/// <summary>
		/// Queries the database to get the number of id libs.
		/// </summary>
		/// <returns>The number of id libs.</returns>
		public int GetNumIDLibs()
		{
			OleDbCommand cmd = new OleDbCommand(QUERY_GET_NUM_LIBS, DbConn);
			OleDbDataReader dr = cmd.ExecuteReader();
			
			int numRecords = 0;

			if(dr.Read())
				numRecords = dr.GetInt32(0);
			else
				numRecords = -1;
		
			dr.Close();

			return numRecords;
		}	

		private ID_LIB IDLibFromDataReader(OleDbDataReader dr)
		{
			ID_LIB idLib = new ID_LIB();
			idLib.libId = dr.GetInt32(0);
			idLib.assemblyPath = dr.GetString(1);
			idLib.fullTypeName = dr.GetString(2);
			return idLib;
		}


	}
}
