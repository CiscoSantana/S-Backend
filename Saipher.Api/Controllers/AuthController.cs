using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saipher.Api.Models;
using Saipher.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Saipher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return Unauthorized(new { message = "Login ou Senha incorreto" });

            var token = _userService.GenerateJwtToken(user);
            return Ok(new { token });
        }
    }
}
