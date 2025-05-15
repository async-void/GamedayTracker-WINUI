using GamedayTracker.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GamedayTracker.Services.Services;

namespace GamedayTracker.Services.Interfaces
{
    public interface IGameData
    {
        Result<List<Matchup>, SystemError<GameDataProviderService>> GetScoreboard(int season, int week);
        Task<Result<List<string>, SystemError<GameDataProviderService>>> GetTeamSchedule(string teamName, int season);
        Result<int, SystemError<GameDataProviderService>> GetCurWeek();
        int GetMatchupCount(int season, int week);
    }
}
