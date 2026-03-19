using System;
using System.Data;

namespace Ufex.FileType
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
	public class IDLibsDb : FileType.Database
	{
		public int Count
		{
			get { return GetIDLibs().Length; }
		}

		public ID_LIB[] GetIDLibs()
		{
			ID_LIB lib0 = new ID_LIB();
			lib0.libId = 2;
			lib0.assemblyPath = "Ufex.FileType.dll";
			lib0.fullTypeName = "Ufex.FileType.Classifiers.SignatureClassifier";
			ID_LIB lib3 = new ID_LIB();
			lib3.libId = 3;
			lib3.assemblyPath = "Ufex.FileType.dll";
			lib3.fullTypeName = "Ufex.FileType.Classifiers.ExtensionClassifier";

			return new ID_LIB[2]
			{
				lib0, lib3
			};
		}
	}
}
