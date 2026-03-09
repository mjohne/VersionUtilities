namespace VersionUtilities
{
    public class AdvancedVersionRange
    {
        public SemVersion MinVersion { get; private set; }
        public SemVersion MaxVersion { get; private set; }

        public bool IncludeMin { get; private set; }
        public bool IncludeMax { get; private set; }

        public static AdvancedVersionRange Parse(string range)
        {
            if (range is null)
                throw new ArgumentNullException(nameof(range));

            if (string.IsNullOrWhiteSpace(range))
                throw new ArgumentException("Version range cannot be empty or whitespace.", nameof(range));
            range = range.Trim();

            if (range.StartsWith("^"))
                return ParseCaret(range);

            if (range.StartsWith("~"))
                return ParseTilde(range);

            if (range.StartsWith("[") || range.StartsWith("("))
                return ParseNuGetRange(range);

            // exact version
            var v = SemVersion.Parse(range);

            return new AdvancedVersionRange
            {
                MinVersion = v,
                MaxVersion = v,
                IncludeMin = true,
                IncludeMax = true
            };
        }

        private static AdvancedVersionRange ParseNuGetRange(string range)
        {
            bool includeMin = range.StartsWith("[");
            bool includeMax = range.EndsWith("]");

            range = range.Trim('[', ']', '(', ')');

            var parts = range.Split(',');

            SemVersion min = null;
            SemVersion max = null;

            if (!string.IsNullOrWhiteSpace(parts[0]))
                min = SemVersion.Parse(parts[0]);

            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                max = SemVersion.Parse(parts[1]);

            return new AdvancedVersionRange
            {
                MinVersion = min,
                MaxVersion = max,
                IncludeMin = includeMin,
                IncludeMax = includeMax
            };
        }

        private static AdvancedVersionRange ParseCaret(string text)
        {
            var baseVersion = SemVersion.Parse(text.Substring(1));

            var max = new SemVersion(baseVersion.Major + 1, 0, 0);

            return new AdvancedVersionRange
            {
                MinVersion = baseVersion,
                MaxVersion = max,
                IncludeMin = true,
                IncludeMax = false
            };
        }

        private static AdvancedVersionRange ParseTilde(string text)
        {
            var baseVersion = SemVersion.Parse(text.Substring(1));

            var max = new SemVersion(baseVersion.Major, baseVersion.Minor + 1, 0);

            return new AdvancedVersionRange
            {
                MinVersion = baseVersion,
                MaxVersion = max,
                IncludeMin = true,
                IncludeMax = false
            };
        }

        public bool IsSatisfied(SemVersion version)
        {
            if (MinVersion != null)
            {
                int cmp = version.CompareTo(MinVersion);

                if (cmp < 0 || (cmp == 0 && !IncludeMin))
                    return false;
            }

            if (MaxVersion != null)
            {
                int cmp = version.CompareTo(MaxVersion);

                if (cmp > 0 || (cmp == 0 && !IncludeMax))
                    return false;
            }

            return true;
        }

        public static bool IsSatisfied(string version, string range)
        {
            var v = SemVersion.Parse(version);
            var r = Parse(range);

            return r.IsSatisfied(v);
        }
    }
}
