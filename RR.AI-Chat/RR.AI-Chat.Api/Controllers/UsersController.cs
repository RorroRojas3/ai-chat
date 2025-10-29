using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto.Actions.User;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserActionDto request, CancellationToken cancellationToken)
        {
            await _userService.CreateUserAsync(request, cancellationToken);
            return Created();
        }
    }
}
