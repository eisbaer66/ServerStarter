using System;

namespace ServerStarter.Server
{
    public interface ITimingSettings
    {
        TimeSpan Interval                     { get; set; }
        TimeSpan UpdateCommunitiesMaxDuration { get; set; }
        TimeSpan UpdateCommunityMaxDuration   { get; set; }
        TimeSpan CacheLength                  { get; set; }
    }

    public class TimingSettings : ITimingSettings
    {
        public TimeSpan Interval { get; set; }
        public TimeSpan UpdateCommunitiesMaxDuration { get; set; }
        public TimeSpan UpdateCommunityMaxDuration { get; set; }
        public TimeSpan CacheLength { get; set; }
    }
}