using nbss.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nbss
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new Dashboard(); // Set data context to view model
        }

        private async void OnGetMylistButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is Dashboard vm)
            {
                torrentsListBox.ItemsSource = null;
                filesListBox.ItemsSource = null;

                resultText.Text = await vm.GetMylist();

                torrentsListBox.ItemsSource = vm.Torrents;
            }
        }

        private void OnCreateTorrentButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateTorrentDialog dialog = new CreateTorrentDialog();
            if (dialog.ShowDialog() == true)
            {
                if (dialog.response.Error != null)
                {
                    resultText.Text = $"Failed to create torrent with error: '{dialog.response.Error}'";
                }
                else
                {
                    resultText.Text = "Torrent created successfully.";
                }
            }
        }

        private void filesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is Dashboard vm)
            {
                vm.SelectedFile = filesListBox.SelectedItem as FileItem;
                resultText.Text = ""; // Clear previous link
            }
        }

        private async void OnGetDownloadLink_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Dashboard vm && vm.SelectedFile != null && vm.SelectedTorrent != null)
            {
                RequestDirectDownloadLinkResponse response = await TorboxAPI.RequestDownloadLinkAsync($"{vm.SelectedTorrent.Id}", $"{vm.SelectedFile.Id}");

                if (response.Error != null)
                {
                    resultText.Text = $"Failed to get direct download link with error: {response.Error}'";
                }
                else
                {
                    Clipboard.SetText(response.ContentURL);
                    resultText.Text = "Direct download link copied to clipboard.";
                }
            }
        }

        private async void OnStreamButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Dashboard vm && vm.SelectedFile != null && vm.SelectedTorrent != null)
            {
                RequestDirectDownloadLinkResponse response = await TorboxAPI.RequestDownloadLinkAsync($"{vm.SelectedTorrent.Id}", $"{vm.SelectedFile.Id}");

                if (response.Error != null)
                {
                    resultText.Text = $"Failed to stream with error: {response.Error}'";
                }
                else
                {
                    VideoPlayer vc = new();
                    vc.Show();
                    await vc.SetVideo(response.ContentURL);
                }
            }
        }

        private string GetFileName(HttpResponseMessage response, string url)
        {
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                return response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            return Path.GetFileName(new Uri(url).LocalPath);
        }

        private async Task DownloadFilesSequentiallyAsync(List<string> urls, string outputFolder)
        {
            using var httpClient = new HttpClient();

            foreach (var url in urls)
            {
                try
                {
                    resultText.Text = $"Downloading: {url}";

                    using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    // Try to get filename from headers, fallback if needed
                    var fileName = GetFileName(response, url);
                    var filePath = Path.Combine(outputFolder, fileName);

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = File.Create(filePath);

                    await stream.CopyToAsync(fileStream);

                    resultText.Text = $"Finished: {fileName}";
                }
                catch (Exception ex)
                {
                    resultText.Text = $"Failed: {url}";
                }
            }
        }

        private async void OnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Dashboard vm && vm.SelectedTorrent != null)
            {
                List<string> urls = new List<string>();
                for (int i = 0; i < vm.SelectedTorrent.Files.Count; ++i)
                {
                    RequestDirectDownloadLinkResponse response = await TorboxAPI.RequestDownloadLinkAsync($"{vm.SelectedTorrent.Id}", $"{vm.SelectedTorrent.Files[i].Id}");
                    if (response.Error != null)
                    {
                        resultText.Text = $"Failed to get torrent direct download link with error: {response.Error}'";
                        continue;
                    }
                    urls.Add(response.ContentURL);
                }

                await DownloadFilesSequentiallyAsync(urls, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
            }
        }

        private async void DeleteTorrent_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Dashboard vm && vm.SelectedTorrent != null && vm.SelectedTorrent.Id != null)
            {
                ControlTorrentResponse response = await TorboxAPI.ControlTorrentAsync(vm.SelectedTorrent.Id?.ToString(), "delete");
                if (response.Error != null)
                {
                    resultText.Text = $"Failed to delete torrent from account with error: ${response.Error}";
                }
                else
                {
                    resultText.Text = "Torrent removed from account successfully, click 'Get Mylist' to refresh.";
                    vm.Torrents.RemoveAt(torrentsListBox.SelectedIndex);
                }
            }
        }

        private void OnTorrentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is Dashboard vm && vm.SelectedTorrent != null)
            {
                torrentViewName.Text = vm.SelectedTorrent.Files != null ? "" : "No files in torrent (torrent may be downloading)";
                if (vm.SelectedTorrent.Files == null)
                {
                    filesListBox.ItemsSource = null;
                }
                else
                {
                    filesListBox.ItemsSource = vm.SelectedTorrent.Files;
                }
            }
        }

        private void OnSetApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            APIKeyDialog prompt = new APIKeyDialog();
            if (prompt.ShowDialog() == true)
            {
                string apiKey = prompt.APIKey;
                Properties.Settings.Default.APIKey = apiKey;
                Properties.Settings.Default.Save();
                TorboxAPI.SetAPIKey(apiKey);
            }
        }
    }
}
