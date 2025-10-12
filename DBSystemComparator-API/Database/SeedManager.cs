using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DBSystemComparator_API.Models.Entities;

namespace DBSystemComparator_API.Database
{
    public static class SeedManager
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await CreateLanguages(context);
        }

        public static async Task CreateLanguages(ApplicationDbContext context)
        {
            //foreach (var language in DBLanguages.All)
            //{
            //    if (!await context.Languages.AnyAsync(a => a.Code == language.Code))
            //    {
            //        await context.Languages.AddAsync(language);
            //    }
            //}
            //await context.SaveChangesAsync();
        }
    }
}