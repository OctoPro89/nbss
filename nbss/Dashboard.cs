using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace nbss
{
    public class Dashboard : INotifyPropertyChanged
    {
        private ObservableCollection<TorrentItem> _torrents;
        public ObservableCollection<TorrentItem> Torrents
        {
            get => _torrents;
            set { _torrents = value; OnPropertyChanged(); }
        }

        // Selected torrent
        private TorrentItem _selectedTorrent;
        public TorrentItem SelectedTorrent
        {
            get => _selectedTorrent;
            set
            {
                if (_selectedTorrent != value)
                {
                    _selectedTorrent = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DownloadStatus));
                    LoadFilesForSelectedTorrent();
                }
            }
        }

        private ObservableCollection<FileItem> _files;
        public ObservableCollection<FileItem> Files
        {
            get => _files;
            set { _files = value; OnPropertyChanged(); }
        }

        // Selected file in the files list
        private FileItem _selectedFile;
        public FileItem SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
            }
        }

        public string DownloadStatus
        {
            get
            {
                if (SelectedTorrent == null)
                    return "No torrent selected.";

                if (SelectedTorrent.DownloadState == null)
                    return "Status unknown.";

                var speed = SelectedTorrent.DownloadSpeed.HasValue
                    ? $"{FormatBytes(SelectedTorrent.DownloadSpeed.Value)}/s"
                    : "N/A";

                var progressPercent = SelectedTorrent.Progress.HasValue
                    ? $"{SelectedTorrent.Progress.Value * 100:F1}%"
                    : "N/A";

                var eta = SelectedTorrent.Eta.HasValue && SelectedTorrent.Eta.Value > 0
                    ? TimeSpan.FromSeconds(SelectedTorrent.Eta.Value).ToString(@"hh\:mm\:ss")
                    : "N/A";

                return $"Status: {SelectedTorrent.DownloadState} " +
                       $"Speed: {speed} " +
                       $"Progress: {progressPercent} " +
                       $"ETA: {eta}";
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public async Task<string> GetMylist()
        {
            string result = "Retrieved list successfully.";

            GetMyListResponse response = await TorboxAPI.GetMyListAsync();
            if (response.Error != null)
            {
                result = $"Encountered error while getting list: '{response.Error}'";
                return result;
            }

            Torrents = new ObservableCollection<TorrentItem>(response.Data);
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadFilesForSelectedTorrent()
        {
            if (_selectedTorrent == null) return;

            if (_selectedTorrent.Files != null)
            {
                Files = new ObservableCollection<FileItem>(_selectedTorrent.Files);
            }
            else
            {
                // Display some text
            }
        }

        public Dashboard()
        {
            Torrents = new ObservableCollection<TorrentItem>();
            Files = new ObservableCollection<FileItem>();
        }
    }
}
