using System;
using System.Data;

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
		public int Count
		{
			get { return GetIDLibs().Length; }
		}

		public ID_LIB[] GetIDLibs()
		{
			ID_LIB lib0 = new ID_LIB();
			lib0.libId = 2;
			lib0.assemblyPath = "FileTypeManager.dll";
			lib0.fullTypeName = "Ufex.Classifiers.SignatureClassifier";
			ID_LIB lib3 = new ID_LIB();
			lib3.libId = 3;
			lib3.assemblyPath = "FileTypeManager.dll";
			lib3.fullTypeName = "Ufex.Classifiers.ExtensionClassifier";

			//ID_LIB lib1 = new ID_LIB();
			//lib1.libId = 0;
			//lib1.assemblyPath = "UFEIDFileType.dll";
			//lib1.fullTypeName = "UniversalFileExplorer.IDFileType";
			//ID_LIB lib2 = new ID_LIB();
			//lib2.libId = 1;
			//lib2.assemblyPath = "UFEIDFileType.dll";
			//lib2.fullTypeName = "UniversalFileExplorer.ExtensionID";

			return new ID_LIB[2]
			{
				lib0, lib3
			};
		}
	}
}
