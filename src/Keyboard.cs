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
				LinesChanged(_line - 1, _lines.Count - _line + 1);
			}
			else
			{
				LinesChanged(Math.Max(_line - 1, 0), 3);
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
					MoveCursor(_line, _selectionEnd.X, true);
					break;
				case Keys.Shift | Keys.Left:
					updateSelectionStart();
					_selectionEnd.X = Math.Max(_selectionEnd.X - 1, 0);
					MoveCursor(_line, _selectionEnd.X, true);
					break;
				case Keys.Shift | Keys.Down:
					updateSelectionStart();
					_selectionEnd.Y++;
					MoveCursor(_selectionEnd.Y, _symbol, true);
					break;
				case Keys.Shift | Keys.Up:
					updateSelectionStart();
					_selectionEnd.Y = Math.Max(_selectionEnd.Y - 1, 0);
					MoveCursor(_selectionEnd.Y, _symbol, true);
					break;
				case Keys.Up:
					MoveCursor(_line - 1, _symbol);
					break;
				case Keys.Down:
					MoveCursor(_line + 1, _symbol);
					break;
				case Keys.Left:
					MoveCursor(_line, _symbol - 1);
					break;
				case Keys.Right:
					MoveCursor(_line, _symbol + 1);
					break;
				case Keys.Control | Keys.Home:
					MoveCursor(0, 0);
					break;
				case Keys.Home:
					MoveCursor(_line, 0);
					break;
				case Keys.End:
					MoveCursor(_line, _lines[_line].Length);
					break;
				case Keys.Control | Keys.End:
					MoveCursor(_lines.Count - 1, _lines[_lines.Count - 1].Length);
					break;
				case Keys.Prior:
					MoveCursor(_line - (ClientSize.Height / _charHeight - 1), _symbol);
					break;
				case Keys.Next:
					MoveCursor(_line + (ClientSize.Height / _charHeight - 1), _symbol);
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

						OnTextChanged(new EventArgs());
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
