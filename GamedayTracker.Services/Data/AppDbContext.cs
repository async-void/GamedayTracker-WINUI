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
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamPlayer> TeamPlayers { get; set; }
        public DbSet<Opponent> Opponents { get; set; }
        public DbSet<Matchup> Matchups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TeamEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TeamPlayerEntityTypeConfiguration());
        }
    }
}
