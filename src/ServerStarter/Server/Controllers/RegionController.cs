using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.util;
using ServerStarter.Shared;

namespace ServerStarter.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegionController : ControllerBase
    {
        private readonly ICommunityRepository      _repository;
        private readonly ICommunityService         _service;
        private readonly ILogger<RegionController> _logger;
        private readonly IUrlHelper                _urlHelper;

        public RegionController(ICommunityRepository repository, 
                                ICommunityService service, 
                                ILogger<RegionController> logger, 
                                IUrlHelper urlHelper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _service    = service    ?? throw new ArgumentNullException(nameof(service));
            _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
            _urlHelper  = urlHelper  ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        [HttpGet]
        public async Task<IEnumerable<Region>> Get(CancellationToken ct)
        {
            var communities = await _repository.Get();

            return await communities
                         .Select(async community =>
                                 {
                                     using (_logger.BeginScope("Region {@CommunityId}", community.Id))
                                     {
                                         var updatedCommunity = await _service.UpdateCommunity(community, ct);

                                         return CreateOverview(updatedCommunity);
                                     }
                                 })
                         .Sequence();
        }

        [HttpGet("{id}")]
        public async Task<Region> Get(Guid id, CancellationToken ct)
        {
            using (_logger.BeginScope("Region {@CommunityId}", id))
            {
                var community = await _repository.Get(id);

                var updatedCommunity = await _service.UpdateCommunity(community, ct);

                return CreateOverview(updatedCommunity);
            }
        }

        private Region CreateOverview(CommunityUpdate updatedCommunity)
        {
            var        iconUrl = _urlHelper.Action("GetIcon", "Communities", new {id =updatedCommunity.Id});
            return new Region
                   {
                       Id             = updatedCommunity.Id,
                       Name           = updatedCommunity.Name,
                       Updated        = updatedCommunity.Updated,
                       WaitingPlayers = updatedCommunity.WaitingPlayers,
                       CurrentPlayers = updatedCommunity.Servers
                                                        .Select(s => s.CurrentPlayers)
                                                        .Sum(),
                       IconUrl = iconUrl,
                   };
        }
    }
}
