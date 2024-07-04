using System.CommandLine;
using System.IO.Compression;
using AutoUpdater;

class Program
{
    const string CurrentJson = "current.json";

    static async Task<int> Main(string[] args)
    {
        var version = new Option<string>(
            name: "--package-version",
            description: "Version number for the package."
        )
        {
            IsRequired = true
        };
        version.AddAlias("-p");

        var runtimeIdentifier = new Option<string>(
            name: "--runtime",
            description: "Runtime identifier for the package."
        )
        {
            IsRequired = true
        };
        runtimeIdentifier.AddAlias("-r");

        var directory = new Option<string>(
            name: "--directory",
            description: "Build output directory of AutoUpdater.Main (relative to current directory).\nThis needs the directory that contains ALL of the different runtime versions,\nnot the specific one targeted. (Typically ending in ../bin/Release/net8.0.)"
        )
        {
            IsRequired = true
        };
        directory.AddAlias("-d");

        var command = new RootCommand("Post-Build Steps");
        command.AddOption(version);
        command.AddOption(runtimeIdentifier);
        command.AddOption(directory);

        command.SetHandler(BuildPackage, version, runtimeIdentifier, directory);

        return await command.InvokeAsync(args);
    }

    static async Task BuildPackage(string version, string runtimeIdentifier, string directory)
    {
        try
        {
            Console.WriteLine($"Assembling package for v{version}, {runtimeIdentifier}.");

            var binPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), directory)
            );

            var runtimeArtifactsPath = Path.Combine(binPath, runtimeIdentifier, "publish");

            if (!Directory.Exists(runtimeArtifactsPath))
            {
                throw new InvalidOperationException(
                    $"Expected output directory does not exist: {runtimeArtifactsPath}"
                );
            }

            // Make sure we don't have a generated "current.json" file left over from a previous execution, or it'll get packaged
            var oldCurrentJson = Path.Combine(runtimeArtifactsPath, CurrentJson);
            if (File.Exists(oldCurrentJson))
            {
                File.Delete(oldCurrentJson);
            }

            var filesToPackage = Directory.GetFiles(runtimeArtifactsPath).ToList();

            var fileInfo = filesToPackage.Select(GetFileInfo).ToList();

            var archiveFileName = $"{runtimeIdentifier}_{version}.zip";

            var packageUrl = new Uri(
                $"{UpdaterConfiguration.Url}/{runtimeIdentifier}/{archiveFileName}"
            );

            // For the initial installation to be valid, it needs to have a VersionInfo file ("current.json"), with entries for all of the other files.
            // This gets used on updates to determine which files have changed and which have not.
            // However, this VersionInfo file does NOT need the archive hash (which is good, because it needs to be IN the archive, so that's basically impossible).
            // Really we don't need the URL, either, but there's no reason we can't get it now.
            var versionInfo = new VersionInfo(
                version,
                DateTimeOffset.UtcNow,
                packageUrl,
                string.Empty,
                fileInfo
            );

            var packageDirectoryPath = Path.Join(binPath, "packages", runtimeIdentifier);
            Directory.CreateDirectory(packageDirectoryPath);

            var currentVersionInfoFilePath = Path.Join(runtimeArtifactsPath, CurrentJson);
            await Json.WriteToFileAsync(currentVersionInfoFilePath, versionInfo);

            filesToPackage.Add(currentVersionInfoFilePath);

            var zipFilePath = Path.Join(packageDirectoryPath, archiveFileName);
            CreateZipFile(filesToPackage, zipFilePath);

            Console.WriteLine($"Package written to {zipFilePath}.");

            var archiveHash = SHA.GetFileHash(zipFilePath);
            versionInfo = versionInfo with { Hash = archiveHash };

            var versionInfoFilePath = Path.Join(
                packageDirectoryPath,
                $"{runtimeIdentifier}_{version}.json"
            );
            var latestVersionInfoFilePath = Path.Join(
                packageDirectoryPath,
                $"{runtimeIdentifier}_latest.json"
            );

            await Json.WriteToFileAsync(versionInfoFilePath, versionInfo);
            await Json.WriteToFileAsync(latestVersionInfoFilePath, versionInfo);

            Console.WriteLine($"Version info written to {versionInfoFilePath}.");
            Console.WriteLine(
                $"Version info written to {latestVersionInfoFilePath}. ONLY DEPLOY THIS IF THIS IS ACTUALLY THE LATEST VERSION."
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during package assembly.\n{ex}");
        }
    }

    static void CreateZipFile(IEnumerable<string> files, string destination)
    {
        var destinationDirectory = Path.GetDirectoryName(destination);
        if (destinationDirectory == null)
        {
            throw new InvalidOperationException(
                $"Failed to find parent directory for destination file '{destination}'."
            );
        }

        Directory.CreateDirectory(destinationDirectory);

        if (File.Exists(destination))
        {
            File.Delete(destination);
        }

        using var archive = ZipFile.Open(destination, ZipArchiveMode.Create);

        foreach (var file in files)
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }
    }

    static VersionFileInfo GetFileInfo(string path)
    {
        var fileName = Path.GetFileName(path);
        var hashString = SHA.GetFileHash(path);

        return new VersionFileInfo(fileName, hashString);
    }
}
