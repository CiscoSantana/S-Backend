using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Saipher.Application.Interfaces;
using Saipher.Domain.Entities;
using Saipher.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _secret;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _secret = configuration["JwtSecret"];
            Console.WriteLine($"JWT Secret: {_secret}"); 
        }

        public async Task<UserModel> Authenticate(string username, string password)
        {
            var user = await _userRepository.GetUserByCredentials(username, password);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            var auxUser = await _userRepository.GetByLoginAsync(user.Login);
            if (auxUser != null)
            {
                //Se diferente de null, o login já existe
                //Independente se está ativo ou não, assim não duplica login e é possível reativar a conta posteriormente.
                return null;
            }

            user.CreateAt = DateTime.UtcNow;
            user.DeletedAt = null;
            user.IsEnabled = true;

            return await _userRepository.CreateUserAsync(user);

        }
        public async Task<PagedResult<UserModel>> GetPagedUsersAsync(int pageNumber, int pageSize)
        {
            return await _userRepository.GetPagedUsersAsync(pageNumber, pageSize);
        }

        public async Task<UserModel> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();//Responsável por gerar o toke, pode ler evalidar também, mas aqui só gera
            var key = Encoding.ASCII.GetBytes(_secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity//Representa a identidade do usuário que está se autenticando
                (
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())//Permite que o id do usuário seja recuperado
                    }
                ),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            Console.WriteLine($"Generated Token: {tokenString}");

            return tokenString;
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            var existingUser = await _userRepository.GetAllDataByIdAsync(user.Id);            

            if (existingUser == null) 
            {
                return null;
            }

            if(string.IsNullOrEmpty(user.Passwd))
            {
                user.Passwd = existingUser.Passwd;
            }            

            user.CreateAt = existingUser.CreateAt;


            return await _userRepository.UpDateUserAsync(user);
        }

        public async Task<UserModel> DeleteAsync(int id)
        {
            var existingUser = await _userRepository.GetAllDataByIdAsync(id);

            existingUser.IsEnabled = false;
            existingUser.DeletedAt = DateTime.UtcNow;

            return await UpdateAsync(existingUser);
        }

        public async Task<UserModel> RealDeleteAsync(int id)
        {
            var existingUser = await _userRepository.GetAllDataByIdAsync(id);

            if (existingUser == null)
            {
                return null;
            }

            var deletedUser = await _userRepository.DeleteUserAsync(existingUser);

            deletedUser.Passwd = string.Empty;

            return deletedUser;
        }
    }
}
