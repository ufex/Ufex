using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UniversalFileExplorer;

namespace Ufex.Gui
{
	internal static class Program
	{
		[DllImport("Shcore.dll")]
		static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);

		// According to https://msdn.microsoft.com/en-us/library/windows/desktop/dn280512(v=vs.85).aspx
		private enum DpiAwareness
		{
			None = 0,
			SystemAware = 1,
			PerMonitorAware = 2
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//SetProcessDpiAwareness((int)DpiAwareness.PerMonitorAware);

			string strCmdLine = String.Join(" ", args);
			try
			{
				UniversalFileExplorerApp appInstance = new UniversalFileExplorerApp();
				appInstance.ShowLoadDialog();
				appInstance.mainForm = new Form1(strCmdLine, appInstance.m_Settings);
				appInstance.mainForm.pApp = appInstance;
				Application.Run(appInstance.mainForm);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
