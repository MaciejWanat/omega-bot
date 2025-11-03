namespace OmegaBot
{
    public record AppSettings
    {
        public ulong GuildId { get; init; }
        public ulong PostChannelId { get; init; }
        public string AuthToken { get; init; }
        public string FetchCron { get; init; }
        public string RssFeed { get; init; }
    }
}
