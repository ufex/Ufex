using System;
using System.Data;
using System.Data.OleDb;

namespace UniversalFileExplorer
{
	/// <summary>
	/// Summary description for UFEDatabase.
	/// </summary>
	public abstract class UFEDatabase
	{
		private OleDbConnection m_dbConn;
		private UFEDebug m_debug;

		public OleDbConnection DbConn
		{
			get { return m_dbConn; }
			set { m_dbConn = value; }
		}

		public UFEDebug Debug
		{
			get { return m_debug; }
			set { m_debug = value; }
		}

		public UFEDatabase()
		{

		}

		public UFEDatabase(OleDbConnection dbConn)
		{
			this.m_dbConn = dbConn;
		}
	}
}
