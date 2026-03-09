namespace VersionUtilities
{
    public static class SemVersionComparer
    {
        public static bool Greater(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) > 0;

        public static bool Lower(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) < 0;

        public static bool Equal(string v1, string v2)
            => SemVersion.Parse(v1).CompareTo(SemVersion.Parse(v2)) == 0;
    }
}
