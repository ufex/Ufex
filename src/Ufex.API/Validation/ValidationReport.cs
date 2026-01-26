using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ufex.API.Validation;

public class ValidationReport
{		
	public enum EntryType
	{
		Message,
		Warning,
		Error
	}
	
	public struct Entry
	{
		public EntryType Type;
		public string Message;
	}

	public Entry[] Entries { get { return entries.ToArray(); } }

	public bool ShowSummary { get; set; }

	public int NumWarnings { get { return entries.Count(e => e.Type == EntryType.Warning); } }
	public int NumErrors { get { return entries.Count(e => e.Type == EntryType.Error); } }

	public Boolean HasWarnings { get { return NumWarnings > 0; } }
	public Boolean HasErrors { get { return NumErrors > 0; } }

	private List<Entry> entries;

	public ValidationReport()
	{
		entries = new List<Entry>();
	}

	~ValidationReport() 
	{
		entries = null;
	}
	
	public void Message(string message)
	{
		entries.Add(new Entry { Type = EntryType.Message, Message = message });
	}

	public void Warning(string message)
	{
		entries.Add(new Entry { Type = EntryType.Warning, Message = message });
	}

	public void Error(string message)
	{
		entries.Add(new Entry { Type = EntryType.Error, Message = message });
	}

	public String[] GetInfo()
	{
		String[] info = new String[entries.Count + 1];
		for (int i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];
			var prefix = entry.Type switch
			{
				EntryType.Warning => "Warning: ",
				EntryType.Error => "Error: ",
				_ => string.Empty
			};
			info[i] = prefix + entry.Message;
		}

		// Add a summary line 
		info[entries.Count] = NumErrors.ToString() + " error(s), " + NumWarnings.ToString() + " warning(s)";
		return info;
	}

}

