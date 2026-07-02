namespace NotepadClone;

public class FindForm : Form
{
    private readonly MainForm _owner;
    private readonly TextBox _findTextBox;
    private readonly CheckBox _matchCaseBox;
    private readonly RadioButton _directionUp;
    private readonly RadioButton _directionDown;

    public FindForm(MainForm owner, string initialTerm, bool matchCase)
    {
        _owner = owner;

        Text = "Find";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 160);

        var findLabel = new Label { Text = "Find what:", Left = 12, Top = 18, Width = 70 };
        _findTextBox = new TextBox { Left = 90, Top = 15, Width = 250, Text = initialTerm };

        var directionGroup = new GroupBox { Text = "Direction", Left = 12, Top = 50, Width = 150, Height = 50 };
        _directionUp = new RadioButton { Text = "Up", Left = 10, Top = 20, Width = 60 };
        _directionDown = new RadioButton { Text = "Down", Left = 75, Top = 20, Width = 75, Checked = true };
        directionGroup.Controls.Add(_directionUp);
        directionGroup.Controls.Add(_directionDown);

        _matchCaseBox = new CheckBox { Text = "Match case", Left = 12, Top = 112, Width = 150, Checked = matchCase };

        var findNextButton = new Button { Text = "Find Next", Left = 250, Top = 50, Width = 90, Height = 26 };
        findNextButton.Click += (_, _) => DoFindNext();

        var cancelButton = new Button { Text = "Cancel", Left = 250, Top = 84, Width = 90, Height = 26 };
        cancelButton.Click += (_, _) => Close();

        AcceptButton = findNextButton;
        CancelButton = cancelButton;

        Controls.Add(findLabel);
        Controls.Add(_findTextBox);
        Controls.Add(directionGroup);
        Controls.Add(_matchCaseBox);
        Controls.Add(findNextButton);
        Controls.Add(cancelButton);
    }

    private void DoFindNext()
    {
        if (string.IsNullOrEmpty(_findTextBox.Text)) return;
        _owner.FindNext(_directionDown.Checked, _findTextBox.Text, _matchCaseBox.Checked);
    }
}
