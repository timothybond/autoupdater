namespace AutoUpdater
{
    /// <summary>
    /// The base type for accessing a package repository.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Gets the VersionInfo for the latest version, using the current system architecture.
        /// </summary>
        public Task<VersionInfo> GetLatestVersionInfoAsync();

        /// <summary>
        /// Retrieves a package from the given <see cref="Uri"/>, saving it to the requested local path.
        /// </summary>
        public Task GetPackageAsync(Uri uri, string localPath);
    }
}
