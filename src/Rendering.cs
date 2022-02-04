using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;

			updateCharSize();

			g.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

			drawSelection(g);

			for (var line = _vscroll; line < _lines.Count; line++)
			{
				var sym = 0;
				if (line < _attributes.Count)
				{
					for (; sym < _attributes[line].Length; sym++)
					{
						var attr = _attributes[line][sym];

						var left = (sym - _hscroll) * _charWidth;
						var top = (line - _vscroll) * _charHeight;

						if (((attr >> 4) & 7) > 0)
						{
							g.FillRectangle(_brushes[(attr >> 4) & 7], left, top, _charWidth, _charHeight);
						}

						if (sym < _lines[line].Length)
						{
							g.DrawString(_lines[line][sym].ToString(), Font, _brushes[(attr & 7) == 0 ? 7 : attr & 7],
								left, top);
						}

						if (left >= ClientSize.Width)
						{
							break;
						}
					}
				}

				for (; sym < _lines[line].Length; sym++)
				{
					var left = (sym - _hscroll) * _charWidth;
					var top = (line - _vscroll) * _charHeight;

					if (left >= ClientSize.Width)
					{
						break;
					}

					g.DrawString(_lines[line][sym].ToString(), Font, _brushes[7 & 7],
						left, top);
				}

				if ((line - _vscroll) * _charHeight >= ClientSize.Height)
				{
					break;
				}
			}

			if (_blink < 2 && Focused)
			{
				g.FillRectangle(Brushes.White, (_symbol - _hscroll) * _charWidth + 2, (_line - _vscroll + 1) * _charHeight - 2, _charWidth, 2);
			}

			drawScrollBar(g);

			base.OnPaint(e);
		}

		private void drawSelection(Graphics g)
		{
			Point selStart;
			Point selEnd;

			normalizeSelection(out selStart, out selEnd);

			for (var line = selStart.Y; line < selEnd.Y; line++)
			{
				g.FillRectangle(_brushes[8],
					((line == selStart.Y ? selStart.X - _hscroll : 0)) * _charWidth + 2, (line - _vscroll) * _charHeight, ClientSize.Width, _charHeight);
			}

			if (selStart.Y < selEnd.Y)
			{
				g.FillRectangle(_brushes[8],
					2, (selEnd.Y - _vscroll) * _charHeight, selEnd.X * _charWidth, _charHeight);
			}
			else
			{
				g.FillRectangle(_brushes[8],
					(selStart.X - _hscroll) * _charWidth + 2, (selEnd.Y - _vscroll) * _charHeight, (selEnd.X - selStart.X) * _charWidth, _charHeight);
			}
		}
	}
}
