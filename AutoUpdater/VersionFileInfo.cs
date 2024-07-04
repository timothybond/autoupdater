namespace AutoUpdater
{
    /// <summary>
    /// Represents a single file inside of a package.
    ///
    /// Used to avoid updating files that haven't changed.
    /// </summary>
    /// <param name="Name">The file name.</param>
    /// <param name="Hash">Hash of the file contents.</param>
    public record VersionFileInfo(string Name, string Hash) { }
}
