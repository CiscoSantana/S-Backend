using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saipher.Application.Interfaces;
using Saipher.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Saipher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel user)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(user);

                if (createdUser == null)
                {
                    return Conflict(new { message = "Nome de usuário existente", details = "O nome de usuário escolhido já existe" });
                }

                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {                
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedUsers(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetPagedUsersAsync(pageNumber, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
           
        }

        [HttpPut]        
        public async Task<IActionResult> UpdateUser([FromBody] UserModel user)
        {
            try
            {
                var updatedUser = await _userService.UpdateAsync(user);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpDelete]        
        public async Task<IActionResult> DeleteUser([FromBody] int id)
        {
            try
            {
                var deletedUser = await _userService.DeleteAsync(id);

                return Ok(deletedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("RealDeleteUser")]
        [SwaggerOperation(Summary = "Exclusão real de dados!", Description = "ATENÇÃO! Esse endpoint faz a exclusão definitiva do registro! Prefira a exclusão lógica!")]
        public async Task<IActionResult> RealDeleteUser([FromBody] int id)
        {
            try
            {
                var deletedUser = await _userService.RealDeleteAsync(id);

                return Ok(deletedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
