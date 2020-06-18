using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Zarlo.Stats.Data;
using Zarlo.Stats.Data.Responses;

namespace Zarlo.Stats.DummyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerController : ControllerBase
    {

        private readonly ILogger<ServerController> _logger;

        public ServerController(ILogger<ServerController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("[action]")]
        public ServersResponse List()
        {
            string json = System.IO.File.ReadAllText("serverlist.json");
            return JsonConvert.DeserializeObject<ServersResponse>(json);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("[action]")]
        public OnlinePlayerResponse Online()
        {
            string json = System.IO.File.ReadAllText("playersonline.json");
            return JsonConvert.DeserializeObject<OnlinePlayerResponse>(json);
        }
    }
}
