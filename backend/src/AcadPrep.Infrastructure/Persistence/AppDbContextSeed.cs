using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // Stub for database seeding
        await Task.CompletedTask;
    }
}
