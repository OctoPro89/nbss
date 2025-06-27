using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace nbss
{
    public class FileItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("absolute_path")]
        public string AbsolutePath { get; set; }

        [JsonProperty("s3_path")]
        public string S3Path { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        [JsonProperty("zipped")]
        public bool Zipped { get; set; }

        [JsonProperty("infected")]
        public bool Infected { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    public class TorrentData
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("torrent_id")]
        public int TorrentId { get; set; }

        [JsonProperty("auth_id")]
        public string AuthId { get; set; }
    }

    public class CreateTorrentResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("data")]
        public TorrentData Data { get; set; }
    }

    public class TorrentItem
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("auth_id")]
        public string? AuthId { get; set; }

        [JsonProperty("server")]
        public int? Server { get; set; }

        [JsonProperty("hash")]
        public string? Hash { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("magnet")]
        public string? Magnet { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("allow_zipped")]
        public bool? AllowZipped { get; set; }

        [JsonProperty("availability")]
        public float? Availability { get; set; }

        [JsonProperty("cached")]
        public bool? Cached { get; set; }

        [JsonProperty("cached_at")]
        public DateTime? CachedAt { get; set; }

        [JsonProperty("download_finished")]
        public bool? DownloadFinished { get; set; }

        [JsonProperty("download_present")]
        public bool? DownloadPresent { get; set; }

        [JsonProperty("download_path")]
        public string? DownloadPath { get; set; }

        [JsonProperty("download_speed")]
        public long? DownloadSpeed { get; set; }

        [JsonProperty("download_state")]
        public string? DownloadState { get; set; }

        [JsonProperty("eta")]
        public long? Eta { get; set; }

        [JsonProperty("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [JsonProperty("files")]
        public List<FileItem>? Files { get; set; }

        [JsonProperty("long_term_seeding")]
        public bool? LongTermSeeding { get; set; }

        [JsonProperty("owner")]
        public string? Owner { get; set; }

        [JsonProperty("peers")]
        public int? Peers { get; set; }

        [JsonProperty("progress")]
        public float? Progress { get; set; }

        [JsonProperty("ratio")]
        public float? Ratio { get; set; }

        [JsonProperty("seed_torrent")]
        public bool? SeedTorrent { get; set; }

        [JsonProperty("seeds")]
        public int? Seeds { get; set; }

        [JsonProperty("torrent_file")]
        public bool? TorrentFile { get; set; }

        [JsonProperty("total_downloaded")]
        public long? TotalDownloaded { get; set; }

        [JsonProperty("total_uploaded")]
        public long? TotalUploaded { get; set; }

        [JsonProperty("tracker")]
        public string? Tracker { get; set; }

        [JsonProperty("tracker_message")]
        public string? TrackerMessage { get; set; }

        [JsonProperty("upload_speed")]
        public long? UploadSpeed { get; set; }
    }

    public class GetMyListResponse
    {
        [JsonProperty("success")]
        public bool? Success { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("detail")]
        public string? Detail { get; set; }

        [JsonProperty("data")]
        public List<TorrentItem>? Data { get; set; }
    }

    public class RequestDirectDownloadLinkResponse
    {
        [JsonProperty("success")]
        public bool? Success { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("detail")]
        public string? Detail { get; set; }

        [JsonProperty("data")]
        public string? ContentURL { get; set; }
    }

    public class ControlTorrentResponse
    {
        [JsonProperty("success")]
        public bool? Success { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("detail")]
        public string? Detail { get; set; }
    }
}
