namespace AutoUpdater
{
    /// <summary>
    /// Base type used to re-launch the main application after an update.
    /// </summary>
    public interface ILauncher
    {
        /// <summary>
        /// Launches the main application, and returns a Task waiting for it to exit.
        /// </summary>
        Task Launch();
    }
}
