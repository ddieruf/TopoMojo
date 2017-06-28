using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TopoMojo.Data;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using Microsoft.Extensions.Logging;
using IdentityModel;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TopoMojo.Services
{
    public class ProfileResolver : IProfileResolver
    {
        public ProfileResolver(
            IHttpContextAccessor context
        ){
            _context = context;
        }

        private readonly IHttpContextAccessor _context;
        private Profile _profile = null;

        public Profile Profile {
            get
            {
                return (_profile != null)
                    ? _profile
                    : BuildProfile();
            }
        }

        private Profile BuildProfile()
        {
            lock(this)
            {
                if (_profile == null)
                {
                    _profile = new Profile();
                    _profile.GlobalId = _context.HttpContext.User.FindFirst(JwtClaimTypes.Subject)?.Value;
                    _profile.Name = _context.HttpContext.User.FindFirst(JwtClaimTypes.Name)?.Value ?? "Anonymous";
                    _profile.IsAdmin = _context.HttpContext.User.IsInRole("admin");
                    if (Int32.TryParse(_context.HttpContext.User.FindFirst("aid")?.Value, out int id))
                    {
                        _profile.Id = id;
                    }
                }
            }
            return _profile;
        }
    }

    public class DirectProfileResolver : IProfileResolver
    {
        public void Init() {}

        public DirectProfileResolver(Profile profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; private set; }
    }
}