namespace NotepadClone;

public class GoToForm : Form
{
    private readonly NumericUpDown _lineNumberInput;

    public int LineNumber => (int)_lineNumberInput.Value;

    public GoToForm(int maxLine)
    {
        Text = "Go To Line";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(260, 110);

        var label = new Label { Text = "Line number (1 - " + Math.Max(1, maxLine) + "):", Left = 12, Top = 15, Width = 220 };
        _lineNumberInput = new NumericUpDown
        {
            Left = 12,
            Top = 40,
            Width = 220,
            Minimum = 1,
            Maximum = Math.Max(1, maxLine),
            Value = 1
        };

        var okButton = new Button { Text = "OK", Left = 60, Top = 75, Width = 80, Height = 26, DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Cancel", Left = 150, Top = 75, Width = 80, Height = 26, DialogResult = DialogResult.Cancel };

        AcceptButton = okButton;
        CancelButton = cancelButton;

        Controls.Add(label);
        Controls.Add(_lineNumberInput);
        Controls.Add(okButton);
        Controls.Add(cancelButton);
    }
}
