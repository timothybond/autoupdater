using AutoUpdater;
using AutoUpdater.Main;
using System.CommandLine;

class Program
{
    const string LatestJson = "latest.json";
    const string CurrentJson = "current.json";
    const string BackupDirectory = "prior";

    static async Task<int> Main(string[] args)
    {
        var skipUpdateOption = new Option<bool>(
            name: "--noupdate",
            description: "If specified, skips the update check."
        );
        skipUpdateOption.AddAlias("-n");

        var command = new RootCommand("Main Application");
        command.AddOption(skipUpdateOption);

        command.SetHandler(UpdateAndRunProgram, skipUpdateOption);

        return await command.InvokeAsync(args);
    }

    static async Task UpdateAndRunProgram(bool skipUpdate)
    {
        if (skipUpdate)
        {
            Console.WriteLine("Skipping update check.");
        }
        else
        {
            var updater = new Updater(
                new HttpRepository(),
                new AssemblyVersionProvider(),
                new AutoLauncher(),
                new ConsoleLogger()
            );

            try
            {
                var updated = await updater.UpdateAsync();

                if (updated)
                {
                    // Note: the new version will have been launched by Updater, when the update finished
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to perform update.\n{ex}");
                return;
            }

        }

        MainProgram();
    }

    /// <summary>
    /// This is a placeholder for "the actual program". Although in this case there isn't one, so it mostly just establishes that we're running the expected version.
    /// </summary>
    static void MainProgram()
    {
        var versionProvider = new AssemblyVersionProvider();
        var version = versionProvider.GetCurrentVersion();

        Console.WriteLine($"Executing Main v. {version}.");

        Console.WriteLine($"Done.");
    }

    //private static async Task UpdateToLatestVersion(VersionInfo latestVersionInfo)
    //{
    //    Console.WriteLine("Performing update...");

    //    var dir = Directory.GetCurrentDirectory();
    //    var currentVersionInfoPath = Path.Combine(dir, CurrentJson);
    //    var currentVersionInfo = await Json.ReadFromFileAsync<VersionInfo>(currentVersionInfoPath);

    //    var packagePath = Path.Combine(dir, Path.GetFileName(latestVersionInfo.Url.LocalPath));

    //    await Json.WriteToFileAsync(Path.Combine(dir, LatestJson), latestVersionInfo);

    //    try
    //    {
    //        await DownloadNewPackage(latestVersionInfo, packagePath);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Failed to download valid update package.\n{ex}");
    //        return;
    //    }

    //    var backupDir = Path.Combine(dir, BackupDirectory);
    //    Console.WriteLine(
    //        $"Downloaded update package ({packagePath}).\nBacking up current version at {backupDir}..."
    //    );

    //    var filesToUpdate = GetFilesToUpdate(currentVersionInfo, latestVersionInfo);

    //    try
    //    {
    //        CreatePriorVersionBackup(
    //            currentVersionInfo,
    //            latestVersionInfo,
    //            dir,
    //            backupDir,
    //            filesToUpdate
    //        );
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Failed to backup current version. Aborting update.\n{ex}");
    //        return;
    //    }

    //    Console.WriteLine($"Prior version backup complete. Extracting new version...");

    //    try
    //    {
    //        using (var archive = ZipFile.OpenRead(packagePath))
    //        {
    //            foreach (var fileToUpdate in filesToUpdate)
    //            {
    //                var filePath = Path.Combine(dir, fileToUpdate);
    //                if (File.Exists(filePath))
    //                {
    //                    File.Delete(filePath);
    //                }

    //                var entry = archive.GetEntry(fileToUpdate);

    //                if (entry == null)
    //                {
    //                    throw new Exception(
    //                        $"Expected file '{fileToUpdate}' not found in package."
    //                    );
    //                }

    //                entry.ExtractToFile(filePath);
    //            }
    //        }

    //        await Json.WriteToFileAsync(currentVersionInfoPath, latestVersionInfo);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(
    //            $"Failed to extract new version. You may need to restore the prior version from {backupDir}\n{ex}"
    //        );
    //    }

    //    try
    //    {
    //        File.Delete(packagePath);
    //    }
    //    catch (Exception)
    //    {
    //        // This actually isn't really that important, since the only consequence is that the .zip file stays there.
    //    }

    //    var currentProcessId = Process.GetCurrentProcess().Id;

    //    var architecture = SystemArchitecture.GetCurrent();

    //    Console.WriteLine("Update complete. Launching new version...");

    //    var process = Process.Start(architecture.RunMainApp);

    //    if (process == null)
    //    {
    //        Console.WriteLine("Failed to re-launch application after update.");
    //        return;
    //    }

    //    await process.WaitForExitAsync();
    //}

    ///// <summary>
    ///// Downloads the
    ///// </summary>
    ///// <param name="latestVersionInfo"></param>
    ///// <param name="localPath"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>
    //private static async Task DownloadNewPackage(VersionInfo latestVersionInfo, string localPath)
    //{
    //    var client = new HttpRepository();
    //    await client.GetPackageAsync(latestVersionInfo.Url, localPath);

    //    var packageHash = SHA.GetFileHash(localPath);

    //    if (!packageHash.Equals(latestVersionInfo.Hash))
    //    {
    //        throw new Exception(
    //            $"Package was downloaded from {latestVersionInfo.Url} but package hash did not match the expected value."
    //        );
    //    }
    //}

    ///// <summary>
    ///// Creates a backup of the current package, in the <see cref="BackupDirectory"/> subdirectory.
    ///// </summary>
    ///// <param name="currentVersionInfo">VersionInfo for the current package.</param>
    ///// <param name="latestVersionInfo">VersionInfo for the latest/new package.</param>
    ///// <param name="currentDir">Current directory.</param>
    ///// <param name="backupDir">Backup directory.</param>
    ///// <param name="filesToUpdate">Files which we plan on updating, based on their non-matching hash values.</param>
    //private static void CreatePriorVersionBackup(
    //    VersionInfo currentVersionInfo,
    //    VersionInfo latestVersionInfo,
    //    string currentDir,
    //    string backupDir,
    //    HashSet<string> filesToUpdate
    //)
    //{
    //    if (Directory.Exists(backupDir))
    //    {
    //        Directory.Delete(backupDir, true);
    //    }

    //    Directory.CreateDirectory(backupDir);

    //    foreach (var file in currentVersionInfo.Files)
    //    {
    //        if (filesToUpdate.Contains(file.Name))
    //        {
    //            File.Move(Path.Combine(currentDir, file.Name), Path.Combine(backupDir, file.Name));
    //        }
    //        else
    //        {
    //            // For consistency make copies of the non-updated files, so we could always just copy the "prior" directory and it would be valid
    //            File.Copy(Path.Combine(currentDir, file.Name), Path.Combine(backupDir, file.Name));
    //        }
    //    }

    //    // By the same token we need current.json, which is deliberately excluded from the file listings
    //    File.Copy(Path.Combine(currentDir, CurrentJson), Path.Combine(backupDir, CurrentJson));
    //}

    //private static HashSet<string> GetFilesToUpdate(
    //    VersionInfo currentVersionInfo,
    //    VersionInfo latestVersionInfo
    //)
    //{
    //    var currentVersionHashes = currentVersionInfo.Files.ToDictionary(f => f.Name, f => f.Hash);

    //    return latestVersionInfo
    //        .Files.Where(f =>
    //        {
    //            if (currentVersionHashes.TryGetValue(f.Name, out var hash))
    //            {
    //                return !hash.Equals(f.Hash);
    //            }

    //            return true;
    //        })
    //        .Select(f => f.Name)
    //        .ToHashSet();
    //}

    //private static Version GetCurrentVersion()
    //{
    //    var assembly = Assembly.GetExecutingAssembly();
    //    var version = assembly.GetName().Version;

    //    if (version == null)
    //    {
    //        throw new InvalidOperationException("Failed to get current assembly version.");
    //    }

    //    return version;
    //}

    //private static bool LatestVersionIsNewer(VersionInfo latestVersionInfo)
    //{
    //    return latestVersionInfo.IsNewerThan(GetCurrentVersion());
    //}
}
