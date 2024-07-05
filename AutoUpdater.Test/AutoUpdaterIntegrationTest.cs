using System.Diagnostics;

namespace AutoUpdater.Test
{
    public class AutoUpdaterIntegrationTest
    {
        [SetUp]
        public void CopyCurrentBuild()
        {
            // This is admittedly a bit clunky - we need to get a fresh copy of this, otherwise running the test once will invalidate it.
            var currentDirectory = Directory.GetCurrentDirectory();
            var mainBuildDirectory = currentDirectory.Replace(
                "AutoUpdater.Test",
                "AutoUpdater.Main"
            );

            var files = Directory.GetFiles(mainBuildDirectory);

            foreach (var file in files)
            {
                File.Copy(file, Path.Combine(currentDirectory, Path.GetFileName(file)), true);
            }
        }

        [TearDown]
        public void DeleteBackupVersion()
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "prior");
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }
        }

        [Test]
        public async Task PerformUpdateAndCheckVersion()
        {
            // Note this test currently depends on external state, namely, that the hosted repository has a latest version of 1.0.2.0.
            // In the long-term we should create some kind of container-based solution so we can set up the repository artifacts as well.
            // Also it's clunky that we have to assume the current version number as well, but I'd rather make sure I check that it's different first.
            const string CurrentVersion = "1.0.0.0";
            const string ExpectedVersion = "1.0.2.0";

            Assert.That(GetVersionNumber(), Is.EqualTo(CurrentVersion));

            var process = Process.Start("Main.exe");
            await process.WaitForExitAsync();

            Assert.That(GetVersionNumber(), Is.EqualTo(ExpectedVersion));
        }

        private string GetVersionNumber()
        {
            var mainDll = Path.Combine(Directory.GetCurrentDirectory(), "Main.dll");
            return FileVersionInfo.GetVersionInfo(mainDll).FileVersion ?? string.Empty;
        }
    }
}
