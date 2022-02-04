using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		public void Cut()
		{
			saveMassUndoFromStartOfSelection();

			Copy(true);

			ClearSelection();

			OnTextChanged(new EventArgs());
		}

		public void Copy(bool keepSelection = false)
		{
			if (_selectionStart == _selectionEnd)
			{
				return;
			}

			Point selStart;
			Point selEnd;

			normalizeSelection(out selStart, out selEnd);

			if (selStart.Y >= _lines.Count)
			{
				return;
			}

			selEnd.Y = Math.Min(selEnd.Y, _lines.Count - 1);

			var text = new StringBuilder();

			if (selStart.Y == selEnd.Y)
			{
				text.Append(getFragment(selStart.Y, selStart.X, selEnd.X - selStart.X));
			}
			else
			{
				text.AppendLine(_lines[selStart.Y].Substring(selStart.X));
				for (var line = selStart.Y + 1; line < selEnd.Y; line++)
				{
					text.AppendLine(_lines[line]);
				}
				if (selEnd.X > 0)
				{
					text.Append(_lines[selEnd.Y].Substring(0, selEnd.X));
				}
			}

			Clipboard.SetText(text.ToString());

			if (!keepSelection)
			{
				_selectionStart = _selectionEnd;
			}

			OnTextChanged(new EventArgs());
		}

		public void Paste()
		{
			if (!Clipboard.ContainsText())
			{
				return;
			}

			if (_selectionStart != _selectionEnd)
			{
				saveMassUndoFromStartOfSelection();
			}
			else
			{
				saveMassUndo(_line);
			}

			ClearSelection();

			var text = Clipboard.GetText();
			foreach (var ch in text)
			{
				AddChar(ch, false);
			}

			linesChanged(0, _lines.Count);

			OnTextChanged(new EventArgs());
		}

		public void ClearSelection()
		{
			if (_selectionStart == _selectionEnd)
			{
				return;
			}

			Point selStart;
			Point selEnd;

			normalizeSelection(out selStart, out selEnd);

			if (selStart.Y >= _lines.Count)
			{
				return;
			}

			selEnd.Y = Math.Min(selEnd.Y, _lines.Count - 1);

			if (selStart.X >= _lines[selStart.Y].Length)
			{
				_lines[selStart.Y] += new string(' ', selStart.X - _lines[selStart.Y].Length + 1);
			}

			if (selEnd.X >= _lines[selEnd.Y].Length)
			{
				_lines[selEnd.Y] += new string(' ', selEnd.X - _lines[selEnd.Y].Length + 1);
			}

			selStart.X = Math.Min(selStart.X, _lines[selStart.Y].Length);
			selEnd.X = Math.Min(selEnd.X, _lines[selEnd.Y].Length);

			if (selStart.Y == selEnd.Y)
			{
				_lines[selStart.Y] = _lines[selStart.Y].Remove(selStart.X, selEnd.X - selStart.X);
			}
			else
			{
				if (selStart.X == 0)
				{
					for (var i = 0; i < selEnd.Y - selStart.Y; i++)
					{
						_lines.RemoveAt(selStart.Y);
					}
				}
				else
				{
					if (selStart.X < _lines[selStart.Y].Length)
					{
						_lines[selStart.Y] = _lines[selStart.Y].Substring(0, selStart.X) +
							_lines[selEnd.Y].Substring(Math.Min(selEnd.X, _lines[selEnd.Y].Length - 1));
						_lines[selEnd.Y] = _lines[selEnd.Y].Remove(0, Math.Min(selEnd.X, _lines[selEnd.Y].Length - 1));
					}
					for (var i = 0; i < selEnd.Y - selStart.Y; i++)
					{
						_lines.RemoveAt(selStart.Y + 1);
					}
				}
			}

			_line = selStart.Y;
			_symbol = selStart.X;

			_selectionStart = _selectionEnd = selStart;

			linesChanged(0, _lines.Count);
		}

		private string getFragment(int line, int symbol, int count)
		{
			if (line >= _lines.Count)
			{
				return new string(' ', count);
			}

			if (symbol >= _lines[line].Length)
			{
				return new string(' ', count);
			}

			if (symbol + count > _lines[line].Length)
			{
				return _lines[line].Substring(symbol) + (new string(' ', symbol + count - _lines[line].Length));
			}

			return _lines[line].Substring(symbol, count);
		}
	}
}
