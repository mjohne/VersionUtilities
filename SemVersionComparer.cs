using System.Collections.Generic;

namespace VersionUtilities
{
    public sealed class SemVersionComparer : IComparer<string>
    {
        public int Compare(string x, string y)
            => SemVersion.Parse(x).CompareTo(SemVersion.Parse(y));
        public static bool Greater(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) > 0;

        public static bool Lower(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) < 0;

        public static bool Equal(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) == 0;
    }
}
