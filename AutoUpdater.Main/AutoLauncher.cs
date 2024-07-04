namespace AutoUpdater.Main
{
    /// <summary>
    /// Launcher that determines what to do by checking the current OS.
    /// </summary>
    public class AutoLauncher : ILauncher
    {
        public async Task Launch()
        {
            ILauncher launcher;

            if (OperatingSystem.IsWindows())
            {
                launcher = new WindowsLauncher();
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                launcher = new UnixLikeLauncher();
            }
            else
            {
                throw new InvalidOperationException("Unsupported OS.");
            }

            await launcher.Launch();
        }
    }
}
