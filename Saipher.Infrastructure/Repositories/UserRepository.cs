using Microsoft.EntityFrameworkCore;
using Saipher.Domain.Entities;
using Saipher.Domain.Interfaces;
using Saipher.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel> GetUserByCredentials(string username, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == username && u.Passwd == password);
        }

        public async Task<UserModel> GetByIdAsync(int id)
        {
            return await _context.Users.Select(u => new UserModel
            {
                Id = id,
                Login = u.Login,
                Email = u.Email,
                CreateAt = u.CreateAt,
                DeletedAt = u.DeletedAt,
                IsEnabled = u.IsEnabled,
            }).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel> GetAllDataByIdAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            _context.Entry(user).State = EntityState.Detached;
            return user;
        }

        public async Task<UserModel> GetByLoginAsync(string login)
        {
            return await _context.Users.Select(u => new UserModel
            {
                Id = u.Id,
                Login = u.Login,
                Email = u.Email,
                CreateAt = u.CreateAt.ToLocalTime(),
                DeletedAt = u.DeletedAt.HasValue ? u.DeletedAt.Value.ToLocalTime() : (DateTime?)null,
                IsEnabled = u.IsEnabled,
            }).FirstOrDefaultAsync(u => u.Login.Equals(login));
        }

        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            try
            {
                _context.Users.Add(user);

                //Se for igual a zero, significa que não conseguiu inseriri o usuário.
                if (await _context.SaveChangesAsync() == 0)
                {
                    throw new Exception("Não foi possível adicionar o usuário");
                }

                return user;
            }
            catch (DbUpdateException ex)
            {

                throw new Exception($"Um erro ocorreu enquanto a base de dados era atualizada: {ex.Message}", ex);
            }
            catch (Exception ex)
            {                
                throw new Exception($"Ocorreu um erro: {ex.Message}", ex);
            }

        }

        public async Task<PagedResult<UserModel>> GetPagedUsersAsync(int pageNumber, int pageSize)
        {
            var totalRecords = await _context.Users.CountAsync();

            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Login = u.Login,
                    Email = u.Email,
                    CreateAt = u.CreateAt.ToLocalTime(),
                    DeletedAt = u.DeletedAt.HasValue ? u.DeletedAt.Value.ToLocalTime() : (DateTime?)null,
                    IsEnabled = u.IsEnabled,
                })
                .ToListAsync();

            return new PagedResult<UserModel>
            {
                Items = users,
                TotalCount = totalRecords
            };
        }


        public async Task<UserModel> UpDateUserAsync(UserModel user)
        {
            try
            {
                _context.Users.Update(user);

                //Se for igual a zero, significa que não conseguiu atualizar o usuário.
                if (await _context.SaveChangesAsync() == 0)
                {
                    throw new Exception("Não foi possível atualizar o usuário");
                }

                user.Passwd = string.Empty;

                return user;
            }
            catch (DbUpdateException ex)
            {

                throw new Exception($"Um erro ocorreu enquanto a base de dados era atualizada: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro: {ex.Message}", ex);
            }

        }

        public async Task<UserModel> DeleteUserAsync(UserModel user)
        {
            /*
             * ATENÇÃO!
             * Exclui o usuário fisicamente do registro!
             * Prefira usar a exclusão lógica do registro!
             * 
             */


            try
            {
                _context.Users.Remove(user);

                //Se for igual a zero, significa que não conseguiu remover o usuário.
                if (await _context.SaveChangesAsync() == 0)
                {
                    throw new Exception("Não foi possível remover o usuário");
                }

                return user;
            }
            catch (DbUpdateException ex)
            {

                throw new Exception($"Um erro ocorreu enquanto a base de dados era atualizada: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro: {ex.Message}", ex);
            }

        }
    }
}
