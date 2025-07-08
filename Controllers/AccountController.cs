using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Account;
using backend.Services;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var result = await _authService.RegisterAsync(registerRequest);

            if (result.Success)
            {
                return Ok(new { message = "Usuario registrado con éxito.", userId = result.Data });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _authService.LoginAsync(loginRequest);

            if (result.Success)
            {
                return Ok(new { token = result.Data });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }
    }
}