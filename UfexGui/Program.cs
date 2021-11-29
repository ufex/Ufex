using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniversalFileExplorer;

namespace Ufex.Gui
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());
			string strCmdLine = ""; // TODO
			UniversalFileExplorerApp appInstance = new UniversalFileExplorerApp();
			appInstance.ShowLoadDialog();
			appInstance.PreRun();
			appInstance.mainForm = new Form1(strCmdLine, appInstance.m_Settings);
			appInstance.mainForm.pApp = appInstance;
			Application.Run(appInstance.mainForm);
		}
	}
}
