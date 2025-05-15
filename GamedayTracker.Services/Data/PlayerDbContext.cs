using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Configuration.EntityTypeConfiguration;
using GamedayTracker.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace GamedayTracker.Services.Data
{
    public class PlayerDbContext(DbContextOptions<PlayerDbContext> options): DbContext(options)
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerPicks> Picks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PlayerEntityTypeConfiguration());
        }
    }
}
