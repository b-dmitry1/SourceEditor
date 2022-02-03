using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected void AddChar(char ch, bool user = true)
		{
			while (_lines.Count <= _line)
			{
				_lines.Add("");
			}

			if (ch == '\n')
			{
				return;
			}

			if (user)
			{
				if (_selectionStart != _selectionEnd)
				{
					saveMassUndoFromStartOfSelection();

					ClearSelection();

					if (ch == '\b')
					{
						return;
					}
				}
			}

			if (ch == '\r')
			{
				var indent = 0;
				if (_symbol < _lines[_line].Length)
				{
					_lines.Insert(_line + 1, _lines[_line].Substring(_symbol));
					_lines[_line] = _lines[_line].Remove(_symbol);
				}
				else
				{
					for (var i = 0; i < _lines[_line].Length; i++)
					{
						if (_lines[_line][i] != ' ')
							break;
						indent++;
					}
					_lines.Insert(_line + 1, new string(' ', indent));
				}
				moveCursor(_line + 1, indent);
			}
			else if (ch == '\b')
			{
				if (_symbol == 0)
				{
					if (user)
					{
						saveMassUndo(_line - 1, 1, 2);
					}
					if (_line > 0)
					{
						var len = _lines[_line - 1].Length;

						_lines[_line - 1] += _lines[_line];

						_lines.RemoveAt(_line);

						moveCursor(_line - 1, len);
					}
				}
				else
				{
					if (user)
					{
						saveCharUndo();
					}
					if (_symbol <= _lines[_line].Length)
					{
						_lines[_line] = _lines[_line].Remove(_symbol - 1, 1);
					}
					moveCursor(_line, _symbol - 1);
				}
			}
			else
			{
				saveCharUndo();
				if (_lines[_line].Length < _symbol)
				{
					_lines[_line] += new string(' ', _symbol - _lines[_line].Length) + ch;
				}
				else
				{
					_lines[_line] = _lines[_line].Insert(_symbol, ch.ToString());
				}
				moveCursor(_line, _symbol + 1);
			}
		}

		private void DeleteChar()
		{
			if (_line >= _lines.Count)
			{
				return;
			}

			if (_symbol < _lines[_line].Length)
			{
				_lines[_line] = _lines[_line].Remove(_symbol, 1);
				linesChanged(_line, 1);
				return;
			}

			if (_line == _lines.Count - 1)
			{
				return;
			}

			if (_symbol > _lines[_line].Length)
			{
				_lines[_line] += new string(' ', _symbol - _lines[_line].Length);
			}

			_lines[_line] += _lines[_line + 1];

			_lines.RemoveAt(_line + 1);

			linesChanged(_line - 1, _lines.Count - _line + 1);
		}
	}
}
