using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamedayTracker.Services.Configuration.EntityTypeConfiguration
{
    public class TeamPlayerEntityTypeConfiguration : IEntityTypeConfiguration<TeamPlayer>
    {
        public void Configure(EntityTypeBuilder<TeamPlayer> builder)
        {
            builder.Property(x => x.Age).HasMaxLength(10);
            builder.Property(x => x.College).HasMaxLength(50);
            builder.Property(x => x.Height).HasMaxLength(10);
            builder.Property(x => x.Number).HasMaxLength(10);
            builder.Property(x => x.PlayerName).HasMaxLength(50);
            builder.Property(x => x.Position).HasMaxLength(10);
            builder.Property(x => x.TeamName).HasMaxLength(100);
            builder.Property(x => x.Weight).HasMaxLength(10);
        }
    }
}
