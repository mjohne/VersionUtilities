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
            if (string.IsNullOrWhiteSpace(updateUrl))
            {
                throw new ArgumentException("Update URL must not be null or whitespace.", nameof(updateUrl));
            }

            if (!Uri.TryCreate(updateUrl, UriKind.Absolute, out var uriResult) ||
        private static readonly HttpClient SharedHttpClient = new HttpClient();

        private readonly HttpClient _httpClient;

        public string UpdateUrl { get; }

        public UpdateChecker(string updateUrl)
            : this(updateUrl, SharedHttpClient)
        {
        }

        public UpdateChecker(string updateUrl, HttpClient httpClient)
        {
            UpdateUrl = updateUrl;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UpdateInfo> GetLatestVersionAsync()
        {
            string json = await _httpClient.GetStringAsync(UpdateUrl);
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
