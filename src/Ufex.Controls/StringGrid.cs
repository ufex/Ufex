using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Ufex.Controls
{
	/// <summary>
	/// Summary description for StringGrid.
	/// </summary>
	public class StringGrid : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// String Grid Dimensions
		public int numRows;
		public int numCols;
		public int cellWidth;
		public int cellHeight;

		private Color cellLineColor;
		private Color borderColor;
		private Color highlightColor;
		
		public String [,] data;
		private String [,] oldData;



		// Optimizations

		// If buffered is set to true: all drawing operations 
		//    are performed using a buffered image
		private bool buffered;

		// Shifts data so only one row is drawn each time SetData is called
		//private bool optimizeShift;
	

		// The Buffered Image
		private Bitmap buffer;
		private Bitmap highlightLayer;

		public String debug1;

		public StringGrid()
		{
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			oldData = new String[numRows, numCols];

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
				if(buffer != null)
				{
					buffer.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // StringGrid
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Name = "StringGrid";
            this.Size = new System.Drawing.Size(240, 208);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.StringGrid_Paint);
            this.ResumeLayout(false);

		}
		#endregion


		// ColorTransform
		// This function modifies baseColor based on the values of rMod, gMod, and bMod
		// and returns the result
		private Color ColorTransform(Color baseColor, int rMod, int gMod, int bMod)
		{
			int newR = baseColor.R + rMod;
			int newG = baseColor.G + gMod;
			int newB = baseColor.B + bMod;

			if(newR < 0)
				newR = 0;
			else if(newR > 255)
				newR = 255;

			if(newG < 0)
				newG = 0;
			else if(newG > 255)
				newG = 255;

			if(newB < 0)
				newB = 0;
			else if(newB > 255)
				newB = 255;

			return Color.FromArgb(baseColor.A, newR, newG, newB);
		}

		public void SetDimensions(int rows, int cols)
		{
			numRows = rows;
			numCols = cols;
			data = new String[rows, cols];
			oldData = new String[rows, cols];
			for(int r = 0; r < rows; r++)
			{
				for(int c = 0; c < cols; c++)
				{
					data[r,c] = "";
					oldData[r,c] = "";
				}
			}
		}
		
        /// <summary>
        /// Sets the dimensions of a single cell.
        /// </summary>
        /// <param name="width">The width of the cell.</param>
        /// <param name="height">The height of the cell.</param>
		public void SetCellSize(int width, int height)
		{
			cellWidth = width;
			cellHeight = height;
		}

		public void ResizeToFit()
		{
			this.Width = (numCols * (cellWidth + 1)) + 4;
			this.Height = (numRows * (cellHeight + 1)) + 4;
			buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
			highlightLayer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
		}

		public void SetData(String [,] newData)
		{
			oldData = data;
			data = newData;
			
			Graphics g;

            if (buffered && buffer != null)
                g = Graphics.FromImage(buffer);
            else
                g = this.CreateGraphics();

			DrawAllCellData(g, true);
			
			g.Dispose();

			// Draw the buffer to the screen
			if(buffered && buffer != null)
			{
				DrawBuffer(buffer);
			}
		}

		private void StringGrid_Paint(object sender, PaintEventArgs e)
		{
			Graphics g;
			
			if(buffered)
				g = Graphics.FromImage(buffer);
			else
				g = this.CreateGraphics();
			
			DrawFrame(g, 0, 0, this.Width - 1, this.Height - 1);
			DrawCellOutlines(g);
			DrawAllCellData(g, true);

			g.Dispose();

			// Draw the buffer to the screen
			if(buffered)
			{
				DrawBuffer(buffer);
			}
			DrawBuffer(highlightLayer);
		}


		private void DrawFrame(Graphics g, int x1, int y1, int x2, int y2)
		{
			//Pen myPen1 = new Pen(Color.FromArgb(172, 168, 153));Color.FromArgb(113, 111, 100)
			Pen myPen1 = new Pen(borderColor);
			Pen myPen2 = new Pen(ColorTransform(borderColor, -59, -57, -53));
			Pen myPen3 = new Pen(ColorTransform(borderColor, 83, 87, 102));
			Pen myPen4 = new Pen(ColorTransform(borderColor, 69, 71, 73));

			g.DrawLine(myPen1, new Point(x1, y1), new Point(x2 - 1, y1));
			g.DrawLine(myPen1, new Point(x1, y1), new Point(x1, y2 - 1));
			g.DrawLine(myPen2, new Point(x1 + 1, y1 + 1), new Point(x2 - 2, y1 + 1));
			g.DrawLine(myPen2, new Point(x1 + 1, y1 + 1), new Point(x1 + 1, y2 - 2));
			
			g.DrawLine(myPen3, new Point(x2, y1), new Point(x2, y2));
			g.DrawLine(myPen3, new Point(x1, y2), new Point(x2, y2));

			g.DrawLine(myPen4, new Point(x2 - 1, y2 - 1), new Point(x2 - 1, y2 - 1));
			g.DrawLine(myPen4, new Point(x1 + 1, y2 - 1), new Point(x2 - 1, y2 - 1));
			
			// Dispose of all the pens
			myPen1.Dispose();
			myPen2.Dispose();
			myPen3.Dispose();
			myPen4.Dispose();
		}

		private void DrawCellOutlines(Graphics g)
		{
			Pen cellPen  = new Pen(cellLineColor);
			
			//int offX = 2 - 1;
			//int offY = 2 - 1;
			int cellWidthP1 = cellWidth + 1;
			int cellHeightP1 = cellHeight + 1;

			// Draw Horizontal Lines
			int y;
			for(int i = 1; i <= numRows; i++)
			{
				y = 1 + (i * cellHeightP1);
				g.DrawLine(cellPen, 
					new Point(2, y), 
					new Point((numCols * cellWidthP1) + 1, y));
			}

			int x;
			// Draw Vertical Lines
			for(int i = 1; i <= numCols; i++)
			{
				x = 1 + (i * cellWidthP1);
				g.DrawLine(cellPen, 
					new Point(x, 2), 
					new Point(x, numRows * cellHeightP1));
			}

			cellPen.Dispose();
		}


		private void DrawAllCellData(Graphics g, bool paintAll)
		{
			Brush bkBrush = new SolidBrush(this.BackColor);
			Brush textBrush = new SolidBrush(this.ForeColor);
			
			int offX = 2;
			int offY = 2;
			int cwp1 = cellWidth + 1;
			int chp1 = cellHeight + 1;

			if(g == null)
				return;
			
			StringFormat drawFormat = new StringFormat();
			drawFormat.Alignment = StringAlignment.Center;
			
			for(int r = 0; r < numRows; r++)
			{
				float y = offY + (r * chp1);
				for(int c = 0; c < numCols; c++)
				{
					if(data[r, c] == null || oldData[r,c] == null) 
						return;

					if(paintAll || !data[r, c].Equals(oldData[r, c]))
					{
						float x = offX + (c * cwp1);
						
						g.FillRectangle(bkBrush, x, y, cellWidth, cellHeight);
						//g.DrawString(data[r, c], this.Font, textBrush, new PointF(x, y));

						g.DrawString(data[r, c], this.Font, textBrush, new RectangleF(x, y, cellWidth, cellHeight), drawFormat);
					}
				}
			}

			textBrush.Dispose();
			bkBrush.Dispose();			
		}
		

		private void DrawBuffer(Bitmap bmpBuffer)
		{
			Graphics myGraphics = this.CreateGraphics();
			myGraphics.DrawImage(bmpBuffer, 0, 0);
			myGraphics.Dispose();
		}

		public void HighlightCells(uint r1, uint c1, uint length)
		{
			uint r = r1;
			uint c = c1;

			Graphics g;
			Color highlight32 = Color.FromArgb(128, this.HighlightColor);
			Brush bkBrush = new SolidBrush(highlight32);
			Brush textBrush = new SolidBrush(this.ForeColor);
			StringFormat drawFormat = new StringFormat();
			drawFormat.Alignment = StringAlignment.Center;

			g = Graphics.FromImage(highlightLayer);
			
			for(int i = 0; i < length; i++)
			{
				//HighlightCell(r, c);
				int offX = 2;
				int offY = 2;
				int cwp1 = cellWidth + 1;
				int chp1 = cellHeight + 1;
				float y = offY + (r * chp1);
				float x = offX + (c * cwp1);
				g.FillRectangle(bkBrush, x, y, cellWidth, cellHeight);
				g.DrawString(data[r, c], this.Font, textBrush, new RectangleF(x, y, cellWidth, cellHeight), drawFormat);
				
				c++;
				if(c >= numCols)
				{
					r++;
					c = 0;
				}
			}
			g.Dispose();

			DrawBuffer(highlightLayer);

			textBrush.Dispose();
			bkBrush.Dispose();
		}

		public void HighlightCell(int r, int c)
		{
			Brush bkBrush = new SolidBrush(this.HighlightColor);
			Brush textBrush = new SolidBrush(this.ForeColor);

			Graphics g;
			
			if(buffered)
				g = Graphics.FromImage(buffer);
			else
				g = this.CreateGraphics();

			int offX = 2;
			int offY = 4;
			int cwp1 = cellWidth + 1;
			int chp1 = cellHeight + 1;
			float y = offY + (r * chp1);
			float x = offX + (c * cwp1);
			StringFormat drawFormat = new StringFormat();
			drawFormat.Alignment = StringAlignment.Center;

			g.FillRectangle(bkBrush, x, y, cellWidth, cellHeight - 2);
			g.DrawString(data[r, c], this.Font, textBrush, new RectangleF(x, y, cellWidth, cellHeight), drawFormat);

			g.Dispose();

			if(buffered)
				DrawBuffer(buffer);

			textBrush.Dispose();
			bkBrush.Dispose();

		}

		public void Clear()
		{
			// Set all the data to blank strings
			for(int r = 0; r < numRows; r++)
			{
				for(int c = 0; c < numCols; c++)
				{
					data[r, c] = "";
				}
			}
			
			// Clear the highlight layer bitmap
			ClearHighlightLayer();			

			// Redraw the highlight layer
			DrawBuffer(highlightLayer);

			SetData(data);
		}

		public void ClearHighlightLayer()
		{
			highlightLayer.Dispose();
			highlightLayer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
		}

		[
		Category("Dimensions"),
		Description("Number of rows in the grid")
		]
		public int NumRows
		{
			set { this.numRows = value; }
			get { return this.numRows; }
		}

		[
		Category("Dimensions"),
		Description("Number of columns in the grid")
		]
		public int NumCols
		{
			set { this.numCols = value; }
			get { return this.numCols; }
		}

		[
		Category("Layout"),
		Description("The width of one cell")
		]
		public int CellWidth
		{
			set { this.cellWidth = value; }
			get { return this.cellWidth; }
		}

		[
		Category("Layout"),
		Description("Number of columns in the grid")
		]
		public int CellHeight
		{
			set { this.cellHeight = value; }
			get { return this.cellHeight; }
		}

		[
		Category("Performance"),
		Description("Use a buffered image for drawing operations")
		]
		public bool BufferedDisplay
		{
			set { this.buffered = value; }
			get { return this.buffered; }
		}

		[
		Category("Appearance"),
		Description("The color used for the Cell Lines")
		]
		public Color CellLineColor
		{
			set { this.cellLineColor = value; }
			get { return this.cellLineColor; }
		}


		[
		Category("Appearance"),
		Description("The color used for the Border")
		]
		public Color BorderColor
		{
			set { this.borderColor = value; }
			get { return this.borderColor; }
		}
		[
		Category("Appearance"),
		Description("The color used to highlight cells")
		]
		public Color HighlightColor
		{
			set { this.highlightColor = value; }
			get { return this.highlightColor; }
		}
	}
}
