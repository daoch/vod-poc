using backend.Interfaces;
using backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest req)
        {
            var result = await _authService.LoginAsync(req);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }
    }
}
