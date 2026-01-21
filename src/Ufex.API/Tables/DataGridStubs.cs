using System.Collections.Generic;
using System.Drawing;

namespace Ufex.API.Tables
{
    // Minimal replacements for System.Windows.Forms DataGrid* types used in TableData.
    public class DataGridTableStyle
    {
        public string MappingName { get; set; } = string.Empty;
        public bool RowHeadersVisible { get; set; }
        public Color AlternatingBackColor { get; set; }
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public Color GridLineColor { get; set; }
        public Color HeaderBackColor { get; set; }
        public Color HeaderForeColor { get; set; }
        public Color LinkColor { get; set; }
        public Color SelectionBackColor { get; set; }
        public Color SelectionForeColor { get; set; }

        public GridColumnStylesCollection GridColumnStyles { get; } = new();
    }

    public abstract class DataGridColumnStyle
    {
        public string MappingName { get; set; } = string.Empty;
        public string HeaderText { get; set; } = string.Empty;
        public int Width { get; set; }
        public HorizontalAlignment Alignment { get; set; }
    }

    public class DataGridTextBoxColumn : DataGridColumnStyle
    {
    }

    public class GridColumnStylesCollection
    {
        private readonly List<DataGridColumnStyle> items = new();

        public void Add(DataGridColumnStyle style) => items.Add(style);

        public bool Contains(string mappingName) => items.Exists(s => s.MappingName == mappingName);

        public IReadOnlyList<DataGridColumnStyle> Items => items;
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }
}
