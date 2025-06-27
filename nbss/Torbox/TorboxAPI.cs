using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nbss
{
    public static class TorboxAPI
    {
        private static HttpClient _httpClient = new();
        private static string _apiKey = "";

        public static void InitWithAPIKey(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public static async Task<CreateTorrentResponse> CreateTorrentAsync(string file, string magnet, string name, bool allowZip, bool asQueued, int seed)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(allowZip ? "true" : "false"), "allow_zip" },
                { new StringContent(asQueued ? "true" : "false"), "as_queued" },
                { new StringContent(magnet), "magnet" },
                { new StringContent(name), "name" },
                { new StringContent($"{seed}"), "seed" }
            };

            if (!string.IsNullOrEmpty(file))
            {
                formData.Add(new StringContent(file), "file");
            }

            if (!string.IsNullOrEmpty(file))
            {
                formData.Add(new StringContent(file), "magnet");
            }

            var response = await _httpClient.PostAsync("https://api.torbox.app/v1/api/torrents/createtorrent", formData);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateTorrentResponse>(json);
            return result;
        }

        public static async Task<ControlTorrentResponse> ControlTorrentAsync(string torrentId, string op)
        {
            var payload = new
            {
                torrent_id = torrentId,
                operation = op,
                all = false
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.torbox.app/v1/api/torrents/controltorrent", jsonContent);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ControlTorrentResponse>(json);
            return result;
        }

        public static async Task<GetMyListResponse> GetMyListAsync()
        {
            var response = await _httpClient.GetAsync("https://api.torbox.app/v1/api/torrents/mylist?offset=0&limit=1000&bypass_cache=false");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetMyListResponse>(json);
            return result;
        }

        public static async Task<string> GetQueuedDownloads()
        {
            /*
            var payload = new
            {
                operation = "start",
                all = "false"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.torbox.app/v1/api/queued/controlqueued", jsonContent);
            var json = await response.Content.ReadAsStringAsync();
            */

            string url = $"https://api.torbox.app/v1/api/queued/getqueued?" +
                         $"bypass_cache=false&type=torrent";

            var response = await _httpClient.GetAsync(url);

            // response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            // var result = JsonConvert.DeserializeObject<RequestDirectDownloadLinkResponse>(json);

            return json;
        }

        public static async Task<RequestDirectDownloadLinkResponse> RequestDownloadLinkAsync(string torrentId, string fileId)
        {
            string url = $"https://api.torbox.app/v1/api/torrents/requestdl?" +
                         $"token={_apiKey}&torrent_id={torrentId}&file_id={fileId}&" +
                         $"zip_link=false&torrent_file=false&user_ip=&redirect=false";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RequestDirectDownloadLinkResponse>(json);
            return result;
        }

        public static async Task<RequestDirectDownloadLinkResponse> RequestTorrentDownloadLinkAsync(string torrentId, bool zipped_link)
        {
            string url = $"https://api.torbox.app/v1/api/torrents/requestdl?" +
                         $"token={_apiKey}&torrent_id={torrentId}&" +
                         $"zip_link={(zipped_link ? "true" : "false")}&user_ip=&redirect=false";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RequestDirectDownloadLinkResponse>(json);
            return result;
        }
    }
}
