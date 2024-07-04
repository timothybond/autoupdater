using Microsoft.Extensions.Configuration;

namespace AutoUpdater
{
    /// <summary>
    /// Utility class for accessing the configuration file.
    /// </summary>
    public static class UpdaterConfiguration
    {
        private const string UpdateUrl = "UpdateUrl";

        static UpdaterConfiguration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false);
            var config = builder.Build();
            var url = config[UpdateUrl];

            Url = url;
        }

        /// <summary>
        /// The base URL for the package repository, where we can check for new versions.
        /// 
        /// Note that versions and version info files are stored in sub-directories, by runtime identifier.
        /// </summary>
        public static string? Url { get; }
    }
}
