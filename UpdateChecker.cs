using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace VersionUtilities
{
    public class UpdateChecker
    {
        public string UpdateUrl { get; }

        public UpdateChecker(string updateUrl)
        {
            UpdateUrl = updateUrl;
        }

        public async Task<UpdateInfo> GetLatestVersionAsync()
        {
            using HttpClient client = new HttpClient();

            string json = await client.GetStringAsync(UpdateUrl);

            return JsonSerializer.Deserialize<UpdateInfo>(json);
        }

        public async Task<bool> IsUpdateAvailableAsync()
        {
            var latest = await GetLatestVersionAsync();

            var current = ApplicationVersion.GetSemanticVersion();
            var remote = SemVersion.Parse(latest.Version);

            return remote.CompareTo(current) > 0;
        }

        public async Task<(bool updateAvailable, UpdateInfo info)> CheckAsync()
        {
            var latest = await GetLatestVersionAsync();

            var current = ApplicationVersion.GetSemanticVersion();
            var remote = SemVersion.Parse(latest.Version);

            bool update = remote.CompareTo(current) > 0;

            return (update, latest);
        }
    }
}
