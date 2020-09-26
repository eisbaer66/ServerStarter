using System;
using System.ComponentModel.DataAnnotations;

namespace ServerStarter.Shared
{
    public class QueueSettings
    {
        public bool PlaySounds                  { get; set; }
        public bool AutomaticJoinEnabled        { get; set; }
        [Display(Name = "auto-join delay in seconds")]
        //[Range(minimum: 0, maximum: 60)]
        public int  AutomaticJoinDelayInSeconds { get; set; }
    }
}