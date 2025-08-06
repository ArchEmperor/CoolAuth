using CoolAuth.DTOs;
using CoolAuth.Requests;
using CoolAuth.Services;
using CoolAuth.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CoolAuth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController(IDistributedCache cache,IAuthService auth) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginRequest  request)
        {
            var tokens= await auth.LoginAsync(request,new SessionConnectionInfoDTO()
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress!,
                UserAgent = Request.Headers.UserAgent.ToString(),
            });
            return Ok(tokens);
        }
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody]SignUpRequest  request)
        {
            var tokens=await auth.SignUpAsync(request,new SessionConnectionInfoDTO()
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress!,
                UserAgent = Request.Headers.UserAgent.ToString(),
            });
            return Ok(tokens);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshSession([FromBody]RefreshSessionRequest  request)
        {
            var tokens = await auth.RefreshSessionAsync(request,new SessionConnectionInfoDTO()
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress!,
                UserAgent = Request.Headers.UserAgent.ToString(),
            });
            return Ok(tokens);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SignOut([FromBody]string refreshToken)
        {
            await auth.SignOutAsync(refreshToken);
            return NoContent();
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Revoke(bool all=false,Guid? sessionId=null)
        {
            await auth.RevokeAsync(HttpContext,all,sessionId);
            return Ok();
        }
    }
}
