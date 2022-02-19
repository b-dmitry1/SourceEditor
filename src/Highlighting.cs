using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected void markKeyword(string line, int linenumber, string word, int charColor, int backgroundColor)
		{
			var p = 0;

			while (true)
			{
				p = line.IndexOf(word, p);

				if (p < 0)
				{
					break;
				}

				var ok = true;

				if (p > 0 && (Char.IsLetter(line[p - 1]) || Char.IsDigit(line[p - 1]) || line[p - 1] == '_'))
				{
					ok = false;
				}

				if (p + word.Length < line.Length - 1 && (Char.IsLetter(line[p + word.Length]) ||
					Char.IsDigit(line[p + word.Length]) || line[p + word.Length] == '_'))
				{
					ok = false;
				}

				if (ok)
				{
					SetAttribute(linenumber, p, word.Length, charColor, backgroundColor);
				}

				p++;
			}
		}

		protected virtual void highlightSyntax(int line, int count)
		{
			for (var i = 0; i < count; i++)
			{
				if (line + i >= _lines.Count)
				{
					break;
				}

				SetAttribute(line + i, 0, _lines[line + i].Length, 7, 0);

				foreach (var word in _keywords)
				{
					markKeyword(_lines[line + i], line + i, word, 1, 0);
				}

				foreach (var word in _identifiers)
				{
					markKeyword(_lines[line + i], line + i, word, 6, 0);
				}

				for (var j = 0; j < _lines[line + i].Length; j++)
				{
					if (Char.IsSymbol(_lines[line + i][j]) || Char.IsPunctuation(_lines[line + i][j]))
					{
						SetAttribute(line + i, j, 1, 5, 0);
					}
				}

				var found = false;
				for (var j = 0; j < _lines[line + i].Length; j++)
				{
					if (_lines[line + i][j] == '\"')
					{
						found = !found;
						SetAttribute(line + i, j, 1, 4, 0);
					}
					else
					{
						if (found)
						{
							SetAttribute(line + i, j, 1, 4, 0);
						}
					}
				}

				markComments(line + i);
			}

			markMultilineComments();
		}

		private void markComments(int line)
		{
			int p = -1;
			while (true)
			{
				p = _lines[line].IndexOf(SingleLineComment, p + 1);
				if (p >= 0)
				{
					if (GetAttr(line, p) != (int)EditorColor.Strings)
					{
						SetAttribute(line, p, _lines[line].Length - p, 2, 0);
					}
				}
				else
				{
					break;
				}
			}
		}

		private void markMultilineComments(string start, string end)
		{
			var comment = false;
			var startline = 0;
			var startsym = 0;

			for (int line = 0, pos = 0; line < _lines.Count;)
			{
				if (comment)
				{
					pos = _lines[line].IndexOf(end, pos);
					if (pos < 0)
					{
						if (startline == line)
						{
							SetAttribute(line, startsym, _lines[line].Length - startsym, (int)EditorColor.MultilineComments, 0);
						}
						else
						{
							SetAttribute(line, 0, _lines[line].Length, (int)EditorColor.MultilineComments, 0);
						}
						line++;
						pos = 0;
						continue;
					}
					else
					{
						pos += end.Length;

						if (startline == line)
						{
							SetAttribute(line, startsym, pos - startsym, (int)EditorColor.MultilineComments, 0);
						}
						else
						{
							SetAttribute(line, 0, pos, (int)EditorColor.MultilineComments, 0);
						}

						comment = !comment;
					}
				}
				else
				{
					pos = _lines[line].IndexOf(start, pos);
					if (pos < 0)
					{
						line++;
						pos = 0;
						continue;
					}
					startline = line;
					startsym = pos;

					pos += start.Length;

					comment = !comment;
				}

				/*
				if (comment)
				{
					if (startline == line)
					{
						SetAttribute(line, startsym, _lines[line].Length - startsym, (int)EditorColor.MultilineComments, 0);
					}
					else
					{
						SetAttribute(line, 0, _lines[line].Length, (int)EditorColor.MultilineComments, 0);
					}
				}
				*/
			}
		}

		private void markMultilineComments()
		{
			for (var line = 0; line < _attributes.Count; line++)
			{
				for (var sym = 0; sym < _attributes[line].Length; sym++)
				{
					if (_attributes[line][sym] == (int)EditorColor.MultilineComments)
					{
						_attributes[line][sym] = 0;
					}
				}
			}

			for (var index = 0; index < _multilineCommentStartSymbols.Count; index++)
			{
				if (index >= _multilineCommentEndSymbols.Count)
				{
					break;
				}
				markMultilineComments(_multilineCommentStartSymbols[index], _multilineCommentEndSymbols[index]);
			}
		}
	}
}
