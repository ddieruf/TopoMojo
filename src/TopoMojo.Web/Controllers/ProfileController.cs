﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Core.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    public class ProfileController : _Controller
    {
        public ProfileController(
            ProfileManager profileManager,
            IServiceProvider sp
        ) : base(sp)
        {
            _mgr = profileManager;
        }

        private readonly ProfileManager _mgr;

        [Authorize(Roles = "admin")]
        [HttpGet("api/profiles")]
        [ProducesResponseType(typeof(SearchResult<Profile>), 200)]
        [JsonExceptionFilter]
        public async Task<IActionResult> List([FromQuery]Search search)
        {
            var result = await _mgr.List(search);
            return Ok(result);
        }

        [HttpPost("api/profile")]
        [ProducesResponseType(typeof(ChangedProfile), 200)]
        [JsonExceptionFilter]
        public async Task<ChangedProfile> UpdateProfile([FromBody]ChangedProfile profile)
        {
            await _mgr.UpdateProfile(profile);
            return profile;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("api/profile/priv")]
        [ProducesResponseType(typeof(Profile), 200)]
        [JsonExceptionFilter]
        public async Task<Profile> PrivilegedUpdate([FromBody]Profile profile)
        {
            await _mgr.PrivilegedUpdate(profile);
            return profile;

        }
    }

}