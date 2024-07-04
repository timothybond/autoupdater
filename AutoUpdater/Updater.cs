using System.IO.Compression;

namespace AutoUpdater
{
    /// <summary>
    /// Main class for performing an update to an application.
    ///
    /// The update logic should work for any arbitrary application, with the following caveats:
    /// - The application and all its dependencies must be in a single directory
    /// - The application directory must contain a file "current.json" describing the application,
    ///   serializable to the <see cref="VersionInfo"/> type.
    /// - The filename "latest.json" will be used during updates, and should not exist otherwise.
    /// - A subdirectory, "prior", is reserved for backing up files during an update.
    /// </summary>
    public class Updater
    {
        const string LatestJson = "latest.json";
        const string CurrentJson = "current.json";
        const string BackupDirectory = "prior";

        public Updater(
            IRepository repository,
            IVersionProvider versionProvider,
            ILauncher launcher,
            ILogger logger
        )
        {
            Repository = repository;
            Launcher = launcher;
            VersionProvider = versionProvider;
            Logger = logger;
        }

        public IRepository Repository { get; }
        public ILauncher Launcher { get; }
        public IVersionProvider VersionProvider { get; }
        public ILogger Logger { get; }

        /// <summary>
        /// Checks if the latest version is newer than the current version and, if so, performs an update and re-launches the program.
        ///
        /// Note it is the caller's responsibility to avoid performing any other tasks after an update (as indicated by the return value).
        /// </summary>
        /// <returns><c>true</c> if an update was performed, <c>false</c> if not.</returns>
        public async Task<bool> UpdateAsync()
        {
            Logger.Log("Checking for updates...");

            VersionInfo latestVersionInfo;

            try
            {
                latestVersionInfo = await Repository.GetLatestVersionInfoAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(
                    $"Failed to determine if an update was available. (Make sure you're connected to the Internet, or skip the update check.)\n{ex}"
                );
                return false;
            }

            if (!latestVersionInfo.IsNewerThan(VersionProvider.GetCurrentVersion()))
            {
                Logger.Log("No updates available.");
                return false;
            }

            Logger.Log("Performing update...");

            var dir = Directory.GetCurrentDirectory();
            var currentVersionInfoPath = Path.Combine(dir, CurrentJson);
            var currentVersionInfo = await Json.ReadFromFileAsync<VersionInfo>(
                currentVersionInfoPath
            );
            var filesToUpdate = GetFilesToUpdate(currentVersionInfo, latestVersionInfo);

            var packagePath = Path.Combine(dir, Path.GetFileName(latestVersionInfo.Url.LocalPath));

            var latestVersionInfoPath = Path.Combine(dir, LatestJson);
            await Json.WriteToFileAsync(latestVersionInfoPath, latestVersionInfo);

            await DownloadNewPackage(latestVersionInfo, packagePath);
            Logger.Log($"Downloaded update package ({packagePath}).");

            CreatePriorVersionBackup(currentVersionInfo, latestVersionInfo, dir, filesToUpdate);
            Logger.Log($"Prior version backup complete. Extracting new version...");

            ExtractNewVersion(dir, packagePath, filesToUpdate);

            ReplaceCurrentVersionJson(currentVersionInfoPath, latestVersionInfoPath);

            DeletePackage(packagePath);

            Logger.Log("Update complete. Launching new version...");

            await Launcher.Launch();
            return true;
        }

        /// <summary>
        /// Replaces "current.json" with "latest.json". Should be invoked  after the update is complete and "latest.json" now represents the current version.
        /// </summary>
        private void ReplaceCurrentVersionJson(
            string currentVersionInfoPath,
            string latestVersionInfoPath
        )
        {
            try
            {
                File.Copy(latestVersionInfoPath, currentVersionInfoPath, true);
            }
            catch (Exception)
            {
                Logger.Log(
                    "Failed to replace current.json. In order to prevent issues on subsequent updates, you need to manually copy the contents of latest.json to current.json, and then delete latest.json."
                );
                return;
            }

            try
            {
                File.Delete(latestVersionInfoPath);
            }
            catch (Exception)
            {
                Logger.Log(
                    "Failed to delete latest.json. You can delete it manually, although it will be replaced on the next update if not."
                );
            }
        }

        /// <summary>
        /// Extracts the specified files from the package into the destination directory.
        /// </summary>
        private void ExtractNewVersion(
            string destination,
            string packagePath,
            IEnumerable<string> filesToUpdate
        )
        {
            try
            {
                using var archive = ZipFile.OpenRead(packagePath);

                foreach (var fileToUpdate in filesToUpdate)
                {
                    var filePath = Path.Combine(destination, fileToUpdate);
                    var entry = archive.GetEntry(fileToUpdate);

                    if (entry == null)
                    {
                        throw new Exception(
                            $"Expected file '{fileToUpdate}' not found in package."
                        );
                    }

                    entry.ExtractToFile(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to extract new version. You may need to restore the prior version (from the 'prior' folder).",
                    ex
                );
            }
        }

        /// <summary>
        /// Deletes the downloaded update package. Should only be invoked after the update is complete.
        /// </summary>
        /// <param name="packagePath"></param>
        private void DeletePackage(string packagePath)
        {
            try
            {
                File.Delete(packagePath);
            }
            catch (Exception)
            {
                Logger.Log(
                    $"Failed to clean up package {packagePath}. It is safe to manually delete it at this point."
                );
            }
        }

        /// <summary>
        /// Downloads the latest update package to the given location.
        /// </summary>
        private async Task DownloadNewPackage(VersionInfo latestVersionInfo, string localPath)
        {
            try
            {
                var client = new HttpRepository();
                await client.GetPackageAsync(latestVersionInfo.Url, localPath);

                var packageHash = SHA.GetFileHash(localPath);

                if (!packageHash.Equals(latestVersionInfo.Hash))
                {
                    throw new Exception(
                        $"Package was downloaded from {latestVersionInfo.Url} but package hash did not match the expected value."
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download valid update package.", ex);
            }
        }

        /// <summary>
        /// Creates a backup of the current package, in the <see cref="BackupDirectory"/> subdirectory.
        /// </summary>
        /// <param name="currentVersionInfo">VersionInfo for the current package.</param>
        /// <param name="latestVersionInfo">VersionInfo for the latest/new package.</param>
        /// <param name="currentDir">Current directory.</param>
        /// <param name="filesToUpdate">Files which we plan on updating, based on their non-matching hash values.</param>
        private void CreatePriorVersionBackup(
            VersionInfo currentVersionInfo,
            VersionInfo latestVersionInfo,
            string currentDir,
            HashSet<string> filesToUpdate
        )
        {
            try
            {
                var backupDir = Path.Combine(currentDir, BackupDirectory);
                Logger.Log($"Backing up current version at {backupDir}...");

                var allLatestVersionFiles = latestVersionInfo.Files.Select(f => f.Name).ToHashSet();

                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, true);
                }

                Directory.CreateDirectory(backupDir);

                foreach (var file in currentVersionInfo.Files)
                {
                    // Move any files to be updated, or any files that are no longer needed in the next version (otherwise they would just sit around forever)
                    if (
                        filesToUpdate.Contains(file.Name)
                        || !allLatestVersionFiles.Contains(file.Name)
                    )
                    {
                        File.Move(
                            Path.Combine(currentDir, file.Name),
                            Path.Combine(backupDir, file.Name)
                        );
                    }
                    else
                    {
                        // For consistency make copies of the non-updated files, so we could always just copy the "prior" directory and it would be valid
                        File.Copy(
                            Path.Combine(currentDir, file.Name),
                            Path.Combine(backupDir, file.Name)
                        );
                    }
                }

                // By the same logic as the non-updated files, we need current.json, which is deliberately excluded from the file listings
                File.Copy(
                    Path.Combine(currentDir, CurrentJson),
                    Path.Combine(backupDir, CurrentJson)
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to backup current version. Aborting update.", ex);
            }
        }

        /// <summary>
        /// Gets a set of all files that need to be updated, based on the fact that they have different hashes in the current and latest <see cref="VersionInfo"> instances.
        /// </summary>
        private HashSet<string> GetFilesToUpdate(
            VersionInfo currentVersionInfo,
            VersionInfo latestVersionInfo
        )
        {
            var currentVersionHashes = currentVersionInfo.Files.ToDictionary(
                f => f.Name,
                f => f.Hash
            );

            return latestVersionInfo
                .Files.Where(f =>
                {
                    if (currentVersionHashes.TryGetValue(f.Name, out var hash))
                    {
                        return !hash.Equals(f.Hash);
                    }

                    return true;
                })
                .Select(f => f.Name)
                .ToHashSet();
        }
    }
}
