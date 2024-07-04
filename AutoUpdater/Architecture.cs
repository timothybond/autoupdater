using System.Runtime.InteropServices;

namespace AutoUpdater
{
    /// <summary>
    /// Helper class to determine the current system OS/architecture.
    /// </summary>
    public static class Architecture
    {
        /// <summary>
        /// Gets the Runtime Identifier of the system we're running on. See <see href="https://learn.microsoft.com/en-us/dotnet/core/rid-catalog">RID Catalog</see>.
        ///
        /// Throws an exception if we're somehow running on an unsupported OS or processor architecture.
        /// </summary>
        public static string GetRuntimeIdentifier()
        {
            var processArch = RuntimeInformation.ProcessArchitecture;

            if (OperatingSystem.IsWindows())
            {
                return processArch switch
                {
                    System.Runtime.InteropServices.Architecture.X86 => "win-x86",
                    System.Runtime.InteropServices.Architecture.X64 => "win-x64",
                    System.Runtime.InteropServices.Architecture.Arm64 => "win-arm64",
                    _ => throw new InvalidOperationException("Unsupported processor architecture."),
                };
            }
            else if (OperatingSystem.IsLinux())
            {
                return processArch switch
                {
                    System.Runtime.InteropServices.Architecture.X64 => "linux-x64",
                    System.Runtime.InteropServices.Architecture.Arm => "linux-arm",
                    System.Runtime.InteropServices.Architecture.Arm64 => "linux-arm64",
                    _ => throw new InvalidOperationException("Unsupported processor architecture."),
                };
            }
            else if (OperatingSystem.IsMacOS())
            {
                return processArch switch
                {
                    System.Runtime.InteropServices.Architecture.X64 => "osx-x64",
                    System.Runtime.InteropServices.Architecture.Arm64 => "osx-arm64",
                    _ => throw new InvalidOperationException("Unsupported processor architecture."),
                };
            }
            else
            {
                throw new InvalidOperationException("Unsupported OS.");
            }
        }
    }
}
