# SourceEditor

- Syntax highlighting
- Keyboard and mouse control
- Cut, Copy, Paste, Delete
- Zooming with Ctrl+Wheel
- Undo

## Appearance

Defining a keywords:

```csharp
editor.Keywords = new string[] { "for", "while", "do", "continue", "break", "switch", "if", "else" };
```

Defining a common identifiers:

```csharp
editor.Identifiers = new string[] { "text", "font", "zoom", "width", "height" };
```

Defining single line comment symbol:

```csharp
editor.SingleLineComment = "--";
```

### Custom color scheme

The highlighter uses 6 default colors:
- Keywords (LightSkyBlue)
- Comments (LightGreen)
- Strings (PeachPuff)
- Punctuation (LemonChiffon)
- Identifiers (Moccasin)
- Default (WhiteSmoke)

To change default colors:

```csharp
editor.SetColor(Keywords, Color.Blue);
```

Also you can change:
- BackColor
- SelectionColor
- ScrollBarColor
- ScrollBarThumbColor
- ScrollBarWidth

## Methods

```csharp
// Cursor position
void MoveCursor(int line, int symbol);

// Load and Save
void LoadFromFile(string fileName);
void SaveToFile(string fileName);

// Clipboard and selection
void Cut();
void Copy();
void Paste();
void ClearSelection();

// Undo
void Undo();

// Keyboard input simulation:
void AddChar(char ch);
void DeleteChar();

// And this should be called if you modify Lines manually:
void LinesChanged(int first, int count);
```

 

![Screenshot](pictures/editor.png)
