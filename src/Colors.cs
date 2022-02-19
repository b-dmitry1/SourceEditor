using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected Color _scrollBarColor = Color.Gray;
		public Color ScrollBarColor { get { return _scrollBarColor; } set { _scrollBarColor = value; Refresh(); } }

		protected Color _scrollBarThumbColor = Color.LightGray;
		public Color ScrollBarThumbColor { get { return _scrollBarThumbColor; } set { _scrollBarThumbColor = value; Refresh(); } }

		protected Color _selectionColor = Color.SlateGray;
		public Color SelectionColor { get { return _selectionColor; } set { _selectionColor = value; Refresh(); } }

		protected Brush[] _brushes = new SolidBrush[]
		{
			new SolidBrush(Color.Black),
			new SolidBrush(Color.LightSkyBlue),
			new SolidBrush(Color.LightGreen),
			new SolidBrush(Color.LimeGreen),
			new SolidBrush(Color.PeachPuff),
			new SolidBrush(Color.LemonChiffon),
			new SolidBrush(Color.Moccasin),
			new SolidBrush(Color.WhiteSmoke),
			new SolidBrush(Color.SlateGray),
			new SolidBrush(Color.LightPink),
		};

		public enum EditorColor
		{
			Keywords = 1, Comments, MultilineComments, Strings = 4, Punctuation, Identifiers, Default
		}

		public void SetColor(EditorColor index, Color color)
		{
			if ((int)index < 0 || (int)index >= _brushes.Length)
			{
				return;
			}
			_brushes[(int)index] = new SolidBrush(color);

			Refresh();
		}
	}
}
