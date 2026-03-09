using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VersionUtilities
{
    public class AutoUpdater
    {
        public string TempFolder { get; }

        public AutoUpdater()
        {
            TempFolder = Path.Combine(Path.GetTempPath(), "AppUpdater");
        }

        public async Task<string> DownloadUpdateAsync(UpdateInfo info)
        {
            Directory.CreateDirectory(TempFolder);

            string zipPath = Path.Combine(TempFolder, "update.zip");

            using HttpClient client = new HttpClient();
            using (var responseStream = await client.GetStreamAsync(info.DownloadUrl))
            using (var fileStream = File.Create(zipPath))
            {
                await responseStream.CopyToAsync(fileStream);
            }
            if (!VerifySha256(zipPath, info.Sha256))
                throw new Exception("SHA256 verification failed.");

            return zipPath;
        }

        public void ExtractUpdate(string zipPath)
        {
            string extractPath = Path.Combine(TempFolder, "update");

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }

        public void InstallUpdate(string applicationFolder)
        {
            string extractPath = Path.Combine(TempFolder, "update");

            string updaterExe = Path.Combine(extractPath, "Updater.exe");

            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExe,
                Arguments = $"\"{extractPath}\" \"{applicationFolder}\"",
                UseShellExecute = true
            });

            Environment.Exit(0);
        }

        private bool VerifySha256(string file, string expectedHash)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(file);

            var hash = sha.ComputeHash(stream);
            string hashString = BitConverter.ToString(hash).Replace("-", "");

            return hashString.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
