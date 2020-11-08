using System;

namespace ServerStarter.Server
{
    public interface IManagedTimedHostedWorkerSettings
    {
        TimeSpan Interval                     { get; set; }
        TimeSpan MaxDuration                  { get; set; }
        bool     OnlyRunIfHubConnectionPresent { get; set; }
    }
    public class ManagedTimedHostedWorkerSettings : IManagedTimedHostedWorkerSettings
    {
        public TimeSpan Interval                      { get; set; }
        public TimeSpan MaxDuration                   { get; set; }
        public bool     OnlyRunIfHubConnectionPresent { get; set; }
    }
}