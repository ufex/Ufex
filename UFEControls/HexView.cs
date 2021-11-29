using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;
using UniversalFileExplorer;

namespace UFEControls
{
	/// <summary>
	/// Summary description for HexViewControl.
	/// </summary>
	public class HexViewControl : System.Windows.Forms.UserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// Buffer size in bytes
		private int bufferSize;

		// The file position that the buffer starts at
		private Int64 bufferStartPosition;

		// The Actual Buffer
		private Byte [] buffer;

		// The File Path
		private String filePath;

		public FileStream fileStream;

		// Set to true if a file is loaded
		private bool fileLoaded;

		// File Position of the first cell being displayed
		private Int64 filePosition;

		// File row number
		private int fileRowNum;
 
		private Int64 fileSize;

		// Number of lines in the file
		private int numLines;

		bool closeFileStreamWhenDone;

		bool ignoreScrollbarChange = false;

		// Frame Sizes
		private const int ADD_BAR_WIDTH = 65;
		//private int ADD_BAR_HEIGHT;


		private const int HEX_CELL_WIDTH = 18;
		private const int HEX_CELL_HEIGHT = 18;
		
		// Location of Seperators
		private const int SEP1X1 = 62;
		private const int SEP1Y1 = 0;
		private const int SEP1X2 = 62;
		private const int SEP1Y2 = 500;

		// Address bar width
		private const int ADD_BAR_CELL_WIDTH = 61;



		private int numCols;
		private int numRows;

		private bool hexCaps;


		private Int64 highlightStart;
		private Int64 highlightEnd;


		public String debugText;
		private bool debugMode;
		private UFEDebug debug;


		//private bool showStatusBar;

		// Used to format the strings in the hex conversions
		//   Should be equal to: "X" or "x"
		private String formatter;

		private UFEControls.StringGrid strgdAddressBar;
		private UFEControls.StringGrid strgdHexData;
        private UFEControls.StringGrid strgdTextData;
		private System.Windows.Forms.VScrollBar vScrollBar;
		private System.Windows.Forms.StatusBar statusDebug;
		private System.Windows.Forms.StatusBarPanel debugPanel1;
		private System.Windows.Forms.StatusBarPanel debugPanel2;
		private System.Windows.Forms.StatusBarPanel debugPanel3;
		private System.Windows.Forms.StatusBarPanel debugPanel4;
		private System.Windows.Forms.StatusBarPanel debugPanel5;
		
		public HexViewControl()
		{
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
			debug = new UFEDebug("UFEControls_HexViewControl.log");

			debugText = "-HexViewDebug-";
			DebugOut("-HexViewControl()-");

			if(debugMode)
				statusDebug.Visible = true;
			else
				statusDebug.Visible = false;
		
			InheritSettings(strgdAddressBar);
			InheritSettings(strgdHexData);
            InheritSettings(strgdTextData);

			vScrollBar.Enabled = false;

			fileLoaded = false;
		}

        void InheritSettings(StringGrid sg)
        {
            sg.Font = this.Font;
            sg.BackColor = this.BackColor;
            sg.ForeColor = this.ForeColor;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
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
			this.vScrollBar = new System.Windows.Forms.VScrollBar();
			this.strgdAddressBar = new UFEControls.StringGrid();
			this.strgdHexData = new UFEControls.StringGrid();
			this.strgdTextData = new UFEControls.StringGrid();
			this.statusDebug = new System.Windows.Forms.StatusBar();
			this.debugPanel1 = new System.Windows.Forms.StatusBarPanel();
			this.debugPanel2 = new System.Windows.Forms.StatusBarPanel();
			this.debugPanel3 = new System.Windows.Forms.StatusBarPanel();
			this.debugPanel4 = new System.Windows.Forms.StatusBarPanel();
			this.debugPanel5 = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel5)).BeginInit();
			this.SuspendLayout();
			// 
			// vScrollBar
			// 
			this.vScrollBar.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.vScrollBar.LargeChange = 1;
			this.vScrollBar.Location = new System.Drawing.Point(624, 0);
			this.vScrollBar.Maximum = 256;
			this.vScrollBar.Name = "vScrollBar";
			this.vScrollBar.Size = new System.Drawing.Size(16, 352);
			this.vScrollBar.TabIndex = 0;
			this.vScrollBar.ValueChanged += new System.EventHandler(this.vScrollBar_ValueChanged);
			this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar_Scroll);
			// 
			// strgdAddressBar
			// 
			this.strgdAddressBar.BackColor = System.Drawing.SystemColors.Control;
			this.strgdAddressBar.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.strgdAddressBar.BufferedDisplay = true;
			this.strgdAddressBar.CellHeight = 18;
			this.strgdAddressBar.CellLineColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(157)), ((System.Byte)(185)));
			this.strgdAddressBar.CellWidth = 64;
			this.strgdAddressBar.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.strgdAddressBar.HighlightColor = System.Drawing.Color.Yellow;
			this.strgdAddressBar.Location = new System.Drawing.Point(8, 8);
			this.strgdAddressBar.Name = "strgdAddressBar";
			this.strgdAddressBar.NumCols = 0;
			this.strgdAddressBar.NumRows = 0;
			this.strgdAddressBar.Size = new System.Drawing.Size(65, 272);
			this.strgdAddressBar.TabIndex = 1;
			// 
			// strgdHexData
			// 
			this.strgdHexData.BackColor = System.Drawing.SystemColors.Control;
			this.strgdHexData.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.strgdHexData.BufferedDisplay = true;
			this.strgdHexData.CellHeight = 18;
			this.strgdHexData.CellLineColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(157)), ((System.Byte)(185)));
			this.strgdHexData.CellWidth = 18;
			this.strgdHexData.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.strgdHexData.HighlightColor = System.Drawing.Color.Yellow;
			this.strgdHexData.Location = new System.Drawing.Point(80, 8);
			this.strgdHexData.Name = "strgdHexData";
			this.strgdHexData.NumCols = 0;
			this.strgdHexData.NumRows = 0;
			this.strgdHexData.Size = new System.Drawing.Size(312, 272);
			this.strgdHexData.TabIndex = 2;
			// 
			// strgdTextData
			// 
			this.strgdTextData.BackColor = System.Drawing.SystemColors.Control;
			this.strgdTextData.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.strgdTextData.BufferedDisplay = true;
			this.strgdTextData.CellHeight = 18;
			this.strgdTextData.CellLineColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(157)), ((System.Byte)(185)));
			this.strgdTextData.CellWidth = 11;
			this.strgdTextData.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.strgdTextData.HighlightColor = System.Drawing.Color.Yellow;
			this.strgdTextData.Location = new System.Drawing.Point(400, 8);
			this.strgdTextData.Name = "strgdTextData";
			this.strgdTextData.NumCols = 0;
			this.strgdTextData.NumRows = 0;
			this.strgdTextData.Size = new System.Drawing.Size(208, 272);
			this.strgdTextData.TabIndex = 3;
			// 
			// statusDebug
			// 
			this.statusDebug.Location = new System.Drawing.Point(0, 328);
			this.statusDebug.Name = "statusDebug";
			this.statusDebug.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						   this.debugPanel1,
																						   this.debugPanel2,
																						   this.debugPanel3,
																						   this.debugPanel4,
																						   this.debugPanel5});
			this.statusDebug.ShowPanels = true;
			this.statusDebug.Size = new System.Drawing.Size(640, 24);
			this.statusDebug.TabIndex = 4;
			// 
			// debugPanel1
			// 
			this.debugPanel1.MinWidth = 50;
			this.debugPanel1.Width = 150;
			// 
			// debugPanel2
			// 
			this.debugPanel2.MinWidth = 50;
			this.debugPanel2.Width = 120;
			// 
			// debugPanel3
			// 
			this.debugPanel3.MinWidth = 50;
			this.debugPanel3.Width = 90;
			// 
			// debugPanel4
			// 
			this.debugPanel4.Width = 125;
			// 
			// debugPanel5
			// 
			this.debugPanel5.Width = 125;
			// 
			// HexViewControl
			// 
			this.Controls.Add(this.statusDebug);
			this.Controls.Add(this.strgdTextData);
			this.Controls.Add(this.strgdHexData);
			this.Controls.Add(this.strgdAddressBar);
			this.Controls.Add(this.vScrollBar);
			this.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Name = "HexViewControl";
			this.Size = new System.Drawing.Size(640, 352);
			this.Load += new System.EventHandler(this.HexViewControl_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.HexViewControl_Paint);
			((System.ComponentModel.ISupportInitialize)(this.debugPanel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.debugPanel5)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
		
		/// <summary>
		/// Redraw the control.
		/// </summary>
		private void HexViewControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			//debug.NewInfo("HexViewControl_Paint(...)");
			
			// Refresh the string grids
			strgdAddressBar.Refresh();
			strgdHexData.Refresh();
			strgdTextData.Refresh();
		}

		private void HexViewControl_Load(object sender, System.EventArgs e)
		{	
			//debug.NewInfo("HexViewControl_Load(...)");
			ResizeGrid();
		}

		/// <summary>
		/// Opens the file and loads it into the hex editor.
		/// </summary>
		public void LoadFile(String file)
		{
			//debug.NewInfo(String.Concat("LoadFile(", file, ")"));
			closeFileStreamWhenDone = true;

			filePath = file;
			try
			{
				fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			}
			catch(IOException ioe)
			{
				debug.NewException(ioe, "HexView", String.Concat("LoadFile(", filePath, ")"), "Error creating new FileStream (IOException)");
				return;
			}
			catch(Exception e)
			{
				debug.NewException(e, "HexView", String.Concat("LoadFile(", filePath, ")"), "Error creating new FileStream (Exception)");
			}
			InitializeFile();
		}

		/// <summary>
		/// Loads a FileStream into the Hex Viewer
		/// </summary>
		public void LoadFileStream(FileStream fs)
		{
			//debug.NewInfo("LoadFileStream(...)");

			closeFileStreamWhenDone = false;
			fileStream = fs;
			InitializeFile();
		}

		/// <summary>
		/// Unloads the File and frees any buffer memory.
		/// </summary>
		public void UnloadFile()
		{			
			//debug.NewInfo("UnloadFile()");

			// Close the stream if needed
			if(fileLoaded && closeFileStreamWhenDone)
				fileStream.Close();

			// Disable the scroll bar
			vScrollBar.Enabled = false;

			// Put blank data in the StringGrid controls
			strgdAddressBar.Clear();
			strgdHexData.Clear();
			strgdTextData.Clear();
			
			// Set the file loaded flag
			fileLoaded = false;

			// Set the debug text to blank
			debugText = "";

			// Set the buffer to null
			buffer = null;

			fileSize = 0;
		}

		/// <summary>
		/// Redraws the data being displayed.
		/// </summary>
		public void RedrawData()
		{
			//debug.NewInfo("RedrawData()");
			//if(!fileLoaded)
			//	return;

			if(fileSize <= (numRows * numCols))
				SetDataOneScreen();
			else
				SetData();
		}

		/// <summary>
		/// Sets the string tables to display the file data at 
		/// the current file position.
		/// </summary>
		private void SetData()
		{
			//debug.NewInfo("SetData()");
			StatusDebug(3, String.Concat("CharSet = ", this.Font.GdiCharSet.ToString()));
			
			if(hexCaps)
				formatter = "X";
			else
				formatter = "x";

			if(!fileLoaded)
				return;

			int bp;
			
			// Create the address bar data
			String [,] addBarData = new String[numRows, 1];
			Int64 pos = filePosition;
			for(int i = 0; i < numRows; i++)
			{
				pos = filePosition + (i * numCols);
				addBarData[i, 0] = pos.ToString(formatter);
			}
			strgdAddressBar.SetData(addBarData);

			// Create the hex data
			String [,] hexData = new String[numRows, numCols];
			bp = (int)(filePosition - bufferStartPosition);
			for(int r = 0; r < numRows; r++)
			{
				for(int c = 0; c < numCols && bp < bufferSize && bp < fileSize; c++)
				{
					hexData[r, c] = ByteToHexString(buffer[bp]);
					bp++;
				}
			}

			// Set all the null strings to ""
			for(int i = numCols - 1; hexData[numRows - 1, i] == null; i--)
				hexData[numRows - 1, i] = "";

			/* Old Method 
			if((filePosition + (numRows * numCols)) > fileSize)
			{
				int numBytesExtra = (int)((filePosition + (numRows * numCols)) - fileSize);
				for(int i = 1; i <= numBytesExtra; i++)
					hexData[numRows - 1, numCols - i] = "";
			}
			*/
			
			strgdHexData.SetData(hexData);
			
			// Create the character data
			String [,] charData = new String[numRows, numCols];
			bp = (int)(filePosition - bufferStartPosition);
			
			// Add the data for the rows
			for(int r = 0; r < numRows; r++)
			{
				for(int c = 0; c < numCols && bp < bufferSize && bp < fileSize; c++)
				{
					char tmp = ((char)buffer[bp]);
					charData[r, c] = tmp.ToString();
					//charData[r, c] = ((char)buffer[bp]).ToString(); 
					bp++;
				}
			}

			// Set all the null strings to ""
			for(int i = numCols - 1; charData[numRows - 1, i] == null; i--)
				charData[numRows - 1, i] = "";
			
			/* Old Method 
			if((filePosition + (numRows * numCols)) > fileSize)
			{
				int numBytesExtra = (int)((filePosition + (numRows * numCols)) - fileSize);
				for(int i = 1; i <= numBytesExtra; i++)
					charData[numRows - 1, numCols - i] = "";
			}
			*/
			
			strgdTextData.SetData(charData);
		}

		/// <summary>
		/// Sets the string tables for a file that fits
		/// on one screen. (No scrollbars)
		/// </summary>
		private void SetDataOneScreen()
		{
			//debug.NewInfo("SetDataOneScreen()");
			int r = 0, c = 0;
	
			if(hexCaps)
				formatter = "X";
			else
				formatter = "x";

			String [,] addBarData = new String[numRows, 1];
			String [,] hexData = new String[numRows, numCols];
			String [,] charData = new String[numRows, numCols];
			
			for(int fpos = 0; fpos < fileSize; fpos++)
			{
				if(c == 0)
					addBarData[r, 0] = fpos.ToString(formatter);

				hexData[r, c] = ByteToHexString(buffer[fpos]);
				charData[r, c] = ((char)buffer[fpos]).ToString(); 

				c++;

				if(c >= numCols)
				{
					r++;
					c = 0;
				}
			}

			strgdAddressBar.SetData(addBarData);
			strgdHexData.SetData(hexData);
			strgdTextData.SetData(charData);
		}

		/***************************************************
		*	Function: InitializeFile
		*
		*
		***************************************************/
		private void InitializeFile()
		{
			debug.NewInfo("InitializeFile()");
			fileLoaded = true;
			fileSize = fileStream.Length;

			numLines = (int)(fileSize / numCols);
			
			if(fileSize % numCols != 0)
				numLines++;

			vScrollBar.Minimum = 0;

			// Check if the file is bigger than the display size
			if(fileSize <= (numRows * numCols))
			{
				vScrollBar.Enabled = false;

				filePosition = 0;

				// Fill up the buffer
				bufferStartPosition = 0;
				buffer = new Byte[bufferSize];
				fileStream.Seek(0, SeekOrigin.Begin);
				fileStream.Read(buffer, (int)bufferStartPosition, bufferSize);

				SetDataOneScreen();
				return;
			}
			else		// The entire file can be displayed on one screen
			{
				filePosition = 0;

				// Fill up the buffer
				bufferStartPosition = 0;
				buffer = new Byte[bufferSize];
				
				fileStream.Seek(0, SeekOrigin.Begin);
				fileStream.Read(buffer, (int)bufferStartPosition, bufferSize);
				
				ignoreScrollbarChange = true;
				vScrollBar.Enabled = true;
				vScrollBar.Maximum = numLines - 1;// - numRows;
				vScrollBar.LargeChange = numRows;
				vScrollBar.Value = 0;
				ignoreScrollbarChange = false;
				SetData();
			}
		}


		private void vScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
		}

		/// <summary>
		/// This function reloads the buffer starting at
		/// the specified position.
		/// </summary>
		private void ReloadBuffer(Int64 position)
		{
			debug.NewInfo(String.Concat("ReloadBuffer(", position.ToString(), ")"));
			// The buffer is reloaded so that the position 
			//   is in the middle of the buffer
			bufferStartPosition = position - (bufferSize / 2);
			if(bufferStartPosition < 0)
			{
				bufferStartPosition = 0;
			}

			if((bufferStartPosition + bufferSize >= fileSize))
			{
				bufferStartPosition = fileSize - bufferSize;
			}

			// Debug
			StatusDebug(3, String.Concat("buffStPos = ", bufferStartPosition.ToString()));

			fileStream.Seek(bufferStartPosition, SeekOrigin.Begin);
			try
			{
				fileStream.Read(buffer, 0, bufferSize);
			}
			catch(ObjectDisposedException ode)
			{
				debug.NewException(ode, "HexView", String.Concat("ReloadBuffer(", position.ToString(), ")"), "Error loading buffer (ObjectDisposedException)");
				return;
			}
		}

		private void DebugOut(String text)
		{	
			if(!debugMode)
				return;

			debugText = String.Concat(debugText, "\r\n", text);
			debug.NewInfo(text);
			StatusDebug(1, text);
		}

		private void StatusDebug(int panNum, String text)
		{
			if(!debugMode)
				return;

			switch(panNum)
			{
				case 1:
					debugPanel1.Text = text;
					break;
				case 2:
					debugPanel2.Text = text;
					break;
				case 3:
					debugPanel3.Text = text;
					break;
				case 4:
					debugPanel4.Text = text;
					break;
				case 5:
					debugPanel5.Text = text;
					break;
			}
		}


		/// <summary>
		/// Converts a Byte to a hex string.
		/// </summary>
		private String ByteToHexString(Byte x)
		{
			if(x < 16)
				return String.Concat("0", x.ToString(formatter));
			else
				return x.ToString(formatter);
		}

		private void ResizeGrid()
		{
			strgdAddressBar.SetDimensions(numRows, 1);
			//strgdAddressBar.SetCellSize(60, 18);
			strgdAddressBar.ResizeToFit();

			strgdHexData.SetDimensions(numRows, numCols);
			//strgdHexData.SetCellSize(HEX_CELL_WIDTH, HEX_CELL_HEIGHT);
			strgdHexData.ResizeToFit();

			strgdTextData.SetDimensions(numRows, numCols);
			//strgdTextData.SetCellSize(11, 18);
			strgdTextData.ResizeToFit();
			
			strgdHexData.Left = strgdAddressBar.Right + 1;
			strgdTextData.Left = strgdHexData.Right + 1;
			vScrollBar.Left = strgdTextData.Right + 1;

			vScrollBar.Top = strgdTextData.Top;
			vScrollBar.Height = strgdAddressBar.Height;

		}

		private void vScrollBar_ValueChanged(object sender, System.EventArgs e)
		{
			if(ignoreScrollbarChange)
				return;

			fileRowNum = vScrollBar.Value;
			filePosition = fileRowNum * numCols;
			
			// Debug
			StatusDebug(2, String.Concat("filePos = ", filePosition.ToString()));
			StatusDebug(4, String.Concat("scrl.val = ", vScrollBar.Value.ToString()));
			StatusDebug(5, String.Concat("scrl.max = ", vScrollBar.Maximum.ToString()));
			// End Debug

			
			// Reload buffer if needed
			if(filePosition < bufferStartPosition || 
				(filePosition + (numRows * numCols)) > (bufferStartPosition + bufferSize))
			{
				ReloadBuffer(filePosition);
			}
			
			SetData();
			if(highlightStart - highlightEnd != 0)
			{
				Highlight(highlightStart, highlightEnd);
			}
		}



		public void GotoPosition(Int64 position)
		{
			if(position >= fileSize)
			{
				debug.NewError("position is greater than or equal to fileSize", "HexView", "GotoPosition(" + position.ToString() + ")", "Invalid value for position");
				return;
			}
			if((fileSize - position) < (numCols * numRows))
			{
				debug.NewInfo("(fileSize - position) < filePosition + (numCols * numRows)");
				vScrollBar.Value = (int)((fileSize - (numCols * numRows)) / (long)numCols);
			}
			else
			{
				vScrollBar.Value = (int)(position / (long)numCols);
			}
			return;
		}

		public void Highlight(Int64 hStartPos, Int64 hEndPos)
		{
			// Calculate the file positions
			Int64 fStartPos = filePosition;
			Int64 fEndPos = filePosition + (numCols * numRows);
			
			// Make sure the startPos is less than the endPos
			if(hStartPos > hEndPos)
				return;

			// If the highlight end position is before the file start position
			if(hEndPos < fStartPos)
				return;

			// If the highlight start position is after the file end position
			if(hStartPos > fEndPos)
				return;
			
			highlightStart = hStartPos;
			highlightEnd = hEndPos;
	
			// If the highlight end position is after the file end position
			if(hEndPos > fEndPos)
				hEndPos -= hEndPos - fEndPos;
			
			if(hStartPos < fStartPos)
				hStartPos += fStartPos - hStartPos;

			uint offset = (uint)(hStartPos - filePosition);
			uint range = (uint)(hEndPos - hStartPos);

			uint c = (uint)(offset % numCols);
			uint r = (uint)(offset / numCols);

			try
			{
				strgdHexData.ClearHighlightLayer();
				strgdHexData.HighlightCells(r, c, range);
				strgdTextData.ClearHighlightLayer();
				strgdTextData.HighlightCells(r, c, range);
			}
			catch(IndexOutOfRangeException iore)
			{
				debug.NewException(iore, "HexView", 
								"Highlight(" + hStartPos.ToString() + ", " + hEndPos.ToString() + ")",
								"c = " + c.ToString() + ", r = " + r.ToString());
			}
			catch(Exception e)
			{
				debug.NewException(e, "HexView", 
					"Highlight(" + hStartPos.ToString() + ", " + hEndPos.ToString() + ")",
					"c = " + c.ToString() + ", r = " + r.ToString());
			}
		}

		/**
		 * 
		 *		 Properties
		 * 
		 */

		[
		Category("Buffer"),
		Description("The size of the buffer in bytes (minimum 512)")
		]
		public int BufferSize
		{
			set 
			{ 
				if(value >= 512) 
					this.bufferSize = value;
				else
					this.bufferSize = 512;
			}
			get { return this.bufferSize; }
		}

		[
		Category("Dimensions"),
		Description("The number of rows that are displayed")
		]
		public int NumRows
		{
			set { vScrollBar.LargeChange = value; this.numRows = value; ResizeGrid(); }
			get { return this.numRows; }
		}

		[
		Category("Dimensions"),
		Description("The number of columns that are displayed")
		]
		public int NumCols
		{
			set { this.numCols = value; ResizeGrid(); }
			get { return this.numCols; }
		}

		[
		Category("Number"),
		Description("Show hex digits as capital letters")
		]
		public bool HexCaps
		{
			set { this.hexCaps = value; RedrawData(); }
			get { return this.hexCaps; }
		}
/*
		[
		Category("Appearance"),
		Description("Show the status bar")
		]
		public bool StatusBar
		{
			set { this.StatusBar = value; }
			get { return this.debugMode; }
		}
*/
		[
		Category("Debug"),
		Description("Display debugging information")
		]
		public bool DebugMode
		{
			set { this.debugMode = value; statusDebug.Visible = this.debugMode; }
			get { return this.debugMode; }
		}

	}
}
