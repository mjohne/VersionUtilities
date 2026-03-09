using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionUtilities
{
    public sealed class SemVersion : IComparable<SemVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public IReadOnlyList<string> PreRelease { get; }
        public string BuildMetadata { get; }

        public bool IsPreRelease => PreRelease.Count > 0;

        public SemVersion(int major, int minor, int patch,
                          IEnumerable<string> preRelease = null,
                          string buildMetadata = null)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major), "Major version must be non-negative.");
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), "Minor version must be non-negative.");
            if (patch < 0)
                throw new ArgumentOutOfRangeException(nameof(patch), "Patch version must be non-negative.");

            var preReleaseArray = preRelease?.ToArray() ?? Array.Empty<string>();
            if (preReleaseArray.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Pre-release identifiers must be non-empty.", nameof(preRelease));

            if (!string.IsNullOrEmpty(buildMetadata))
            {
                var buildParts = buildMetadata.Split('.');
                if (buildParts.Any(string.IsNullOrEmpty))
                    throw new ArgumentException("Build metadata identifiers must be non-empty.", nameof(buildMetadata));
            }

            Major = major;
            Minor = minor;
            Patch = patch;

            PreRelease = preReleaseArray;
            BuildMetadata = buildMetadata;
        }

        public static SemVersion Parse(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version string is empty.", nameof(version));

            version = version.Trim();

            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                version = version.Substring(1);

            string build = null;
            string pre = null;

            int buildIndex = version.IndexOf('+');
            if (buildIndex >= 0)
            {
                build = version[(buildIndex + 1)..];
                version = version[..buildIndex];
            }

            int preIndex = version.IndexOf('-');
            if (preIndex >= 0)
            {
                pre = version[(preIndex + 1)..];
                version = version[..preIndex];
            }

            string[] numbers = version.Split('.');

            if (numbers.Length != 3)
                throw new ArgumentException(
                    "Version core must consist of exactly three dot-separated numeric identifiers (MAJOR.MINOR.PATCH).",
                    nameof(version));

            if (!int.TryParse(numbers[0], out int major) || major < 0)
                throw new ArgumentException("MAJOR version must be a non-negative integer.", nameof(version));

            if (!int.TryParse(numbers[1], out int minor) || minor < 0)
                throw new ArgumentException("MINOR version must be a non-negative integer.", nameof(version));

            if (!int.TryParse(numbers[2], out int patch) || patch < 0)
                throw new ArgumentException("PATCH version must be a non-negative integer.", nameof(version));
            string[] preReleaseParts = pre?.Split('.') ?? Array.Empty<string>();

            return new SemVersion(major, minor, patch, preReleaseParts, build);
        }

        public int CompareTo(SemVersion other)
        {
            if (other == null) return 1;

            int r;

            r = Major.CompareTo(other.Major);
            if (r != 0) return r;

            r = Minor.CompareTo(other.Minor);
            if (r != 0) return r;

            r = Patch.CompareTo(other.Patch);
            if (r != 0) return r;

            if (!IsPreRelease && other.IsPreRelease) return 1;
            if (IsPreRelease && !other.IsPreRelease) return -1;

            int max = Math.Max(PreRelease.Count, other.PreRelease.Count);

            for (int i = 0; i < max; i++)
            {
                if (i >= PreRelease.Count) return -1;
                if (i >= other.PreRelease.Count) return 1;

                string a = PreRelease[i];
                string b = other.PreRelease[i];

                bool aNum = int.TryParse(a, out int ai);
                bool bNum = int.TryParse(b, out int bi);

                if (aNum && bNum)
                {
                    int cmp = ai.CompareTo(bi);
                    if (cmp != 0) return cmp;
                }
                else if (aNum)
                {
                    return -1;
                }
                else if (bNum)
                {
                    return 1;
                }
                else
                {
                    int cmp = string.Compare(a, b, StringComparison.Ordinal);
                    if (cmp != 0) return cmp;
                }
            }

            return 0;
        }

        public override string ToString()
        {
            string core = $"{Major}.{Minor}.{Patch}";

            if (IsPreRelease)
                core += "-" + string.Join(".", PreRelease);

            if (!string.IsNullOrEmpty(BuildMetadata))
                core += "+" + BuildMetadata;

            return core;
        }
    }
}
