﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServerStarter.Shared;

namespace ServerStarter.Client.Hubs
{
    public interface IQueueService
    {
        HubConnectionState?            State        { get; }
        EventCallback<CommunityServer> JoinedServer { get; set; }
        Task                           Init();
        Task<IDisposable>              Init(StateEvents                    events);
        IDisposable                    OnCommunityChanged(Func<Guid, Task> callback);
        IDisposable                    On<T>(string                        methodName, Func<T, Task> callback);
        bool                           IsJoined(Guid                       communityId);
        Task                           Join(Guid                           communityId);
        Task                           Leave(Guid                          communityId);
        Task<IDisposable>              RegisterQueueEvents(QueueEvents     events);
        Task                           SendMessage(Guid                    communityId, string input);
        IEnumerable<Guid>              GetJoined();
        Task                           JoinGame(CommunityServer            server);
        Task                           JoinGameImmediately(CommunityServer server);
    }

    public class QueueService : IAsyncDisposable, IQueueService
    {
        private readonly        IJSRuntime    _jsRuntime;
        private static readonly SemaphoreSlim _stateEventsSemaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly HubConnection                  _hubConnection;
        private readonly HashSet<Guid>                  _joinedQueues = new HashSet<Guid>();
        private readonly IList<QueueEvents>             _queueEvents  = new List<QueueEvents>();
        private readonly IList<StateEvents>             _stateEvents  = new List<StateEvents>();
        public           EventCallback<CommunityServer> JoinedServer { get; set; }

        public async Task<HubConnection> GetConnection()
        {
            await EnsureConnectionIsStarted();
            return _hubConnection;
        }

        public HubConnectionState? State => _hubConnection?.State;

        public QueueService(IAccessTokenProvider accessTokenProvider, NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            if (accessTokenProvider == null) throw new ArgumentNullException(nameof(accessTokenProvider));
            if (navigationManager   == null) throw new ArgumentNullException(nameof(navigationManager));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

            _hubConnection = new HubConnectionBuilder()
                             .WithDefaultConfig(accessTokenProvider, navigationManager)
                             .Build();
            
            OnJoinQueue(async (communityId) => await Join(communityId));
            OnLeaveQueue(async (communityId) => await Leave(communityId));


            OnMessageReceived(async (communityId, user, message) =>
                              {
                                  foreach (QueueEvents events in _queueEvents)
                                  {
                                      await events.MessageReceived(communityId, user, message);
                                  }
                              });

            OnUserJoined(async (communityId, user) =>
                         {
                             foreach (QueueEvents events in _queueEvents)
                             {
                                 await events.UserJoined(communityId, user);
                             }
                         });

            OnUserLeft(async (communityId, user) =>
                       {
                           foreach (QueueEvents events in _queueEvents)
                           {
                               await events.UserLeft(communityId, user);
                           }
                       });
        }

        public async Task Init()
        {
            await EnsureConnectionIsStarted();
        }

        public async Task<IDisposable> Init(StateEvents events)
        {
            await EnsureConnectionIsStarted();

            await _stateEventsSemaphoreSlim.WaitAsync();
            _stateEvents.Add(events);
            _stateEventsSemaphoreSlim.Release();

            return new DisposeAction(() =>
            {
                _stateEventsSemaphoreSlim.Wait();
                _stateEvents.Remove(events);
                _stateEventsSemaphoreSlim.Release();
            });
        }

        public async Task<IDisposable> RegisterQueueEvents(QueueEvents events)
        {
            await EnsureConnectionIsStarted();
            _queueEvents.Add(events);
            return new DisposeAction(() => { _queueEvents.Remove(events); });
        }

        public async Task SendMessage(Guid communityId, string input)
        {
            var connection = await GetConnection();
            await connection.SendAsync("SendMessage", communityId, input);
        }

        public IEnumerable<Guid> GetJoined()
        {
            return _joinedQueues.ToArray();
        }

        private IDisposable OnJoinQueue(Func<Guid, Task> callback)
        {
            return On("JoinQueue", callback);
        }

        private IDisposable OnLeaveQueue(Func<Guid, Task> callback)
        {
            return On("LeaveQueue", callback);
        }

        private IDisposable OnMessageReceived(Func<string, string, string, Task> callback)
        {
            return _hubConnection.On("MessageReceived", callback);
        }

        private IDisposable OnUserJoined(Func<string, string, Task> callback)
        {
            return _hubConnection.On("UserJoined", callback);
        }

        private IDisposable OnUserLeft(Func<string, string, Task> callback)
        {
            return _hubConnection.On("UserLeft", callback);
        }

        public IDisposable OnCommunityChanged(Func<Guid, Task> callback)
        {
            return On("Changed", callback);
        }

        public IDisposable On<T>(string methodName, Func<T, Task> callback)
        {
            return _hubConnection.On<T>(methodName, callback);
        }

        public bool IsJoined(Guid communityId)
        {
            return _joinedQueues.Contains(communityId);
        }

        public async Task Join(Guid communityId)
        {
            if (_joinedQueues.Contains(communityId))
                return;

            var connection = await GetConnection();
            await connection.InvokeAsync("JoinGroup", communityId);
            
            _joinedQueues.Add(communityId);

            await _stateEventsSemaphoreSlim.WaitAsync();
            foreach (var e in _stateEvents)
            {
                await e.Joined(communityId);
            }
            _stateEventsSemaphoreSlim.Release();
        }

        public async Task Leave(Guid communityId)
        {
            if (!_joinedQueues.Contains(communityId))
                return;

            _joinedQueues.Remove(communityId);
            var connection = await GetConnection();
            await connection.InvokeAsync("LeaveGroup", communityId);

            await _stateEventsSemaphoreSlim.WaitAsync();
            foreach (var e in _stateEvents)
            {
                await e.Left(communityId);
            }
            _stateEventsSemaphoreSlim.Release();
        }

        public async Task JoinGameImmediately(CommunityServer server)
        {
            string link = GetConnectLink(server);
            await _jsRuntime.InvokeAsync<string>("openNewWindow", link);
        }

        public async Task JoinGame(CommunityServer server)
        {
            await JoinGameImmediately(server);

            if (JoinedServer.HasDelegate)
                await JoinedServer.InvokeAsync(server);
        }

        private string GetConnectLink(CommunityServer server)
        {
            return "steam://connect/" + server.Ip;
        }

        private async Task EnsureConnectionIsStarted()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                await _hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await (_hubConnection?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
    }

    public class QueueEvents
    {
        public string                             Name            { get; set; }
        public Func<string, string, string, Task> MessageReceived { get; set; } = async (communityId, user, message) => { };
        public Func<string, string, Task>         UserJoined      { get; set; } = async (communityId, user) => { };
        public Func<string, string, Task>         UserLeft        { get; set; } = async (communityId, user) => { };
    }

    public class StateEvents
    {
        public Func<Guid, Task> Joined { get; set; } = async (communityId) => { };
        public Func<Guid, Task> Left   { get; set; } = async (communityId) => { };
    }

    public class CommunityEventArgs
    {
        public Guid Id { get; }

        public CommunityEventArgs(Guid id)
        {
            Id = id;
        }
    }
}