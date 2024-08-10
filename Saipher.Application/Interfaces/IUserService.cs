using Saipher.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserModel> Authenticate(string username, string password);
        string GenerateJwtToken(UserModel user);
        Task<UserModel> CreateUserAsync(UserModel user);
        Task<PagedResult<UserModel>> GetPagedUsersAsync(int pageNumber, int pageSize);
        Task<UserModel> GetByIdAsync(int id);
        Task<UserModel> UpdateAsync(UserModel user);
        Task<UserModel> DeleteAsync(int id);
        Task<UserModel> RealDeleteAsync(int id);
    }
}
