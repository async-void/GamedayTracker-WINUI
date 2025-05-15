using GamedayTracker.Services.Enum;
using GamedayTracker.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GamedayTracker.Services.Interfaces;

namespace GamedayTracker.Services.Services
{
    public class ConfigurationProviderService : IConfigurationProvider
    {
        public Result<string, SystemError<ConfigurationProviderService>> GetConnectionString(ConnectionStringType type)
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "config.json");
            var content = File.ReadAllText(configPath);
            var json = JsonSerializer.Deserialize<ConfigJson>(content);
            var conStr = "";

            conStr = type.ToString() == "Default" ? json!.ConnectionStrings!.Default : json!.ConnectionStrings!.Gameday;

            if (conStr != "")
                return Result<string, SystemError<ConfigurationProviderService>>.Ok(conStr!);

            return Result<string, SystemError<ConfigurationProviderService>>.Err(new SystemError<ConfigurationProviderService>
            {
                ErrorMessage = "Could not find config.json file",
                ErrorType = ErrorType.WARNING,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = this
            });
        }
    }
}
