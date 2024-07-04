using System.Net.Http.Json;

namespace AutoUpdater
{
    /// <summary>
    /// A client to access an HTTP-hosted repository.
    /// </summary>
    public class HttpRepository : IRepository
    {
        const string HttpsScheme = "https";

        /// <summary>
        /// Gets the VersionInfo for the latest version, using the current system architecture.
        /// </summary>
        public async Task<VersionInfo> GetLatestVersionInfoAsync()
        {
            var runtimeIdentifier = Architecture.GetRuntimeIdentifier();

            var latestVersionInfoUrl = new Uri(
                $"{UpdaterConfiguration.Url}/{runtimeIdentifier}/{runtimeIdentifier}_latest.json"
            );

            if (!latestVersionInfoUrl.Scheme.Equals(HttpsScheme))
            {
                throw new Exception(
                    $"Invalid URL for repository ({UpdaterConfiguration.Url}). Only https is allowed."
                );
            }

            var response = await Http.Client.GetAsync(latestVersionInfoUrl);
            response.EnsureSuccessStatusCode();

            var versionInfo = await response.Content.ReadFromJsonAsync<VersionInfo>();

            if (versionInfo == null)
            {
                throw new Exception("Failed to read version info from retrieved JSON.");
            }

            return versionInfo;
        }

        public async Task GetPackageAsync(Uri uri, string localPath)
        {
            try
            {
                if (!uri.Scheme.Equals(HttpsScheme))
                {
                    throw new Exception(
                        $"Invalid URL for package ({UpdaterConfiguration.Url}). Only https is allowed."
                    );
                }

                var response = await Http.Client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using var destinationFile = File.Create(localPath);

                await response.Content.CopyToAsync(destinationFile);
                destinationFile.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download package from {uri}.", ex);
            }
        }
    }
}
