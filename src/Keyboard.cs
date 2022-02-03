using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			AddChar(e.KeyChar);

			if (e.KeyChar == '\r' || e.KeyChar == '\b')
			{
				linesChanged(_line - 1, _lines.Count - _line + 1);
			}
			else
			{
				linesChanged(Math.Max(_line - 1, 0), 3);
			}

			Refresh();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Shift | Keys.Right:
					updateSelectionStart();
					_selectionEnd.X++;
					moveCursor(_line, _selectionEnd.X, true);
					break;
				case Keys.Shift | Keys.Left:
					updateSelectionStart();
					_selectionEnd.X = Math.Max(_selectionEnd.X - 1, 0);
					moveCursor(_line, _selectionEnd.X, true);
					break;
				case Keys.Shift | Keys.Down:
					updateSelectionStart();
					_selectionEnd.Y++;
					moveCursor(_selectionEnd.Y, _symbol, true);
					break;
				case Keys.Shift | Keys.Up:
					updateSelectionStart();
					_selectionEnd.Y = Math.Max(_selectionEnd.Y - 1, 0);
					moveCursor(_selectionEnd.Y, _symbol, true);
					break;
				case Keys.Up:
					moveCursor(_line - 1, _symbol);
					break;
				case Keys.Down:
					moveCursor(_line + 1, _symbol);
					break;
				case Keys.Left:
					moveCursor(_line, _symbol - 1);
					break;
				case Keys.Right:
					moveCursor(_line, _symbol + 1);
					break;
				case Keys.Control | Keys.Home:
					moveCursor(0, 0);
					break;
				case Keys.Home:
					moveCursor(_line, 0);
					break;
				case Keys.End:
					moveCursor(_line, _lines[_line].Length);
					break;
				case Keys.Control | Keys.End:
					moveCursor(_lines.Count - 1, _lines[_lines.Count - 1].Length);
					break;
				case Keys.Prior:
					moveCursor(_line - (ClientSize.Height / _charHeight - 1), _symbol);
					break;
				case Keys.Next:
					moveCursor(_line + (ClientSize.Height / _charHeight - 1), _symbol);
					break;
				case Keys.Tab:
					break;
				case Keys.Delete:
					if (_selectionStart == _selectionEnd)
					{
						DeleteChar();
					}
					else
					{
						saveMassUndoFromStartOfSelection();

						ClearSelection();
					}
					break;
				case Keys.Shift | Keys.Delete:
				case Keys.Control | Keys.X:
					Cut();
					break;
				case Keys.Control | Keys.Insert:
				case Keys.Control | Keys.C:
					Copy();
					break;
				case Keys.Shift | Keys.Insert:
				case Keys.Control | Keys.V:
					Paste();
					break;
				case Keys.Control | Keys.Z:
					Undo();
					break;
				default:
					return false;
			}

			Refresh();
			return true;
		}
	}
}
