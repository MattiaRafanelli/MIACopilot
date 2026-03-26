using System;
using System.Windows.Forms;

namespace MIACopilot;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(/* services… */));
    }
}
