namespace NotepadClone;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var form = new MainForm();

        // Support "open with" — a file path passed as a command-line argument.
        if (args.Length > 0 && File.Exists(args[0]))
        {
            form.OpenFileOnStartup(args[0]);
        }

        Application.Run(form);
    }
}
