using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.User;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IServiceScopeFactory _scopeFactory;
    public UserRepository(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<User?> GetUserAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
        return await context.Users.AsNoTracking().FirstOrDefaultAsync();
    }
    public async Task<User> CreateUserAsync(User user)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}