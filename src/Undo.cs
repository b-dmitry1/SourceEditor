using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SourceEditor
{
	public partial class SourceEditor : UserControl
	{
		public static int SameLineUndoChars = 5;

		protected Stack<UndoRecord> _undo = new Stack<UndoRecord>();

		protected class UndoRecord
		{
			public int Line;
			public int Symbol;
			public int First;
			public int LinesChanged;
			public int CharsLeft;
			public List<string> Text = new List<string>();
		}

		protected void preventAddingCharsToLastUndo()
		{
			if (_undo.Count > 0)
			{
				var top = _undo.Peek();
				{
					top.CharsLeft = 0;
				}
			}
		}

		protected void saveCharUndo()
		{
			if (_undo.Count > 0)
			{
				var top = _undo.Peek();
				if (top.Line == _line && top.First == _line && top.LinesChanged == 1 && top.CharsLeft > 0)
				{
					// Same line
					top.CharsLeft--;
					return;
				}
			}

			var undo = new UndoRecord { Line = _line, Symbol = _symbol, First = _line, LinesChanged = 1, CharsLeft = SameLineUndoChars };
			undo.Text.Add(_lines[_line]);
			_undo.Push(undo);
		}

		protected void saveMassUndo(int startLine, int changed = 0, int save = 0)
		{
			if (changed == 0)
			{
				changed = _lines.Count - startLine;
			}

			if (save == 0)
			{
				save = changed;
			}

			startLine = Math.Max(0, startLine);
			changed = Math.Min(_lines.Count - startLine, changed);
			save = Math.Min(_lines.Count - startLine, save);

			var undo = new UndoRecord { Line = _selectionStart.Y, Symbol = _selectionStart.X, First = startLine, LinesChanged = changed };
			for (var i = undo.First; i < save + undo.First; i++)
			{
				undo.Text.Add(_lines[i]);
			}
			_undo.Push(undo);
		}

		protected void saveMassUndoFromStartOfSelection()
		{
			Point selStart;
			Point selEnd;

			normalizeSelection(out selStart, out selEnd);

			saveMassUndo(selStart.Y);
		}

		public void Undo()
		{
			if (_undo.Count == 0)
			{
				return;
			}

			var undo = _undo.Pop();

			_lines.RemoveRange(undo.First, Math.Min(_lines.Count - undo.First, undo.LinesChanged));

			_lines.InsertRange(undo.First, undo.Text);

			// _line = undo.Line;
			// _symbol = undo.Symbol;

			MoveCursor(undo.Line, undo.Symbol);

			LinesChanged(0, _lines.Count);

			OnTextChanged(new EventArgs());
		}
	}
}
