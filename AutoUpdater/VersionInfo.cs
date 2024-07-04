namespace AutoUpdater
{
    /// <summary>
    /// Details on a given version of the main application.
    /// </summary>
    /// <param name="Version">The version number.</param>
    /// <param name="Published">The timestamp when the version was published.</param>
    /// <param name="Url">The location of the .zip archive of the package.</param>
    /// <param name="Hash">The hash of the .zip archive of the package.</param>
    /// <param name="Files">A list of all files in the package.</param>
    public record VersionInfo(
        string Version,
        DateTimeOffset Published,
        Uri Url,
        string Hash,
        IReadOnlyList<VersionFileInfo> Files
    )
    {
        public bool IsNewerThan(VersionInfo other) => IsNewerThan(other.Version);

        public bool IsNewerThan(string otherVersion) =>
            IsNewerThan(System.Version.Parse(otherVersion));

        public bool IsNewerThan(Version otherVersion)
        {
            var thisVersion = System.Version.Parse(Version);

            return thisVersion.CompareTo(otherVersion) > 0;
        }
    }
}
