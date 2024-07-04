using System.Reflection;

namespace AutoUpdater.Main
{
    public class AssemblyVersionProvider : IVersionProvider
    {
        public Version GetCurrentVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            if (version == null)
            {
                throw new Exception("Failed to get current assembly version.");
            }

            return version;
        }
    }
}
