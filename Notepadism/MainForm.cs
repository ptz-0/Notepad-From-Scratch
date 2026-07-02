using System.Text;

namespace NotepadClone;

public partial class MainForm : Form
{
    private readonly TextBox _textBox;
    private readonly MenuStrip _menuStrip;
    private readonly StatusStrip _statusStrip;
    private readonly ToolStripStatusLabel _positionLabel;
    private readonly ToolStripMenuItem _wordWrapItem;
    private readonly ToolStripMenuItem _statusBarItem;

    private string? _currentFilePath;
    private bool _isDirty;
    private Encoding _currentEncoding = new UTF8Encoding(false);
    private int _zoomPercent = 100;
    private Font _baseFont = new("Consolas", 11f);
    private readonly System.Drawing.Printing.PageSettings _pageSettings = new();

    private FindForm? _findForm;
    private ReplaceForm? _replaceForm;
    private string _lastSearchTerm = string.Empty;
    private bool _lastSearchDown = true;
    private bool _lastSearchMatchCase = false;

    public MainForm()
    {
        Text = "Untitled - Notepad";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Fall back to the default WinForms icon if extraction fails for any reason.
        }

        _textBox = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            AcceptsTab = true,
            Font = _baseFont,
            BorderStyle = BorderStyle.None
        };
        _textBox.TextChanged += (_, _) => MarkDirty();
        _textBox.KeyUp += (_, _) => UpdatePositionLabel();
        _textBox.Click += (_, _) => UpdatePositionLabel();
        _textBox.MouseWheel += TextBox_MouseWheel;

        _positionLabel = new ToolStripStatusLabel { Text = "Ln 1, Col 1", Spring = false, TextAlign = ContentAlignment.MiddleRight };
        _statusStrip = new StatusStrip { Visible = false };
        _statusStrip.Items.Add(_positionLabel);

        _menuStrip = new MenuStrip();
        _wordWrapItem = new ToolStripMenuItem("Word Wrap") { CheckOnClick = true, Checked = true };
        _statusBarItem = new ToolStripMenuItem("Status Bar") { CheckOnClick = true, Checked = false };

        BuildMenus();

        Controls.Add(_textBox);
        Controls.Add(_statusStrip);
        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;

        FormClosing += MainForm_FormClosing;
    }

    public void OpenFileOnStartup(string path)
    {
        LoadFile(path);
    }

    // ---------- Menu construction ----------

    private void BuildMenus()
    {
        _menuStrip.Items.Add(BuildFileMenu());
        _menuStrip.Items.Add(BuildEditMenu());
        _menuStrip.Items.Add(BuildFormatMenu());
        _menuStrip.Items.Add(BuildViewMenu());
        _menuStrip.Items.Add(BuildHelpMenu());
    }

    private ToolStripMenuItem BuildFileMenu()
    {
        var file = new ToolStripMenuItem("File");

        var newItem = new ToolStripMenuItem("New", null, (_, _) => NewFile()) { ShortcutKeys = Keys.Control | Keys.N };
        var newWindowItem = new ToolStripMenuItem("New Window", null, (_, _) => OpenNewWindow()) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.N };
        var openItem = new ToolStripMenuItem("Open...", null, (_, _) => OpenFile()) { ShortcutKeys = Keys.Control | Keys.O };
        var saveItem = new ToolStripMenuItem("Save", null, (_, _) => SaveFile()) { ShortcutKeys = Keys.Control | Keys.S };
        var saveAsItem = new ToolStripMenuItem("Save As...", null, (_, _) => SaveFileAs()) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.S };
        var pageSetupItem = new ToolStripMenuItem("Page Setup...", null, (_, _) => PageSetup());
        var printItem = new ToolStripMenuItem("Print...", null, (_, _) => PrintDocument()) { ShortcutKeys = Keys.Control | Keys.P };
        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) => Close());

        file.DropDownItems.Add(newItem);
        file.DropDownItems.Add(newWindowItem);
        file.DropDownItems.Add(openItem);
        file.DropDownItems.Add(saveItem);
        file.DropDownItems.Add(saveAsItem);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(pageSetupItem);
        file.DropDownItems.Add(printItem);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(exitItem);

        return file;
    }

    private ToolStripMenuItem BuildEditMenu()
    {
        var edit = new ToolStripMenuItem("Edit");

        var undoItem = new ToolStripMenuItem("Undo", null, (_, _) => _textBox.Undo()) { ShortcutKeys = Keys.Control | Keys.Z };
        var cutItem = new ToolStripMenuItem("Cut", null, (_, _) => _textBox.Cut()) { ShortcutKeys = Keys.Control | Keys.X };
        var copyItem = new ToolStripMenuItem("Copy", null, (_, _) => _textBox.Copy()) { ShortcutKeys = Keys.Control | Keys.C };
        var pasteItem = new ToolStripMenuItem("Paste", null, (_, _) => _textBox.Paste()) { ShortcutKeys = Keys.Control | Keys.V };
        var deleteItem = new ToolStripMenuItem("Delete", null, (_, _) => DeleteSelection()) { ShortcutKeys = Keys.Delete };
        var findItem = new ToolStripMenuItem("Find...", null, (_, _) => ShowFind()) { ShortcutKeys = Keys.Control | Keys.F };
        var findNextItem = new ToolStripMenuItem("Find Next", null, (_, _) => FindNext(_lastSearchDown)) { ShortcutKeys = Keys.F3 };
        var replaceItem = new ToolStripMenuItem("Replace...", null, (_, _) => ShowReplace()) { ShortcutKeys = Keys.Control | Keys.H };
        var goToItem = new ToolStripMenuItem("Go To...", null, (_, _) => ShowGoTo()) { ShortcutKeys = Keys.Control | Keys.G };
        var selectAllItem = new ToolStripMenuItem("Select All", null, (_, _) => _textBox.SelectAll()) { ShortcutKeys = Keys.Control | Keys.A };
        var timeDateItem = new ToolStripMenuItem("Time/Date", null, (_, _) => InsertTimeDate()) { ShortcutKeys = Keys.F5 };

        edit.DropDownItems.Add(undoItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add(cutItem);
        edit.DropDownItems.Add(copyItem);
        edit.DropDownItems.Add(pasteItem);
        edit.DropDownItems.Add(deleteItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add(findItem);
        edit.DropDownItems.Add(findNextItem);
        edit.DropDownItems.Add(replaceItem);
        edit.DropDownItems.Add(goToItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add(selectAllItem);
        edit.DropDownItems.Add(timeDateItem);

        return edit;
    }

    private ToolStripMenuItem BuildFormatMenu()
    {
        var format = new ToolStripMenuItem("Format");

        _wordWrapItem.Click += (_, _) => ToggleWordWrap();
        var fontItem = new ToolStripMenuItem("Font...", null, (_, _) => ChooseFont());

        format.DropDownItems.Add(_wordWrapItem);
        format.DropDownItems.Add(fontItem);

        return format;
    }

    private ToolStripMenuItem BuildViewMenu()
    {
        var view = new ToolStripMenuItem("View");
        var zoom = new ToolStripMenuItem("Zoom");
        var zoomIn = new ToolStripMenuItem("Zoom In", null, (_, _) => ChangeZoom(10)) { ShortcutKeys = Keys.Control | Keys.Oemplus };
        var zoomOut = new ToolStripMenuItem("Zoom Out", null, (_, _) => ChangeZoom(-10)) { ShortcutKeys = Keys.Control | Keys.OemMinus };
        var zoomRestore = new ToolStripMenuItem("Restore Default Zoom", null, (_, _) => ResetZoom()) { ShortcutKeys = Keys.Control | Keys.D0 };
        zoom.DropDownItems.Add(zoomIn);
        zoom.DropDownItems.Add(zoomOut);
        zoom.DropDownItems.Add(zoomRestore);

        _statusBarItem.Click += (_, _) => ToggleStatusBar();

        view.DropDownItems.Add(zoom);
        view.DropDownItems.Add(_statusBarItem);

        return view;
    }

    private ToolStripMenuItem BuildHelpMenu()
    {
        var help = new ToolStripMenuItem("Help");
        var aboutItem = new ToolStripMenuItem("About Notepad", null, (_, _) => ShowAbout());
        help.DropDownItems.Add(aboutItem);
        return help;
    }

    // ---------- File operations ----------

    private void NewFile()
    {
        if (!ConfirmDiscardChanges()) return;
        _textBox.Clear();
        _currentFilePath = null;
        _isDirty = false;
        _currentEncoding = new UTF8Encoding(false);
        UpdateTitle();
    }

    private static void OpenNewWindow()
    {
        try
        {
            System.Diagnostics.Process.Start(Application.ExecutablePath);
        }
        catch
        {
            // Ignore if we can't spawn a second instance (e.g. running from a debugger).
        }
    }

    private void OpenFile()
    {
        if (!ConfirmDiscardChanges()) return;

        using var dialog = new OpenFileDialog
        {
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            FilterIndex = 1
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void LoadFile(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            _currentEncoding = DetectEncoding(bytes);
            _textBox.Text = _currentEncoding.GetString(StripPreamble(bytes, _currentEncoding));
            _currentFilePath = path;
            _isDirty = false;
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not open the file.\n\n{ex.Message}", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static byte[] StripPreamble(byte[] bytes, Encoding encoding)
    {
        byte[] preamble = encoding.GetPreamble();
        if (preamble.Length > 0 && bytes.Length >= preamble.Length)
        {
            bool matches = true;
            for (int i = 0; i < preamble.Length; i++)
            {
                if (bytes[i] != preamble[i]) { matches = false; break; }
            }
            if (matches)
            {
                var trimmed = new byte[bytes.Length - preamble.Length];
                Array.Copy(bytes, preamble.Length, trimmed, 0, trimmed.Length);
                return trimmed;
            }
        }
        return bytes;
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(true);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;
        return new UTF8Encoding(false);
    }

    private bool SaveFile()
    {
        if (_currentFilePath is null) return SaveFileAs();

        try
        {
            File.WriteAllText(_currentFilePath, _textBox.Text, _currentEncoding);
            _isDirty = false;
            UpdateTitle();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save the file.\n\n{ex.Message}", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private bool SaveFileAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            FilterIndex = 1,
            FileName = _currentFilePath is null ? "Untitled.txt" : Path.GetFileName(_currentFilePath)
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _currentFilePath = dialog.FileName;
            return SaveFile();
        }
        return false;
    }

    private void PageSetup()
    {
        using var pageSetupDialog = new PageSetupDialog { PageSettings = _pageSettings };
        pageSetupDialog.ShowDialog(this);
    }

    private void PrintDocument()
    {
        using var printDialog = new PrintDialog();
        using var doc = new System.Drawing.Printing.PrintDocument();
        doc.DefaultPageSettings = _pageSettings;
        printDialog.Document = doc;

        if (printDialog.ShowDialog(this) != DialogResult.OK) return;

        string text = _textBox.Text;
        int charsPrinted = 0;

        doc.PrintPage += (_, e) =>
        {
            if (e.Graphics is null) return;
            using var font = _baseFont;
            float lineHeight = font.GetHeight(e.Graphics);
            float y = e.MarginBounds.Top;
            var lines = text.Substring(charsPrinted).Split('\n');
            int consumed = 0;

            foreach (var rawLine in lines)
            {
                if (y + lineHeight > e.MarginBounds.Bottom) break;
                string line = rawLine.TrimEnd('\r');
                e.Graphics.DrawString(line, font, Brushes.Black, e.MarginBounds.Left, y);
                y += lineHeight;
                consumed += rawLine.Length + 1;
            }

            charsPrinted += consumed;
            e.HasMorePages = charsPrinted < text.Length;
        };

        doc.Print();
    }

    private bool ConfirmDiscardChanges()
    {
        if (!_isDirty) return true;

        string name = _currentFilePath is null ? "Untitled" : Path.GetFileName(_currentFilePath);
        var result = MessageBox.Show(this, $"Do you want to save changes to {name}?", "Notepad",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        return result switch
        {
            DialogResult.Yes => SaveFile(),
            DialogResult.No => true,
            _ => false
        };
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscardChanges())
        {
            e.Cancel = true;
        }
    }

    // ---------- Edit operations ----------

    private void DeleteSelection()
    {
        if (_textBox.SelectionLength > 0)
        {
            _textBox.SelectedText = string.Empty;
        }
    }

    private void InsertTimeDate()
    {
        _textBox.SelectedText = DateTime.Now.ToString("h:mm tt M/d/yyyy");
    }

    private void ShowFind()
    {
        if (_findForm is { IsDisposed: false })
        {
            _findForm.Focus();
            return;
        }
        _findForm = new FindForm(this, _lastSearchTerm, _lastSearchMatchCase);
        _findForm.Show(this);
    }

    private void ShowReplace()
    {
        if (_replaceForm is { IsDisposed: false })
        {
            _replaceForm.Focus();
            return;
        }
        _replaceForm = new ReplaceForm(this, _lastSearchTerm, _lastSearchMatchCase);
        _replaceForm.Show(this);
    }

    private void ShowGoTo()
    {
        if (_wordWrapItem.Checked)
        {
            MessageBox.Show(this, "Go To is not available when Word Wrap is turned on.", "Notepad",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var goToForm = new GoToForm(_textBox.Lines.Length);
        if (goToForm.ShowDialog(this) == DialogResult.OK)
        {
            int lineIndex = Math.Clamp(goToForm.LineNumber - 1, 0, Math.Max(0, _textBox.Lines.Length - 1));
            int charIndex = _textBox.GetFirstCharIndexFromLine(lineIndex);
            if (charIndex >= 0)
            {
                _textBox.SelectionStart = charIndex;
                _textBox.SelectionLength = 0;
                _textBox.ScrollToCaret();
                _textBox.Focus();
            }
        }
    }

    public void FindNext(bool searchDown, string? term = null, bool? matchCase = null)
    {
        string search = term ?? _lastSearchTerm;
        if (string.IsNullOrEmpty(search)) return;

        _lastSearchTerm = search;
        _lastSearchDown = searchDown;
        _lastSearchMatchCase = matchCase ?? _lastSearchMatchCase;

        string haystack = _textBox.Text;
        StringComparison comparison = _lastSearchMatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        int startIndex;
        int foundIndex;

        if (searchDown)
        {
            startIndex = _textBox.SelectionStart + _textBox.SelectionLength;
            foundIndex = haystack.IndexOf(search, Math.Min(startIndex, haystack.Length), comparison);
            if (foundIndex < 0)
                foundIndex = haystack.IndexOf(search, 0, comparison); // wrap around
        }
        else
        {
            startIndex = _textBox.SelectionStart;
            string before = startIndex <= haystack.Length ? haystack[..startIndex] : haystack;
            foundIndex = before.LastIndexOf(search, comparison);
            if (foundIndex < 0)
                foundIndex = haystack.LastIndexOf(search, comparison); // wrap around
        }

        if (foundIndex >= 0)
        {
            _textBox.SelectionStart = foundIndex;
            _textBox.SelectionLength = search.Length;
            _textBox.ScrollToCaret();
            _textBox.Focus();
        }
        else
        {
            MessageBox.Show(this, $"Cannot find \"{search}\"", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public void ReplaceCurrent(string findText, string replaceText, bool matchCase)
    {
        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (_textBox.SelectionLength > 0 &&
            string.Equals(_textBox.SelectedText, findText, comparison))
        {
            _textBox.SelectedText = replaceText;
        }
        FindNext(true, findText, matchCase);
    }

    public void ReplaceAll(string findText, string replaceText, bool matchCase)
    {
        if (string.IsNullOrEmpty(findText)) return;

        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        string text = _textBox.Text;
        var sb = new StringBuilder();
        int index = 0;
        int count = 0;

        while (true)
        {
            int found = text.IndexOf(findText, index, comparison);
            if (found < 0)
            {
                sb.Append(text, index, text.Length - index);
                break;
            }
            sb.Append(text, index, found - index);
            sb.Append(replaceText);
            index = found + findText.Length;
            count++;
        }

        if (count > 0)
        {
            _textBox.Text = sb.ToString();
        }

        MessageBox.Show(this, count == 0 ? $"Cannot find \"{findText}\"" : $"Replaced {count} occurrence(s).",
            "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ---------- Format / View ----------

    private void ToggleWordWrap()
    {
        _textBox.WordWrap = _wordWrapItem.Checked;
        _textBox.ScrollBars = _wordWrapItem.Checked ? ScrollBars.Vertical : ScrollBars.Both;
        UpdatePositionLabel();
    }

    private void ChooseFont()
    {
        using var fontDialog = new FontDialog { Font = _baseFont };
        if (fontDialog.ShowDialog(this) == DialogResult.OK)
        {
            _baseFont = fontDialog.Font;
            ApplyZoomedFont();
        }
    }

    private void TextBox_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (ModifierKeys != Keys.Control) return;

        // Prevent the TextBox from also scrolling the caret/view while we're zooming.
        if (e is HandledMouseEventArgs handledArgs)
        {
            handledArgs.Handled = true;
        }

        ChangeZoom(e.Delta > 0 ? 10 : -10);
    }

    private void ChangeZoom(int delta)
    {
        _zoomPercent = Math.Clamp(_zoomPercent + delta, 10, 500);
        ApplyZoomedFont();
    }

    private void ResetZoom()
    {
        _zoomPercent = 100;
        ApplyZoomedFont();
    }

    private void ApplyZoomedFont()
    {
        float size = _baseFont.Size * (_zoomPercent / 100f);
        _textBox.Font = new Font(_baseFont.FontFamily, Math.Max(1f, size), _baseFont.Style);
    }

    private void ToggleStatusBar()
    {
        _statusStrip.Visible = _statusBarItem.Checked;
        UpdatePositionLabel();
    }

    private void UpdatePositionLabel()
    {
        if (!_statusStrip.Visible) return;
        int index = _textBox.SelectionStart;
        int line = _textBox.GetLineFromCharIndex(index);
        int col = index - _textBox.GetFirstCharIndexFromLine(line);
        _positionLabel.Text = $"Ln {line + 1}, Col {col + 1}";
    }

    private static void ShowAbout()
    {
        MessageBox.Show(
            "Notepad\nA little recreation of Notepad for fun.",
            "About Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ---------- Helpers ----------

    private void MarkDirty()
    {
        _isDirty = true;
        UpdateTitle();
        UpdatePositionLabel();
    }

    private void UpdateTitle()
    {
        string name = _currentFilePath is null ? "Untitled" : Path.GetFileName(_currentFilePath);
        Text = _isDirty ? $"*{name} - Notepad" : $"{name} - Notepad";
    }
}
