using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.util;
using Community = ServerStarter.Shared.Community;

namespace ServerStarter.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CommunitiesController : ControllerBase
    {
        private readonly ICommunityRepository _repository;
        private readonly ICommunityService    _service;

        public CommunitiesController(ICommunityRepository repository, ICommunityService service)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _service    = service    ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IEnumerable<Community>> Get()
        {
            var communities = await _repository.Get();

            return await communities
                         .Select(_service.UpdateCommunity)
                         .WhenAll();
        }

        [HttpGet("{id}")]
        public async Task<Community> Get(Guid id)
        {
            var community = await _repository.Get(id);

            return await _service.UpdateCommunity(community);
        }
    }
}
