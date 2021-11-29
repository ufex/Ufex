using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace UFEControls
{
	public struct FileSection
	{
		public String name;
		public Int64 startPos;
		public Int64 endPos;
		public Color fillColor;
	};

	/// <summary>
	/// Summary description for FileMap.
	/// </summary>
	public class FileMap : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		ArrayList sections;
		Int64 fileSize;

		public FileMap()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			sections = new ArrayList();
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
			// 
			// FileMap
			// 
			this.Name = "FileMap";
			this.Resize += new System.EventHandler(this.FileMap_Resize);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.FileMap_Paint);

		}
		#endregion

		public void AddSection(String name, Int64 startPos, Int64 endPos)
		{
			AddSection(name, startPos, endPos, Color.White);
		}

		public void AddSection(String name, Int64 startPos, Int64 endPos, Color fillColor)
		{
			FileSection temp = new FileSection();
			temp.name = name;
			temp.startPos = startPos;
			temp.endPos = endPos;
			temp.fillColor = fillColor;
			sections.Add(temp);
		}

		public void ClearSections()
		{
			sections.Clear();
		}



		private void FileMap_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{			
			Graphics g;
			g = this.CreateGraphics();

			Color trans = Color.FromArgb(0, 0, 0, 255);


			int width = this.Width / 2;
			int height = this.Height;
			int extraWidth = this.Width / 2;

			Pen blackPen = new Pen(Color.Black, 1);
			Brush blackBrush = new SolidBrush(Color.Black);
			Font myFont = this.Font;

			g.DrawRectangle(blackPen, 0, 0, width - 1, height - 1);
		
			bool tooSmall = false;

			for(int i = 0; i < sections.Count; i++)
			{
				FileSection tmpSection = (FileSection)sections[i];
				
				// Create a brush to fill the rectangle with
				//Brush fillBrush = new HatchBrush(HatchStyle.DottedGrid, tmpSection.fillColor, trans);
				Brush fillBrush = new SolidBrush(tmpSection.fillColor);
				int start;
				int finish;
				
				if(tmpSection.startPos > 0)
					start = (int)(((double)(tmpSection.startPos - 1) / fileSize) * height);
				else
					start = (int)(((double)(0) / fileSize) * height);

				finish = (int)(((double)tmpSection.endPos / fileSize) * height);
				
				int sectHeight = finish - start;
				
				//g.DrawLine(blackPen, 0, start, width - 1, start);
				//g.DrawLine(blackPen, 0, finish, width - 1, finish);
				g.DrawRectangle(blackPen, 0, start, width - 1, sectHeight);
				g.FillRectangle(fillBrush, 0, start + 1, width - 1, sectHeight - 1);

				// If there is enough room to draw the section name
				if(!tooSmall && sectHeight > myFont.Height)
				{
					SizeF strSize = g.MeasureString(tmpSection.name, this.Font);
					
					PointF mypt;
					if(width > strSize.Width)
						mypt = new PointF((float)((width - strSize.Width) / 2), (float)((start + finish) / 2) - (strSize.Height / 2));
					else
						mypt = new PointF((float)(width  / 3), (float)((start + finish) / 2) - (strSize.Height / 2));

					g.DrawString(tmpSection.name, this.Font, blackBrush, mypt);
				}
				else
				{
					// Draw a diagonal line to the upper right
					g.DrawLine(blackPen, width - 1, start, width + (extraWidth / 2), start - 25);
					
					// See if next section fits
					int nextSection;
					if((i + 1) < sections.Count)
						nextSection = GetSectionHeight((FileSection)sections[i + 1], height);
					else
						nextSection = myFont.Height;

					if(nextSection > myFont.Height)
					{
						tooSmall = true;
					}
					else
					{
						tooSmall = false;
						g.DrawLine(blackPen, width - 1, finish, width + (extraWidth / 2), finish + 25);
					}
				}
			}
		}

		int GetSectionHeight(FileSection fs, int height)
		{
			int start;
			int finish;
				
			if(fs.startPos > 0)
				start = (int)(((double)(fs.startPos - 1) / fileSize) * height);
			else
				start = (int)(((double)(0) / fileSize) * height);

			finish = (int)(((double)fs.endPos / fileSize) * height);
			return finish - start;
		}

		private void FileMap_Resize(object sender, System.EventArgs e)
		{
			this.Refresh();
		}
	
		// Properties
		[
		Category("File"),
		Description("The size of the file")
		]
		public Int64 FileSize
		{
			set { this.fileSize = value; }
			get { return this.fileSize; }
		}
	
	}
}
