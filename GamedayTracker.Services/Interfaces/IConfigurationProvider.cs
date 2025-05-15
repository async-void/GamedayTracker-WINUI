using GamedayTracker.Services.Enum;
using GamedayTracker.Services.Models;
using GamedayTracker.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamedayTracker.Services.Interfaces
{
    public interface IConfigurationProvider
    {
        Result<string, SystemError<ConfigurationProviderService>> GetConnectionString(ConnectionStringType type);
    }
}
