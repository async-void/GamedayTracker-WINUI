using GamedayTracker.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Services;

namespace GamedayTracker.Services.Interfaces
{
    public interface ITeamData
    {
        Task<Result<TeamStats, SystemError<TeamDataServiceProvider>>> GetTeamStatsAsync(int choice, int season, string teamName);
    }
}
