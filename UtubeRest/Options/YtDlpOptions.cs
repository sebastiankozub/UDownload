namespace UtubeRest.Options
{
    public class YtDlpOptions
    {
        public string CookiesFilePath { get; set; } = "/home/cookies/youtube.txt";
        public bool UseCookies { get; set; } = true;

        public string? UserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36";
        public string? RateLimit { get; set; } = "2M";
        public int SleepRequestsSeconds { get; set; } = 1;
        public int MaxSleepIntervalSeconds { get; set; } = 3;
        public string? ExtractorArgs { get; set; } = "youtube:player_client=android";
        public int Retries { get; set; } = 6;
        public int FragmentRetries { get; set; } = 6;
    }
}
