using System.Collections.Generic;

namespace Ufex.API.Tables
{
		// Minimal replacements for System.Windows.Forms ListView-related types used in TableData.
		public class ListView
		{
				public ListView()
				{
						Columns = new ListViewColumnCollection();
						Items = new ListViewItemCollection();
				}

				public ListViewColumnCollection Columns { get; }
				public ListViewItemCollection Items { get; }
		}

		public class ListViewColumnCollection
		{
				private readonly List<ColumnHeader> items = new();

				public void Clear() => items.Clear();

				public void Add(ColumnHeader header) => items.Add(header);
		}

		public class ListViewItemCollection
		{
				private readonly List<ListViewItem> items = new();

				public void Clear() => items.Clear();

				public void Add(ListViewItem item) => items.Add(item);
		}

		public class ColumnHeader
		{
				public string Text { get; set; } = string.Empty;
				public int Width { get; set; }
				public HorizontalAlignment TextAlign { get; set; }
		}

		public class ListViewItem
		{
				public ListViewItem(string[] subItems)
				{
						SubItems = subItems;
				}

				public string[] SubItems { get; }
		}
}
