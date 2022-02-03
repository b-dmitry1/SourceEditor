using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (ModifierKeys.HasFlag(Keys.Control))
			{
				if (e.Delta < 0)
				{
					if (Font.Size > 6)
					{
						Font = new Font(Font.Name, (int)Font.Size - 1);
					}
				}
				else if (e.Delta > 0)
				{
					if (Font.Size < 24)
					{
						Font = new Font(Font.Name, (int)Font.Size + 1);
					}
				}
			}
			else
			{
				moveCursor(_line + (e.Delta < 0 ? 3 : -3), _symbol);
			}
			Refresh();
		}

		protected Point mouseToChar(int mx, int my)
		{
			var x = mx / _charWidth + _hscroll;
			var y = my / _charHeight + _vscroll;
			x = Math.Max(x, 0);
			y = Math.Max(y, 0);
			return new Point(x, y);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			_selecting = false;
			if (e.X >= ClientSize.Width - _scrollBarWidth)
			{
				doMouseScroll(e.Y);
				return;
			}

			_selecting = true;
			var coord = mouseToChar(e.X, e.Y);
			moveCursor(coord.Y, coord.X);
			_selectionStart = coord;
			_selectionEnd = coord;
			Refresh();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_selecting)
			{
				if (e.Button == System.Windows.Forms.MouseButtons.Left)
				{
					doMouseScroll(e.Y);
					return;
				}
				return;
			}

			if (e.Button != System.Windows.Forms.MouseButtons.Left)
			{
				return;
			}
			var coord = mouseToChar(e.X, e.Y);
			moveCursor(coord.Y, coord.X, true);
			_selectionEnd = coord;
			Refresh();
		}

		private void doMouseScroll(int y)
		{
			moveCursor(y * _lines.Count / (ClientSize.Height - 4), _symbol);

			_blink = 0;

			Refresh();
		}
	}
}
