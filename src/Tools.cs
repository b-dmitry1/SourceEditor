using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		protected bool isAnIdentifierChar(char ch)
		{
			return char.IsLetterOrDigit(ch) || ch == '_';
		}

		protected bool isANumberChar(char ch)
		{
			return char.IsDigit(ch) || ch == '.';
		}

		protected bool isInSelection(int line, int symbol)
		{
			if (_selectionStart == _selectionEnd)
			{
				return false;
			}

			if (line < 0 || line >= _lines.Count)
			{
				return false;
			}

			if (line < _selectionStart.Y || line > _selectionEnd.Y)
			{
				return false;
			}

			if (_selectionStart.Y == _selectionEnd.Y)
			{
				return symbol >= _selectionStart.X && symbol <= _selectionEnd.X;
			}

			if (line == _selectionStart.Y)
			{
				return symbol >= _selectionStart.X;
			}

			if (line == _selectionEnd.Y)
			{
				return symbol <= _selectionEnd.X;
			}

			return true;
		}

		public char GetChar(int line, int symbol)
		{
			if (line < 0 || line >= _lines.Count || symbol < 0)
			{
				return (char)0;
			}

			if (symbol >= _lines[line].Length)
			{
				return (char)0;
			}

			return _lines[line][symbol];
		}

		public int GetAttr(int line, int symbol)
		{
			if (line < 0 || line >= _attributes.Count || symbol < 0)
			{
				return 0;
			}

			if (symbol >= _attributes[line].Length)
			{
				return 0;
			}

			return _attributes[line][symbol];
		}
	}
}
