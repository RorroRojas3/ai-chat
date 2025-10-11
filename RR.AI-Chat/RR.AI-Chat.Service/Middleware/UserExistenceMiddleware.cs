using Microsoft.AspNetCore.Http;
using RR.AI_Chat.Dto;
using System.Net;

namespace RR.AI_Chat.Service.Middleware
{
    public class UserExistenceMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            ITokenService tokenService,
            IUserService userService)
        {
            // Only check if user is authenticated via MSAL
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // MSAL typically uses "oid" (Object ID) or "sub" (Subject) claim
                var userId = tokenService.GetOid();
                var cancellationToken = context.RequestAborted;

                if (userId == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Message = "User ID claim not found in token.",
                        StatusCode = HttpStatusCode.Unauthorized
                    });
                    return;
                }

                var userExists = await userService.IsUserInDatabaseAsync(userId.Value, cancellationToken);

                if (!userExists)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Message = "User not found in database.",
                        StatusCode = HttpStatusCode.Unauthorized
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}