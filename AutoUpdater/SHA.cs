using System.Security.Cryptography;

namespace AutoUpdater
{
    /// <summary>
    /// Utility class for getting file hashes.
    /// </summary>
    public static class SHA
    {
        private static readonly SHA256 sha256;

        static SHA()
        {
            sha256 = SHA256.Create();
        }

        public static string GetFileHash(string path)
        {
            using var file = File.Open(path, FileMode.Open, FileAccess.Read);
            file.Seek(0, SeekOrigin.Begin);

            var hash = sha256.ComputeHash(file);

            return Convert.ToBase64String(hash);
        }
    }
}
