using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServerPickerX.Helpers
{
    public class FileHelper
    {
        public static async Task LogErrorToFile(string exception, string sourceOfError, string? fileName = "")
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + 
                (String.IsNullOrEmpty(fileName) ? DateTimeOffset.Now.ToUnixTimeSeconds().ToString() : fileName) + ".txt";

            await File.AppendAllTextAsync(path, sourceOfError + Environment.NewLine + exception);

            if (OperatingSystem.IsLinux())
            {
                await ChangeLinuxFileOwner(path);
            }
        }

        public static async Task ChangeLinuxFileOwner(string path)
        {
            // change the owner of the log file to current user since it defaults to 
            // root user and prevents the current user to manipulate the log file
            try
            {
                var user = Environment.GetEnvironmentVariable("SUDO_USER");

                using Process process = ProcessHelper.CreateProcess("sudo");

                process.StartInfo.Arguments = $"chown {user}:{user} " + path;

                process.Start();

                await process.WaitForExitAsync();
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", "An error has occured while changing file owner for: " + path);
            }
        }
    }
}
