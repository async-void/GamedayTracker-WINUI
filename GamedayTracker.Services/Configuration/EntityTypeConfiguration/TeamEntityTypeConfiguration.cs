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
    public class TeamEntityTypeConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.Property(x => x.Abbreviation).HasMaxLength(12);
            builder.Property(x => x.Name).HasMaxLength(50);
            builder.Property(x => x.Division).HasMaxLength(50);
            builder.Property(x => x.Record).HasMaxLength(6);
            builder.Property(x => x.LogoPath).HasMaxLength(200);
            builder.Property(x => x.Score).HasDefaultValue(10);
        }
    }
}
