using System;
using System.Reflection;

namespace VersionUtilities
{
    public static class ApplicationVersion
    {
        /// <summary>
        /// Gets the AssemblyVersion.
        /// </summary>
        public static Version GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        /// <summary>
        /// Gets the AssemblyFileVersion.
        /// </summary>
        public static Version GetFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

            if (attr != null)
                return new Version(attr.Version);

            return GetAssemblyVersion();
        }

        /// <summary>
        /// Gets the informational version (often SemVer).
        /// </summary>
        public static string GetInformationalVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            return attr?.InformationalVersion;
        }

        /// <summary>
        /// Returns the current version as SemVersion.
        /// </summary>
        public static SemVersion GetSemanticVersion()
        {
            string info = GetInformationalVersion();

            if (!string.IsNullOrWhiteSpace(info))
                return SemVersion.Parse(info);

            var assemblyVersion = GetAssemblyVersion();
            int major = assemblyVersion.Major;
            int minor = assemblyVersion.Minor < 0 ? 0 : assemblyVersion.Minor;
            int patch = assemblyVersion.Build < 0 ? 0 : assemblyVersion.Build;
            string buildMetadata = assemblyVersion.Revision < 0 ? null : assemblyVersion.Revision.ToString();

            return new SemVersion(major, minor, patch, null, buildMetadata);
        }

        /// <summary>
        /// Checks if the running application is newer than the specified version.
        /// </summary>
        public static bool IsNewerThan(string version)
        {
            var current = GetSemanticVersion();
            var other = SemVersion.Parse(version);

            return current.CompareTo(other) > 0;
        }

        /// <summary>
        /// Checks if the running application satisfies a version range.
        /// </summary>
        public static bool IsInRange(string range)
        {
            var current = GetSemanticVersion();
            var r = AdvancedVersionRange.Parse(range);

            return r.IsSatisfied(current);
        }
    }
}
