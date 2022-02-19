using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected bool _dragging = false;

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
				MoveCursor(_line + (e.Delta < 0 ? 3 : -3), _symbol);
			}
			Refresh();
		}

		protected Point mouseToChar(int mx, int my)
		{
			if (_charWidth == 0 || _charHeight == 0)
			{
				return new Point(0, 0);
			}
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

			_dragging = false;

			var coord = mouseToChar(e.X, e.Y);
			if (isInSelection(coord.Y, coord.X))
			{
				_dragging = true;
				return;
			}

			_selecting = true;
			MoveCursor(coord.Y, coord.X);
			_selectionStart = coord;
			_selectionEnd = coord;
			Refresh();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			var coord = mouseToChar(e.X, e.Y);
	
			if (_dragging)
			{
				if (e.Button != System.Windows.Forms.MouseButtons.Left)
				{
					_dragging = false;
				}
				else
				{
					MoveCursor(coord.Y, coord.X, true);

					Refresh();

					return;
				}
			}

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

			MoveCursor(coord.Y, coord.X, true);
			_selectionEnd = coord;
			Refresh();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			var coord = mouseToChar(e.X, e.Y);
			MoveCursor(coord.Y, coord.X, true);

			Refresh();

			if (!_dragging)
			{
				return;
			}

			_dragging = false;

			if (e.Button != System.Windows.Forms.MouseButtons.Left)
			{
				return;
			}

			if (_selectionStart == _selectionEnd)
			{
				return;
			}

			if (isInSelection(coord.Y, coord.X))
			{
				return;
			}

			saveMassUndo(0, _lines.Count, _lines.Count);

			var dx = 0;
			if (_selectionStart.Y == _selectionEnd.Y && _selectionStart.Y == coord.Y)
			{
				dx = _selectionEnd.X - _selectionStart.X;
			}
			var dy = _lines.Count;

			var start = _selectionStart;
			var end = _selectionEnd;

			Cut(true);

			dy = dy - _lines.Count;

			MoveCursor(coord.Y - dy, coord.X - dx, false);

			_selectionStart = _selectionEnd;

			Paste(true);

			_selectionStart.X = coord.X - dx;
			_selectionStart.Y = coord.Y - dy;
			_selectionEnd = _selectionStart;
			_selectionEnd.X += end.X - _selectionEnd.X;
			_selectionEnd.Y += end.Y - start.Y;

			// Remove automatically created
			_undo.Pop();
			_undo.Pop();
		}

		// Double click selects single word or a whole line
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			_selecting = false;
			if (e.X >= ClientSize.Width - _scrollBarWidth)
			{
				doMouseScroll(e.Y);
				return;
			}

			var coord = mouseToChar(e.X, e.Y);
			MoveCursor(coord.Y, coord.X, true);

			if (coord.Y < 0 || coord.Y >= _lines.Count)
			{
				// Somewhere outside the workspace so just deselect
				_selectionStart = coord;
				_selectionEnd = coord;

				Refresh();

				return;
			}

			if (_selectionStart.Y != _selectionEnd.Y)
			{
				// Multiple lines selected so deselect and try to select a word
				_selectionStart = coord;
				_selectionEnd = coord;
			}
			else
			{
				if ((_selectionStart.X > 0 || _selectionEnd.X < _lines[coord.Y].Length) && _selectionStart != _selectionEnd)
				{
					// Something is selected so select whole line
					_selectionStart.X = 0;
					_selectionStart.Y = coord.Y;
					_selectionEnd.X = _lines[coord.Y].Length;
					_selectionEnd.Y = coord.Y;

					Refresh();

					return;
				}
			}

			_selectionStart = coord;
			_selectionEnd = coord;

			var ch = GetChar(coord.Y, coord.X);

			var attr = GetAttr(coord.Y, coord.X);

			if (attr == (int)EditorColor.Strings)
			{
				for (var p = coord.X - 1; p >= 0; p--)
				{
					if (GetChar(coord.Y, p) == '\"')
					{
						_selectionStart.X = p;
						break;
					}
					_selectionStart.X = p;
				}
				for (var p = coord.X; ; p++)
				{
					if (GetChar(coord.Y, p) == '\"')
					{
						_selectionEnd.X = p + 1;
						break;
					}
					_selectionEnd.X = p + 1;
				}
			}
			else if (isANumberChar(ch))
			{
				for (var p = coord.X - 1; p >= 0; p--)
				{
					if (!isANumberChar(GetChar(coord.Y, p)))
					{
						break;
					}
					_selectionStart.X = p;
				}
				for (var p = coord.X; ; p++)
				{
					if (!isANumberChar(GetChar(coord.Y, p)))
					{
						break;
					}
					_selectionEnd.X = p + 1;
				}
			}
			else if (isAnIdentifierChar(ch))
			{
				for (var p = coord.X - 1; p >= 0; p--)
				{
					if (!isAnIdentifierChar(GetChar(coord.Y, p)))
					{
						break;
					}
					_selectionStart.X = p;
				}
				for (var p = coord.X; ; p++)
				{
					if (!isAnIdentifierChar(GetChar(coord.Y, p)))
					{
						break;
					}
					_selectionEnd.X = p + 1;
				}
			}
			else
			{
				_selectionEnd.X++;
			}

			Refresh();
		}

		private void doMouseScroll(int y)
		{
			MoveCursor(y * _lines.Count / (ClientSize.Height - 4), _symbol);

			_blink = 0;

			Refresh();
		}
	}
}
