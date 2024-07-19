using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebsiteBlockerApp
{
    public class Blocker
    {
        private readonly List<string> pornSites;
        private readonly List<string> distractionSites;
        private readonly string hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";
        private readonly string pornSitesFilePath;
        private readonly string distractionSitesFilePath;
        private bool isFocusModeEnabled;

        public DateTime LastUpdated { get; private set; }
        public int PornSitesCount => pornSites.Count;
        public int DistractionSitesCount => distractionSites.Count;
        public bool IsDownloading { get; private set; }

        private readonly List<string> pornSources = new List<string>
        {
            "https://gist.githubusercontent.com/sibaram-sahu/5248d7600a24284f580219b29d178c49/raw/pornsiteslist.txt",
            "https://raw.githubusercontent.com/Bon-Appetit/porn-domains/master/domains.txt"
        };

        private readonly List<string> distractionSources = new List<string>
        {
            "https://github.com/DWW256/distracting-websites/raw/main/distracting-websites.txt"
        };

        public Blocker()
        {
            pornSites = new List<string>();
            distractionSites = new List<string>();
            isFocusModeEnabled = true; // تمكين وضع التركيز بشكل افتراضي
            pornSitesFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "pornsiteslist.txt");
            distractionSitesFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "distractionsiteslist.txt");
            LoadSavedSites();
        }

        private void LoadSavedSites()
        {
            if (File.Exists(pornSitesFilePath))
            {
                var savedPornSites = File.ReadAllLines(pornSitesFilePath);
                pornSites.AddRange(savedPornSites);
            }

            if (File.Exists(distractionSitesFilePath))
            {
                var savedDistractionSites = File.ReadAllLines(distractionSitesFilePath);
                distractionSites.AddRange(savedDistractionSites);
            }
        }

        public async Task LoadSites()
        {
            IsDownloading = true;
            await LoadSitesFromSources(pornSources, pornSitesFilePath, pornSites);
            await LoadSitesFromSources(distractionSources, distractionSitesFilePath, distractionSites);
            IsDownloading = false;
            await UpdateHostsFile();
            LastUpdated = DateTime.Now;
        }

        private async Task LoadSitesFromSources(List<string> sources, string filePath, List<string> siteList)
        {
            foreach (var source in sources)
            {
                if (await DownloadSitesListAsync(source, filePath))
                {
                    break;
                }
            }

            if (File.Exists(filePath))
            {
                var sites = await ReadAllLinesAsync(filePath);
                foreach (var site in sites)
                {
                    if (!siteList.Contains(site))
                    {
                        siteList.Add(site);
                    }
                }
            }
        }

        private async Task<bool> DownloadSitesListAsync(string url, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();
                    await WriteAllTextAsync(filePath, content);
                    return true;
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP error while downloading sites list from {url}: " + httpEx.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error while downloading sites list from {url}: " + ex.Message);
                }
                return false;
            }
        }

        public void AddDistractionSite(string site)
        {
            if (!distractionSites.Contains(site))
            {
                distractionSites.Add(site);
                SaveSites(distractionSites, distractionSitesFilePath);
                Task.Run(() => UpdateHostsFile()).Wait();
            }
        }

        public void RemoveDistractionSite(string site)
        {
            if (distractionSites.Contains(site))
            {
                distractionSites.Remove(site);
                SaveSites(distractionSites, distractionSitesFilePath);
                Task.Run(() => UpdateHostsFile()).Wait();
            }
        }

        private void SaveSites(List<string> sites, string filePath)
        {
            File.WriteAllLines(filePath, sites);
        }

        private async Task UpdateHostsFile()
        {
            try
            {
                var existingLines = await ReadAllLinesAsync(hostsFilePath);
                var newLines = existingLines.Where(line => !pornSites.Any(site => line.Contains(site)) && !distractionSites.Any(site => line.Contains(site))).ToList();

                foreach (var site in pornSites)
                {
                    newLines.Add($"127.0.0.1 {site}");
                    newLines.Add($"127.0.0.1 www.{site}");
                }

                if (isFocusModeEnabled)
                {
                    foreach (var site in distractionSites)
                    {
                        newLines.Add($"127.0.0.1 {site}");
                        newLines.Add($"127.0.0.1 www.{site}");
                    }
                }

                await WriteAllLinesAsync(hostsFilePath, newLines);
                Console.WriteLine("Hosts file updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while updating hosts file: " + ex.Message);
            }
        }

        public void RemoveDistractionSites()
        {
            try
            {
                var existingLines = File.ReadAllLines(hostsFilePath);
                var newLines = existingLines.Where(line => !distractionSites.Any(site => line.Contains(site))).ToList();

                File.WriteAllLines(hostsFilePath, newLines);
                Console.WriteLine("Distraction sites removed from hosts file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while removing distraction sites from hosts file: " + ex.Message);
            }
        }

        public List<string> GetPornSites()
        {
            return pornSites;
        }

        public List<string> GetDistractionSites()
        {
            return distractionSites;
        }

        public void EnableFocusMode()
        {
            isFocusModeEnabled = true;
            Task.Run(() => UpdateHostsFile()).Wait();
        }

        public void DisableFocusMode()
        {
            isFocusModeEnabled = false;
            Task.Run(() => UpdateHostsFile()).Wait();
        }

        private async Task<string[]> ReadAllLinesAsync(string path)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines.ToArray();
        }

        private async Task WriteAllTextAsync(string path, string content)
        {
            using (var writer = new StreamWriter(path, false))
            {
                await writer.WriteAsync(content);
            }
        }

        private async Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
        {
            using (var writer = new StreamWriter(path, false))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
        }
    }
}
