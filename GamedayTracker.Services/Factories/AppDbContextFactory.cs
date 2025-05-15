using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Data;
using GamedayTracker.Services.Enum;
using GamedayTracker.Services.Interfaces;
using GamedayTracker.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace GamedayTracker.Services.Factories
{
    public class AppDbContextFactory(IConfigurationProvider dataProvider) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            var result = dataProvider.GetConnectionString(ConnectionStringType.Default);
            if (result.IsOk)
            {
                var conStr = result.Value;
                var options = new DbContextOptionsBuilder<AppDbContext>();
                options.UseNpgsql(conStr);
                return new AppDbContext(options.Options);
            }
            else
            {
                return null;
            }
        }
    }
}
