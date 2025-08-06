using CoolAuth.DTOs;
using CoolAuth.Requests;
using CoolAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CoolAuth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController(IDistributedCache cache) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Test([FromBody] string value)
        {
            await cache.SetStringAsync("test",value,new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return Ok(await cache.GetStringAsync("test"));
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginRequest  request)
        {
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody]SignUpRequest  request)
        {
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> SignOut([FromBody]TokensDTO tokens)
        {
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> RefreshSession([FromBody]TokensDTO tokens)
        {
            return Ok();
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Revoke(bool all,Guid sessionId)
        {
            
            
            
            return Ok();
        }
        
        
        
    }
}
