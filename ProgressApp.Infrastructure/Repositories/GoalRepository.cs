using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories;

public class GoalRepository :  IGoalRepository
{ 
    private readonly IServiceScopeFactory _scopeFactory;

    public GoalRepository(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<Goal?> GetActiveGoalAsync(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        return await context.Goals
            .AsNoTracking()
            .Include(g => g.Milestones.Where(m => m.Status == MilestoneStatus.Active))
            .ThenInclude(m => m.Actions)
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        await context.Goals.AddAsync(goal);
        await context.SaveChangesAsync();

        return goal;
    }   

    public async Task UpdateGoalAsync(Goal goal)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        context.Goals.Update(goal);
        await context.SaveChangesAsync();
    }

    public async Task<Milestone?> GetActiveMilestoneAsync(Guid goalId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        return await context.Milestones
            .AsNoTracking()
            .Include(m => m.Actions)
            .Where(m => m.GoalId == goalId && m.Status == MilestoneStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<Milestone> CreateMilestoneAsync(Milestone milestone)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

        await context.Milestones.AddAsync(milestone);
        await context.SaveChangesAsync();

        return milestone;
    }
}