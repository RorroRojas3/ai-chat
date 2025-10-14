using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        private readonly string _oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public Guid? GetOid() 
        {  
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                _logger.LogWarning("No user context available to extract OID.");
                return null;
            }

            var oidClaim = user?.Identities?.FirstOrDefault()?.Claims?.FirstOrDefault(x => x.Type == _oidClaimType);
            return oidClaim != null ? Guid.Parse(oidClaim.Value) : null;
        }
    }
}
