using System.Diagnostics;

namespace AutoUpdater
{
    /// <summary>
    /// Launcher that works on Unix-like systems (i.e. OSX and Linux), where the main app is just named "Main" and can be directly invoked.
    /// </summary>
    public class UnixLikeLauncher : ILauncher
    {
        public async Task Launch()
        {
            var mainApp = Process.Start("Main", "-n");
            await mainApp.WaitForExitAsync();
        }
    }
}
