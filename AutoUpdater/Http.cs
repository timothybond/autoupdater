namespace AutoUpdater
{
    /// <summary>
    /// Utility class to ensure we only create one <see cref="HttpClient">.
    /// </summary>
    public static class Http
    {
        // Note: per MSDN, you should only ever create one HttpClient instance and reuse it.
        // In some more advanced scenarios you can use IHttpClientFactory instead,
        // but I felt that was overkill for this demonstration project.
        public static readonly HttpClient Client = new HttpClient();
    }
}
