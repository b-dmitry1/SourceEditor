using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected int _scrollBarWidth = 16;
		public int ScrollBarWidth { get { return _scrollBarWidth; } set { _scrollBarWidth = value; Refresh(); } }

		private void drawScrollBar(Graphics g)
		{
			g.FillRectangle(Brushes.Gray, ClientSize.Width - _scrollBarWidth, 0, _scrollBarWidth, ClientSize.Height);

			if (_lines.Count == 0)
			{
				return;
			}

			var thumb = (int)((double)_line * (ClientSize.Height - 8.0) / _lines.Count);

			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.FillEllipse(Brushes.LightGray,
				ClientSize.Width - _scrollBarWidth + 2,
				thumb + 2,
				_scrollBarWidth - 4, _scrollBarWidth - 4);
		}
	}
}
