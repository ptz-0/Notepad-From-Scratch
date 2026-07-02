namespace NotepadClone;

public class ReplaceForm : Form
{
    private readonly MainForm _owner;
    private readonly TextBox _findTextBox;
    private readonly TextBox _replaceTextBox;
    private readonly CheckBox _matchCaseBox;

    public ReplaceForm(MainForm owner, string initialTerm, bool matchCase)
    {
        _owner = owner;

        Text = "Replace";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(410, 160);

        var findLabel = new Label { Text = "Find what:", Left = 12, Top = 18, Width = 90 };
        _findTextBox = new TextBox { Left = 110, Top = 15, Width = 190, Text = initialTerm };

        var replaceLabel = new Label { Text = "Replace with:", Left = 12, Top = 48, Width = 90 };
        _replaceTextBox = new TextBox { Left = 110, Top = 45, Width = 190, Text = string.Empty };

        _matchCaseBox = new CheckBox { Text = "Match case", Left = 12, Top = 80, Width = 150, Checked = matchCase };

        var findNextButton = new Button { Text = "Find Next", Left = 310, Top = 14, Width = 90, Height = 26 };
        findNextButton.Click += (_, _) => _owner.FindNext(true, _findTextBox.Text, _matchCaseBox.Checked);

        var replaceButton = new Button { Text = "Replace", Left = 310, Top = 46, Width = 90, Height = 26 };
        replaceButton.Click += (_, _) => DoReplace();

        var replaceAllButton = new Button { Text = "Replace All", Left = 310, Top = 78, Width = 90, Height = 26 };
        replaceAllButton.Click += (_, _) => DoReplaceAll();

        var cancelButton = new Button { Text = "Cancel", Left = 310, Top = 110, Width = 90, Height = 26 };
        cancelButton.Click += (_, _) => Close();

        AcceptButton = findNextButton;
        CancelButton = cancelButton;

        Controls.Add(findLabel);
        Controls.Add(_findTextBox);
        Controls.Add(replaceLabel);
        Controls.Add(_replaceTextBox);
        Controls.Add(_matchCaseBox);
        Controls.Add(findNextButton);
        Controls.Add(replaceButton);
        Controls.Add(replaceAllButton);
        Controls.Add(cancelButton);
    }

    private void DoReplace()
    {
        if (string.IsNullOrEmpty(_findTextBox.Text)) return;
        _owner.ReplaceCurrent(_findTextBox.Text, _replaceTextBox.Text, _matchCaseBox.Checked);
    }

    private void DoReplaceAll()
    {
        if (string.IsNullOrEmpty(_findTextBox.Text)) return;
        _owner.ReplaceAll(_findTextBox.Text, _replaceTextBox.Text, _matchCaseBox.Checked);
    }
}
