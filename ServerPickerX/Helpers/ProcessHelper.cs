using Avalonia.Controls;
using ServerPickerX.Views;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ServerPickerX.Helpers
{
    public class ProcessHelper
    {
        public static Process CreateProcess(string filename = "")
        {
            Process process = new();

            process.StartInfo.FileName = filename;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            return process;
        }

        public async static Task OpenUrl(string url)
        {
            var topLevel = TopLevel.GetTopLevel(MainWindow.Instance);

            if (topLevel == null)
            {
                return;
            }

            await topLevel.Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
