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

				if (p > 0 && (Char.IsLetter(line[p - 1]) || line[p - 1] == '_'))
				{
					ok = false;
				}

				if (p + word.Length < line.Length - 1 && (Char.IsLetter(line[p + word.Length]) || line[p + word.Length] == '_'))
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

				for (var j = 0; j < _lines[line + i].Length; j++)
				{
					if (Char.IsSymbol(_lines[line + i][j]) || Char.IsPunctuation(_lines[line + i][j]))
					{
						SetAttribute(line + i, j, 1, 5, 0);
					}
				}

				markComments(line + i);

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
			}
		}

		private void markComments(int line)
		{
			var p = _lines[line].IndexOf(SingleLineComment);
			if (p >= 0)
			{
				SetAttribute(line, p, _lines[line].Length - p, 2, 0);
			}
		}
	}
}
