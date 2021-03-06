using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected List<string> _lines = new List<string>();
		protected List<int[]> _attributes = new List<int[]>();
		protected int _line = 0;
		protected int _symbol = 0;
		protected int _hscroll = 0;
		protected int _vscroll = 0;
		protected int _charWidth = 0;
		protected int _charHeight = 0;
		protected int _blink = 0;
		protected bool _selecting = false;
		protected Point _selectionStart = new Point();
		protected Point _selectionEnd = new Point();
		public List<string> Lines
		{
			get
			{
				return _lines;
			}
			set
			{
				_line = _vscroll = _symbol = _hscroll = 0;
				_lines = value;
				LinesChanged(0, _lines.Count);
				Refresh();
			}
		}

		protected string[] _keywords = new string[0];
		public string[] Keywords
		{
			get
			{
				return _keywords;
			}
			set
			{
				_keywords = value;
				LinesChanged(0, _lines.Count);
				Refresh();
			}
		}

		protected string[] _identifiers = new string[0];
		public string[] Identifiers
		{
			get
			{
				return _identifiers;
			}
			set
			{
				_identifiers = value;
				LinesChanged(0, _lines.Count);
				Refresh();
			}
		}

		public string SingleLineComment { get; set; }

		protected List<string> _multilineCommentStartSymbols = new List<string>();
		public List<string> MultilineCommentStartSymbols { get { return _multilineCommentStartSymbols; } }

		protected List<string> _multilineCommentEndSymbols = new List<string>();
		public List<string> MultilineCommentEndSymbols { get { return _multilineCommentEndSymbols; } }

		public enum CursorShapeKind
		{
			Vertical, Underline, Block
		}

		protected CursorShapeKind _cursorShape = CursorShapeKind.Vertical;
		public CursorShapeKind CursorShape { get { return _cursorShape; } set { _cursorShape = value; } }

		public override string Text
		{
			get
			{
				var sb = new StringBuilder();
				foreach (var line in _lines)
				{
					sb.AppendLine(line);
				}
				return sb.ToString();
			}
			set
			{
				var delim = "\n";
				if (value.IndexOf("\r\n") >= 0)
				{
					delim = "\r\n";
				}
				else if (value.IndexOf("\r") >= 0)
				{
					delim = "\r";
				}
				_lines = Text.Split(new string[] {delim}, StringSplitOptions.None).ToList();
			}
		}

		public SourceEditor()
		{
			InitializeComponent();

			Font = new Font("Consolas", 10.0f);

			BackColor = Color.FromArgb(0x44, 0x44, 0x44);

			DoubleBuffered = true;

			SingleLineComment = "//";

			_multilineCommentStartSymbols.Add("/*");
			_multilineCommentEndSymbols.Add("*/");

			var timer = new Timer();
			timer.Tick += timer_Tick;
			timer.Interval = 500;
			timer.Enabled = true;

			Resize += SourceEditor_Resize;

			LinesChanged(0, _lines.Count);
		}

		void SourceEditor_Resize(object sender, EventArgs e)
		{
			Refresh();
		}

		public void LoadFromFile(string fileName)
		{
			_lines.Clear();
			_line = _symbol = _vscroll = _hscroll = 0;
			_selectionStart.X = _selectionStart.Y = _selectionEnd.X = _selectionEnd.Y = 0;

			if (File.Exists(fileName))
			{
				var lines = File.ReadAllLines(fileName);
				_lines.AddRange(lines);
			}

			LinesChanged(0, _lines.Count);

			_undo.Clear();
		}

		public void SaveToFile(string fileName)
		{
			File.WriteAllLines(fileName, _lines);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if (Focused)
			{
				_blink++;
				if (_blink > 2)
				{
					_blink = 1;
				}
				Refresh();
			}
		}

		public void LinesChanged(int first, int count)
		{
			first = Math.Max(0, first);
			count = Math.Min(_lines.Count - first, count);

			highlightSyntax(first, count);
		}
		
		private void updateCharSize()
		{
			using (var bmp = new Bitmap(10, 10))
			{
				using (var g = Graphics.FromImage(bmp))
				{
					var sz = g.MeasureString("1234567890", Font);
					_charWidth = (int)(sz.Width) / 10;
					_charHeight = (int)(sz.Height);
				}
			}
		}

		public void SetAttribute(int line, int symbol, int count, int charColor, int backgroundColor)
		{
			if (line < 0 || line >= _lines.Count || symbol < 0)
			{
				return;
			}

			while (_attributes.Count <= line)
			{
				_attributes.Add(new int[Math.Max(symbol + count, _lines[line].Length)]);
			}

			if (_attributes[line].Length < symbol + count)
			{
				var newArray = new int[Math.Max(symbol + count, _lines[line].Length)];
				Array.Copy(_attributes[line], newArray, _attributes[line].Length);
				_attributes[line] = newArray;
			}
			for (var i = 0; i < count; i++)
				_attributes[line][symbol + i] = (charColor & 7) + ((backgroundColor & 7) << 4);
		}

		public void MoveCursor(int line, int symbol, bool keepSelection = false)
		{
			line = Math.Max(line, 0);
			symbol = Math.Max(symbol, 0);
			line = Math.Min(_lines.Count - 1, line);

			line = Math.Max(line, 0);

			if (line != _line)
			{
				preventAddingCharsToLastUndo();
			}

			_line = line;
			_symbol = symbol;

			_vscroll = Math.Min(_vscroll, _line);
			_hscroll = Math.Min(_hscroll, _symbol);

			_vscroll = Math.Max(_vscroll, _line - ClientSize.Height / _charHeight + 1);
			_hscroll = Math.Max(_hscroll, _symbol - ClientSize.Width / _charWidth + 1);

			if (!keepSelection)
			{
				_selectionStart.X = _symbol;
				_selectionStart.Y = _line;
				_selectionEnd = _selectionStart;
			}

			_blink = 0;
		}

		protected void updateSelectionStart()
		{
			if (_selectionStart == _selectionEnd)
			{
				_selectionStart.Y = _line;
				_selectionStart.X = _symbol;
				_selectionEnd = _selectionStart;
			}
		}

		private void normalizeSelection(out Point selStart, out Point selEnd)
		{
			if (_selectionStart.Y > _selectionEnd.Y || (_selectionStart.Y == _selectionEnd.Y && _selectionStart.X > _selectionEnd.X))
			{
				selStart = _selectionEnd;
				selEnd = _selectionStart;
			}
			else
			{
				selStart = _selectionStart;
				selEnd = _selectionEnd;
			}
		}
	}
}
