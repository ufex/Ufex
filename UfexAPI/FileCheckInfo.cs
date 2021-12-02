using System;
using System.Collections;

namespace Ufex.API
{
	public class FileCheckInfo
	{
		public FileCheckInfo()
		{
			m_FileCheckData = new ArrayList(1);
		}

		~FileCheckInfo() 
		{
			m_FileCheckData = null;
		}
		
		public void Message(string message)
        {
			m_FileCheckData.Add(message);
		}

		public void Warning(string message)
        {
			m_FileCheckData.Add("Warning: " + message);
			m_NumWarnings++;
		}

		public void Error(string message)
        {
			m_FileCheckData.Add("Error: " + message);
			m_NumErrors++;
		}

		[Obsolete("Use Message instead")]
		public void NewMessage(String message)
        {
			this.Message(message);
        }

		[Obsolete("Use Warning instead")]
		public void NewWarning(String message)
        {
			Warning(message);
		}
		
		[Obsolete("Use Error instead")]
		public void NewError(String message)
        {
			Error(message);
		}

		public bool HasErrors() 
		{ 
			return (m_NumErrors > 0); 
		}
		
		public bool HasWarnings() 
		{ 
			return (m_NumWarnings > 0); 
		}

		public String[] GetInfo()
        {
			String[] info = new String[m_FileCheckData.Count + 1];
			for (int i = 0; i < m_FileCheckData.Count; i++)
			{
				info[i] = (string)m_FileCheckData[i];
			}

			// Add a summary line 
			info[m_FileCheckData.Count] = m_NumErrors.ToString() + " error(s), " + m_NumWarnings.ToString() + " warning(s)";
			return info;
		}

		private ArrayList m_FileCheckData;

		// NYI
		private bool m_ShowSummary;

		private int m_NumWarnings;
		private int m_NumErrors;
	}
}
