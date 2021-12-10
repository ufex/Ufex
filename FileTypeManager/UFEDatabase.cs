using System;
using System.IO;
using Ufex.API;

namespace UniversalFileExplorer
{
	/// <summary>
	/// Summary description for UFEDatabase.
	/// </summary>
	public abstract class UFEDatabase
	{
		protected FileInfo[] configFiles;
		private Logger m_debug;

		public Logger Debug
		{
			get { return m_debug; }
			set { m_debug = value; }
		}

		public UFEDatabase()
		{

		}

		public UFEDatabase(FileInfo[] configFiles)
		{
			this.configFiles = configFiles;
		}
	}
}
