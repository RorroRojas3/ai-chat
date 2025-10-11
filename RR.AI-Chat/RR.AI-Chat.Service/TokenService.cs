using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RR.AI_Chat.Service
{
    public interface ITokenService
    {
        Guid? GetOid();
    }

    public class TokenService(ILogger<TokenService> logger, 
        IHttpContextAccessor httpContextAccessor) : ITokenService
    {
        private readonly ILogger<TokenService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid? GetOid() 
        {  
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var oidClaim = user?.Claims?.FirstOrDefault(x => x.Type == "oid");
            return oidClaim != null ? Guid.Parse(oidClaim.Value) : null;
        }
    }
}
