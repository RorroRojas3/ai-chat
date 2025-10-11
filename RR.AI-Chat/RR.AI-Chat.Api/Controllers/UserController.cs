using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto.Actions.User;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserActionDto request)
        {
            await _userService.CreateUserAsync(request);
            return Created();
        }
    }
}
