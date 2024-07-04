# AutoUpdater
A self-updating program. (Originally a programming test for a prospective employer).

# Structure

## AutoUpdater

Base library with various shared components for performing updates. The main worker class here is Updater, which has logic for performing the update of an arbitrary application, with some caveats:
- The application must be in a single directory
- The application must have a "current.json" file describing itself, serializable to the VersionInfo type
- The subdirectory "prior" and the file "latest.json" are reserved for use of the update process

Updates are performed in-place, by moving any to-be-modified files to the "prior" directory and then extracting the updated ones. This avoids the prohibition (in Windows at least, not sure if it would apply in Linux/OSX) against modifying in-use files.

Files that have not changed (based on their hash) are not replaced, although they are copied to the "prior" directory, on the assumption that it's more convenient, if something goes wrong, to just have a full, working version of the application in the "prior" directory.

## Main

AutoUpdater.Main builds an executable, just called "Main" (or "Main.exe" on Windows).

When executed, it will invoke an Updater. If there is a new version available, it will be updated, and then the new version will automatically launch.

If there is no new version available, it just prints its current version number (helpful for validating the updates).

The main executable also can take a single argument, "-n" or "--noupdate", to skip the update check.

## PostBuild and Creating Builds

AutoUpdater.PostBuild has logic to create packages, including authoring the essential "current.json" file.

There's a .bat file in AutoUpdater.Main that rebuilds the solution, publishes packages for each architecture, and runs AutoUpdater.PostBuild against them. When this is done, there will be a "packages" directory in AutoUpdater.Main\bin\Release\net8.0 that contains everything that needs to be uploaded to the repository.

## Hosted Packages

For testing/convenience I have initially hosted such a repository on my personal website (already specified in appsettings.json). The latest package version there is 1.0.2.0. Building and running any version earlier than that will trigger the update process.

Alternately, you can download and extract one, e.g.:

	https://eclecticdevlog.com/autoupdater/win-x64/win-x64_1.0.0.0.zip
	https://eclecticdevlog.com/autoupdater/linux-x64/linux-x64_1.0.0.0.zip

# Assumptions

This solution was build with the following assumptions:
- End-users will initially install the application manually by extracting a package to a folder that they have write-access to, and thus will not need additional permissions to perform updates
- End-users will already have an appropriate version of the .NET runtime installed (so we don't have to package it, and thus have much larger updates)

# Misc Notes

## Manual vs. Automated Testing

Normally I would never want to put together something like this (or, y'know, any code whatsoever) without automated tests. However, because this consists almost entirely of infrastructure stuff, and almost no self-contained logic, it would be a lot of additional work to put together tests for it, so I've relied on manual testing instead.

If I were supporting this long-term, I would definitely want to put something together to ensure that it works and test some complex update scenarios (e.g., files not being used anymore, running a series of updates in a row), using Docker containers to ensure a clean environment each time. Also, several parts of the logic are already written against interfaces to make it easier to substitute alternate implementations for testing.

## OS Compatibility

I tested this manually on Windows, which is what my dev workstation is. I also tested the Linux build using WSL, which I assume is representative enough. I don't have a full Linux machine handy at the moment, nor do I have an OSX machine.

## Individual File Versions

When updating, the code only replaces versions that have non-matching hashes. This could hypothetically run into an issue (albeit an extremely rare one) where we encounter a hash collision. I didn't solve for this because of the extremely low likelihood, and also because any alternate solution (using version numbers / file authoring times / etc.) would probably be selected based on the broader context of the project, but this demonstration project has no broader context.

## Update Loops

The "-n" flag is used when re-launching the main executable after an update, because if two updates were performed in a row, it would fail (in Windows at least).

This is because the files in the "prior" directory would still be in use - technically, the original Main executable is still running after the update, waiting for the new Main executable to finish.

I could avoid this by letting the original Main executable stop as soon as it launches the new version. The main reason I did not do this is because it causes slightly weird-looking console output, where the completion of the first process makes the console think it's "done", so it prints the directory again, but then continues printing the output of the second process, and then when the second process finishes, it *doesn't* print the directory, which can make it seem like the application is hanging.

In a more practical scenario, when the main application was something other than a borderline-Hello-World utility, this probably wouldn't matter.