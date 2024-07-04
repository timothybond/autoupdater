using System.Text.Json;

namespace AutoUpdater
{
    /// <summary>
    /// Convenience methods for reading/writing JSON files.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Gets an instance of the given type from the file.
        ///
        /// Throws an error if deserialization fails or returns null.
        /// </summary>
        public static async Task<T> ReadFromFileAsync<T>(string path)
        {
            using var file = File.OpenRead(path);
            var result = await JsonSerializer.DeserializeAsync<T>(file);

            if (result == null)
            {
                throw new InvalidOperationException($"Could not deserialize JSON from '{path}'.");
            }

            return result;
        }

        /// <summary>
        /// Writes a JSON file for a given object.
        /// </summary>
        public static async Task WriteToFileAsync<T>(string path, T item)
        {
            using var file = File.Create(path);

            await JsonSerializer.SerializeAsync(file, item);
        }
    }
}
