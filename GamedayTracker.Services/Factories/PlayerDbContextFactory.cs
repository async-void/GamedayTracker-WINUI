using GamedayTracker.Services.Data;
using GamedayTracker.Services.Enum;
using GamedayTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace GamedayTracker.Services.Factories
{
    public class PlayerDbContextFactory(IConfigurationProvider dataProvider) : IDbContextFactory<PlayerDbContext>
    {
        public PlayerDbContext CreateDbContext()
        {
            var result = dataProvider.GetConnectionString(ConnectionStringType.Gameday);
            if (result.IsOk)
            {
                var conStr = result.Value;
                var options = new DbContextOptionsBuilder<PlayerDbContext>();
                options.UseNpgsql(conStr);
                return new PlayerDbContext(options.Options);
            }
            else
            {
                return null;
            }
        }
    }
}
