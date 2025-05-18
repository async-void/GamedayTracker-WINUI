using GamedayTracker.Services.Enum;
using GamedayTracker.Services.Factories;
using GamedayTracker.Services.Interfaces;
using GamedayTracker.Services.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Extensions;

namespace GamedayTracker.Services.Services
{
    public class TeamDataServiceProvider(IConfigurationProvider configProvider) : ITeamData
    {
        #region GET TEAM STATS

        public async Task<Result<TeamStats, SystemError<TeamDataServiceProvider>>> GetTeamStatsAsync(int choice, int season, string teamName)
        {
            await using var db = new AppDbContextFactory(configProvider).CreateDbContext();
            HtmlNodeCollection? nodes = null;
            var link = "";
            var lineType = choice == 0 ? LineType.Offense : LineType.Defense;
            var web = new HtmlWeb();
            HtmlDocument? doc = null;
            List<TeamStats> statList;
            var textInfo = CultureInfo.CurrentCulture.TextInfo;

            switch (choice)
            {
                case 0:
                    teamName = textInfo.ToTitleCase(teamName);

                    statList = db.TeamStats.Where(x => x.Season == season && x.TeamName!.Equals(teamName) && x.LineType.Equals(lineType)).ToList();
                    if (statList.Count > 0)
                    {
                        return Result<TeamStats, SystemError<TeamDataServiceProvider>>.Ok(statList.First());
                    }

                    link = $"https://www.footballdb.com/statistics/nfl/team-stats/offense-totals/{season}/regular-season?sort=ydsgm";
                    doc = web.Load(link);
                    nodes = doc.DocumentNode.SelectNodes(".//table[contains(@class, 'statistics')]//tbody//tr");
                    break;
                case 1:
                    teamName = textInfo.ToTitleCase(teamName);

                    statList = db.TeamStats.Where(x => x.Season == season && x.TeamName!.Equals(teamName) && x.LineType.Equals(lineType)).ToList();
                    if (statList.Count > 0)
                    {
                        return Result<TeamStats, SystemError<TeamDataServiceProvider>>.Ok(statList.First());
                    }

                    link = $"https://www.footballdb.com/statistics/nfl/team-stats/defense-totals/{season}/regular-season?sort=ydsgm";
                    doc = web.Load(link);
                    nodes = doc.DocumentNode.SelectNodes(".//table[contains(@class, 'statistics')]//tbody//tr");
                    break;
                default:
                    break;
            }

            if (nodes is null) return Result<TeamStats, SystemError<TeamDataServiceProvider>>.Err(new SystemError<TeamDataServiceProvider>()
            {
                ErrorMessage = $"No stats found for {teamName}",
                ErrorType = ErrorType.INFORMATION,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = this
            });

            for (var i = 0; i < nodes.Count; i++)
            {
                var curNode = nodes[i];
                if (!curNode.HasChildNodes) continue;
                if (curNode.ChildNodes[0].ChildNodes[1].InnerText != teamName) continue;

                var name = curNode.ChildNodes[0].ChildNodes[1].InnerText;
                var gamesPlayed = curNode.ChildNodes[1].InnerText.ToInt();
                var totalPoints = curNode.ChildNodes[2].InnerText.ToInt();
                var pointsPerGame = curNode.ChildNodes[3].InnerText.Replace(",", string.Empty).ToDouble();
                var rushYards = curNode.ChildNodes[4].InnerText.Replace(",", string.Empty).ToInt();
                var rushYardsPerGame = curNode.ChildNodes[5].InnerText.Replace(",", string.Empty).ToDouble();
                var passYards = curNode.ChildNodes[6].InnerText.Replace(",", string.Empty).ToInt();
                var passYardsPerGame = curNode.ChildNodes[7].InnerText.Replace(",", string.Empty).ToDouble();
                var totalYards = curNode.ChildNodes[8].InnerText.Replace(",", string.Empty).ToInt();
                var yardsPerGame = curNode.ChildNodes[9].InnerText.Replace(",", string.Empty).ToDouble();

                var stats = new TeamStats()
                {
                    TeamName = name,
                    LineType = lineType,
                    Season = season,
                    GamesPlayed = gamesPlayed,
                    TotalYards = totalYards,
                    TotalPoints = totalPoints,
                    PassYardsPerGame = passYardsPerGame,
                    PassYardsTotal = passYards,
                    PointsPerGame = pointsPerGame,
                    RushPerGame = rushYardsPerGame,
                    RushYardsTotal = rushYards,
                    YardsPerGame = yardsPerGame
                };
                await db.TeamStats.AddAsync(stats);
                await db.SaveChangesAsync();
                return Result<TeamStats, SystemError<TeamDataServiceProvider>>.Ok(stats);
            }

            return Result<TeamStats, SystemError<TeamDataServiceProvider>>.Err(new SystemError<TeamDataServiceProvider>()
            {
                ErrorMessage = $"No stats found for {teamName}",
                ErrorType = ErrorType.INFORMATION,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = this
            });

        }

        #endregion
    }
}
