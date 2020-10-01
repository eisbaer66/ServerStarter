using System;

namespace ServerStarter.Server.Services
{
    public interface IMessaging
    {
        void                                    UserJoinedNotification(object sender, UserJoinedEventArgs args);
        event EventHandler<UserJoinedEventArgs> UserJoined;

        void                                  UserLeftNotification(object sender, UserLeftEventArgs args);
        event EventHandler<UserLeftEventArgs> UserLeft;
    }

    public class Messaging : IMessaging
    {
        public void UserJoinedNotification(object sender, UserJoinedEventArgs args)
        {
            UserJoined?.Invoke(sender, args);
        }

        public event EventHandler<UserJoinedEventArgs> UserJoined;

        public void UserLeftNotification(object sender, UserLeftEventArgs args)
        {
            UserLeft?.Invoke(sender, args);
        }

        public event EventHandler<UserLeftEventArgs> UserLeft;
    }
}