namespace Ufex.API.Tables;

public static class TableDataTemplates
{	
	public static DynamicTableData PropertyValueDescription(string id, object[][] rows)
	{
		DynamicTableData td = new DynamicTableData(3, id + ".PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		for(int i = 0; i < rows.Length; i++)
		{
			td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "");
		}
		return td;
	}

	public static DynamicTableData PropertyValueDescriptionOffset(string id, object[][] rows, long startOffset)
	{
		DynamicTableData td = new DynamicTableData(4, id + ".PropertyValueDescriptionOffset");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		long offset = startOffset;
		for (int i = 0; i < rows.Length; i++)
		{
			if(rows[i].Length > 3 && rows[i][3] == null)
			{
				// Offset is null, so don't calculate it or display it
				td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "", "");
			}
			else
			{
				td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "", new FileOffset(offset));
				offset += ByteUtil.GetObjectSize(rows[i][1]);				
			}
		}
		return td;
	}
	
}