using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using ServerStarter.Server;
using ServerStarter.Shared;

namespace ServerStarter.Client.Components
{
    public partial class JoinServerPopupComponent
    {
        private readonly Random    _random = new Random();
        private          Community _community;
        private          DateTime  _autoJoinTime;
        private          Timer     _timer;

        private int             SecondsTillAutoJoin => Math.Max(0, (int) (_autoJoinTime - DateTime.UtcNow).TotalSeconds);
        private bool            ShowPopup           => _community != null;
        private CommunityServer PreferredServer     => _community.Servers.FirstOrDefault(s => s.PreferredForQueue);

        public async Task Notify(Community community)
        {
            if (ShowPopup)
                return;

            _community = community;

            if (PreferredServer == null)
            {
                Logger.LogError("could not find fitting server for players in {Community}", _community);
                return;     //TODO Callback for zarlo?
            }

            if (QueueSettings.PlaySounds)
            {
                string id = "startSound" + _random.Next(4);
                await JsRuntime.InvokeAsync<string>("Play", id);
            }

            if (await UserIsPlayingOnServer(PreferredServer))
            {
                Logger.LogInformation("user is already playing on {PreferredServerIp}. leaving queue", PreferredServer.Ip);
                await LeaveQueue();
            }
            else
                await SetupAutomaticJoin();

            StateHasChanged();
        }

        private async Task SetupAutomaticJoin()
        {
            if (!QueueSettings.AutomaticJoinEnabled)
            {
                return;
            }

            _autoJoinTime = DateTime.UtcNow.AddSeconds(QueueSettings.AutomaticJoinDelayInSeconds);
            if (DateTime.UtcNow >= _autoJoinTime)
            {
                Logger.LogTrace("auto-joining immediately");
                await JoinGame();
                return;
            }

            SetupAutoJoinUpdates();
        }

        private async Task JoinGame()
        {
            DisableAutoJoinTimer();
            await QueueService.JoinGame(PreferredServer);
        }

        private async Task LeaveQueue()
        {
            DisableAutoJoinTimer();
            await QueueService.Leave(_community.Id);
        }

        private void SetupAutoJoinUpdates()
        {
            _timer = new Timer(1000)
            {
                AutoReset = true,
            };
            _timer.Elapsed += async (sender, args) =>
            {
                Logger.LogTrace("checking auto-join");

                if (DateTime.UtcNow >= _autoJoinTime)
                {
                    Logger.LogTrace("auto-joining");
                    await JoinGame();
                }

                StateHasChanged();
            };
            _timer.Enabled = true;
        }

        private async Task<bool> UserIsPlayingOnServer(CommunityServer preferredServer)
        {

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var claim = user.FindFirst(IcebearClaimTypes.SteamId);
            if (claim == null)
            {
                Logger.LogError("{SteamIdClaimType} claim not found. user is considered to not be playing on any server", IcebearClaimTypes.SteamId);
                return false;
            }

            return PreferredServer.Players.Select(p => p.SteamId.ToString()).Contains(claim.Value);
        }

        private void DisableAutoJoinTimer()
        {
            _autoJoinTime = DateTime.MinValue;
            if (_timer == null)
                return;

            _timer.Enabled = false;
            _timer.Dispose();
            _timer = null;
        }

        public async Task ClosePopup()
        {
            await LeaveQueue();
            _community = null;
        }
    }
}
