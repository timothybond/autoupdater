using System.Diagnostics;

namespace AutoUpdater
{
    /// <summary>
    /// Launcher that works on Windows systems, where the main app is an executable.
    /// </summary>
    public class WindowsLauncher : ILauncher
    {
        public async Task Launch()
        {
            var mainApp = Process.Start("Main.exe", "-n");
            await mainApp.WaitForExitAsync();
        }
    }
}
