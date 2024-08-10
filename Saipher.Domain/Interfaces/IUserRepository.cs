using Saipher.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> GetUserByCredentials(string username, string password);
        Task<UserModel> GetAllDataByIdAsync(int id);
        Task<UserModel> GetByIdAsync(int id);
        Task<UserModel> GetByLoginAsync(string login);
        Task<UserModel> CreateUserAsync(UserModel user);
        Task<PagedResult<UserModel>> GetPagedUsersAsync(int pageNumber, int pageSize);
        Task<UserModel> UpDateUserAsync(UserModel user);
        Task<UserModel> DeleteUserAsync(UserModel user);
    }
}
