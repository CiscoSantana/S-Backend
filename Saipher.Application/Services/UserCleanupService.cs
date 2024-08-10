using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saipher.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Application.Services
{
    public class UserCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public UserCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await InactivateOldUsersAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Executa a cada minuto apenas para teste
            }
        }

        private async Task InactivateOldUsersAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var usersToInactivate = await context.Users
                    .Where(u => u.CreateAt <= DateTime.UtcNow.AddDays(-30) && u.IsEnabled)//Subtrai 30 dias da data atual e verifica se a data do usuário é igual ou anterior 
                    .ToListAsync();

                foreach (var user in usersToInactivate)
                {
                    user.IsEnabled = false;
                    user.DeletedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
