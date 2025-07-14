using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        [HttpPost]
        public async Task CreateSessionAsync()
        {

        }
    }
}
