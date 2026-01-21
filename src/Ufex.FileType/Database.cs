using System;
using System.IO;
using Ufex.API;

namespace Ufex.FileType
{
	/// <summary>
	/// A collection of configuration files that represent a database
	/// </summary>
	public abstract class Database
	{
		protected FileInfo[] configFiles;
		private Logger m_debug;

		public Logger Debug
		{
			get { return m_debug; }
			set { m_debug = value; }
		}

		public Database()
		{

		}

		public Database(FileInfo[] configFiles)
		{
			this.configFiles = configFiles;
		}
	}
}
