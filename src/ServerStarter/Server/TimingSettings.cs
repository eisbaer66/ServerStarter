using System;

namespace ServerStarter.Server
{
    public interface ITimingSettings
    {
        TimeSpan UpdateCommunityMaxDuration        { get; set; }
        TimeSpan CacheLength                       { get; set; }
        TimeSpan CommunityCacheDuration            { get; set; }
        TimeSpan CommunityHeaderImageCacheDuration { get; set; }
        TimeSpan CommunityUpdateCacheDuration      { get; set; }
    }

    public class TimingSettings : ITimingSettings
    {
        public TimeSpan UpdateCommunityMaxDuration        { get; set; }
        public TimeSpan CacheLength                       { get; set; }
        public TimeSpan CommunityCacheDuration            { get; set; }
        public TimeSpan CommunityHeaderImageCacheDuration { get; set; }
        public TimeSpan CommunityUpdateCacheDuration      { get; set; }
    }
}