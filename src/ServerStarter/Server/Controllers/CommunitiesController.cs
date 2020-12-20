using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Services;
using ServerStarter.Server.util;
using Community = ServerStarter.Shared.Community;

namespace ServerStarter.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommunitiesController : ControllerBase
    {
        private readonly ICommunityRepository           _repository;
        private readonly ICommunityService              _service;
        private readonly ILogger<CommunitiesController> _logger;
        private readonly IUrlHelper                     _urlHelper;

        public CommunitiesController(ICommunityRepository repository, 
                                     ICommunityService service, 
                                     ILogger<CommunitiesController> logger, 
                                     IUrlHelper urlHelper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _service    = service    ?? throw new ArgumentNullException(nameof(service));
            _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
            _urlHelper  = urlHelper  ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        [HttpGet]
        public async Task<IEnumerable<Community>> Get(CancellationToken ct)
        {
            var communities = await _repository.Get();

            return await communities
                         .Select(async community =>
                                 {
                                     using (_logger.BeginScope("Community {@CommunityId}", community.Id))
                                     {
                                         var update  = await _service.UpdateCommunity(community, ct);
                                         var iconUrl = _urlHelper.Action("GetIcon", "Communities", new { id = update.Id });
                                         return update.ToDto(iconUrl);
                                     }
                                 })
                         .Sequence();
        }

        [HttpGet("{id}")]
        public async Task<Community> Get(Guid id, CancellationToken ct)
        {
            using (_logger.BeginScope("Community {@CommunityId}", id))
            {
                var community = await _repository.Get(id);

                var update  = await _service.UpdateCommunity(community, ct);
                var iconUrl = _urlHelper.Action("GetIcon", "Communities", new { id = id });
                return update.ToDto(iconUrl);
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}/icon")]
        [ResponseCache(CacheProfileName = CacheProfileName.StaticFiles)]
        public async Task<FileContentResult> GetIcon(Guid id, CancellationToken ct)
        {
            using (_logger.BeginScope("CommunityIcon {@CommunityId}", id))
            {
                var icon = await _repository.GetIcon(id, ct);
                return File(icon.Bytes, icon.ContentType);
            }
        }
    }
}
