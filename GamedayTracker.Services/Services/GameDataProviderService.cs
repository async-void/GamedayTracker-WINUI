﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GamedayTracker.Services.Enum;
using System.Xml;
using GamedayTracker.Services.Extensions;
using GamedayTracker.Services.Interfaces;
using GamedayTracker.Services.Models;
using HtmlAgilityPack;
using ChalkDotNET;
using GamedayTracker.Services.Factories;
using Microsoft.EntityFrameworkCore;

namespace GamedayTracker.Services.Services
{
    public class GameDataProviderService(IConfigurationProvider dataProvider) : IGameData
    {
        private readonly AppDbContextFactory _dbFactory = new(dataProvider);

        #region GET CURRENT WEEK
        public Result<int, SystemError<GameDataProviderService>> GetCurWeek()
        {
            const string link = "https://www.footballdb.com/scores/index.html";
            var web = new HtmlWeb();
            var doc = web.Load(link);
            var weekNode = doc.DocumentNode.SelectSingleNode(".//h2");
            var week = weekNode?.InnerText;
            var weekResult = int.TryParse(week, out var wResult);

            if (weekResult)
            {
                return Result<int, SystemError<GameDataProviderService>>.Ok(wResult);
            }
            return Result<int, SystemError<GameDataProviderService>>.Err(new SystemError<GameDataProviderService>
            {
                ErrorMessage = "Unable to get current week",
                ErrorType = ErrorType.INFORMATION,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = this
            });
        }

        #endregion

        /// <summary>
        /// Get Scoreboard for season and week
        /// </summary>
        /// <param name="season"></param>
        /// <param name="week"></param>
        /// <returns>List Matchup</returns>
        #region GET SCOREBOARD
        public Result<List<Matchup>, SystemError<GameDataProviderService>> GetScoreboard(int season, int week)
        {
            var matchups = new List<Matchup>();
            var db = _dbFactory.CreateDbContext();
            const string seasonType = "reg";
            var link = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type={seasonType}&wk={week}";
            var sw = new Stopwatch();
            sw.Start();
            var weeklyMatchups = db.Matchups?
                .Where(x => x.Season == season && x.Week == week)
                .Include(x => x.Opponents)
                .Include(x => x.Opponents.AwayTeam)
                .Include(x => x.Opponents.HomeTeam)
                .AsSplitQuery()
            .AsQueryable();
            sw.Stop();

            Console.WriteLine($"Query Time: {sw.ElapsedMilliseconds}ms");

            if (weeklyMatchups!.Any())
            {
                return Result<List<Matchup>, SystemError<GameDataProviderService>>.Ok(weeklyMatchups!.ToList());
            }

            Console.WriteLine($"no matchups in database for Season {season} : Week {week}\r\nAttempting to collect data from website!");


            if (week > 18)
            {
                switch (week)
                {
                    case 19:
                        link = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type=post&wk=1";
                        break;
                    case 20:
                        link = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type=post&wk=2";
                        break;
                    case 21:
                        link = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type=post&wk=3";
                        break;
                    case 22:
                        link = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type=post&wk=4";
                        break;
                    default:
                        return Result<List<Matchup>, SystemError<GameDataProviderService>>.Err(new SystemError<GameDataProviderService>
                        {
                            ErrorMessage = $"no results for season: {season} week: {week}",
                            ErrorType = ErrorType.INFORMATION,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = this
                        });
                }

            }

            var web = new HtmlWeb();
            var doc = web.Load(link);

            var scoreboardNodes = doc.DocumentNode.SelectNodes(".//div[@class='lngame']//table");

            if (scoreboardNodes is null)
                return Result<List<Matchup>, SystemError<GameDataProviderService>>.Err(new SystemError<GameDataProviderService>
                {
                    ErrorMessage = $"no matchups found for Season [{season}] Week [{week}]",
                    ErrorType = ErrorType.WARNING,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this
                });
            for (var i = 0; i <= scoreboardNodes.Count - 1; i++)
            {
                var node = scoreboardNodes[i].ChildNodes[3];
                if (!node.HasChildNodes) continue;
                try
                {
                    var matchup = ParseMatchup(node, season, week);

                    if (matchup.IsOk)
                        matchups.Add(matchup.Value);
                    else
                    {
                        Console.WriteLine($"{Chalk.Red("[ERROR]")} {Chalk.DarkGray(matchup.Error.ErrorMessage!)}");
                    }
                }
                catch (Exception e)
                {
                    var error = new SystemError<GameDataProviderService>()
                    {
                        ErrorMessage = e.Message,
                        CreatedBy = this,
                        CreatedAt = DateTime.UtcNow,
                        ErrorType = ErrorType.INFORMATION,
                    };
                    Console.WriteLine($"{Chalk.Red("[ERROR]")} {Chalk.DarkGray(error.ErrorMessage)}");
                }
            }
            Console.WriteLine($"{Chalk.Green("Week [")}{Chalk.Yellow($"{week}")}{Chalk.Green("] Complete...")}");

            db.AddRange(matchups);
            db.SaveChanges();

            return Result<List<Matchup>, SystemError<GameDataProviderService>>.Ok(matchups);

        }
        #endregion

        /// <summary>
        /// Parse Matchup Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="season"></param>
        /// <param name="week"></param>
        /// <returns>Matchup</returns>
        #region PARSE MATCHUP

        public Result<Matchup, SystemError<GameDataProviderService>> ParseMatchup(HtmlNode node, int season, int week)
        {
            try
            {
                var awayNode = node.ChildNodes[1];
                var awayName = awayNode.ChildNodes[1].InnerText;
                var homeNode = node.ChildNodes[3];
                var homeName = homeNode.ChildNodes[1].InnerText;
                var awayMatch = Regex.Match(awayName, """(?<name>[\w\s]+) (?<rec>\(\d+-\d+\))""");
                var homeMatch = Regex.Match(homeName, """(?<name>[\w\s]+) (?<rec>\(\d+-\d+\))""");

                var awayNameFinal = awayMatch.Groups["name"];
                var awayAbbr = awayNameFinal.Value.ToAbbr();
                var awayRecord = awayMatch.Groups["rec"];

                var homeNameFinal = homeMatch.Groups["name"];
                var homeAbbr = homeNameFinal.Value.ToAbbr();
                var homeRecord = homeMatch.Groups["rec"];

                var awayScore = awayNode.LastChild.InnerText;
                var homeScore = homeNode.LastChild.InnerText;

                var awayTeam = new Team()
                {
                    Name = awayNameFinal.Value,
                    Score = int.Parse(awayScore),
                    Record = awayRecord.Value,
                    Division = awayNameFinal.Value.ToDivision(),
                    Abbreviation = awayAbbr,
                    LogoPath = LogoPathProviderService.GetLogoPath(awayAbbr)
                };

                var homeTeam = new Team()
                {
                    Name = homeNameFinal.Value,
                    Score = int.Parse(homeScore),
                    Record = homeRecord.Value,
                    Division = homeNameFinal.Value.ToDivision(),
                    Abbreviation = homeAbbr,
                    LogoPath = LogoPathProviderService.GetLogoPath(homeAbbr)
                };
                var matchup = new Matchup()
                {
                    Week = week,
                    Season = season,
                    Opponents = new Opponent { AwayTeam = awayTeam, HomeTeam = homeTeam }
                };


                return Result<Matchup, SystemError<GameDataProviderService>>.Ok(matchup);
            }
            catch (Exception)
            {
                var error = new SystemError<GameDataProviderService>
                {
                    ErrorType = ErrorType.INFORMATION,
                    ErrorMessage = "An Error occured while parsing the matchup data!",
                    CreatedBy = this,
                    CreatedAt = DateTime.UtcNow,
                };
                return Result<Matchup, SystemError<GameDataProviderService>>.Err(error);
            }
        }

        #endregion

        /// <summary>
        /// get the matchup count for a specific week
        /// </summary>
        /// <param name="season"></param>
        /// <param name="week"></param>
        /// <returns>int</returns>
        #region GET MATCHUP COUNT

        public int GetMatchupCount(int season, int week)
        {
            var mainLink = $"https://www.footballdb.com/scores/index.html?lg=NFL&yr={season}&type=reg&wk={week}";
            var web = new HtmlWeb();
            var doc = web.Load(mainLink);
            var scoreNodes = doc.DocumentNode.SelectNodes(".//div[@class='lngame']//table");

            return scoreNodes?.Count ?? 0;
        }

        #endregion

        /// <summary>
        /// get the team schedule
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="season"></param>
        /// <returns>Task</returns>
        #region GET TEAM SCHEDULE
        public async Task<Result<List<string>, SystemError<GameDataProviderService>>> GetTeamSchedule(string teamName, int season)
        {
            var scheduleList = new List<string>();
            var teamLinkName = teamName.ToLower().ToTeamLinkName();
            var scheduleLink = $"https://www.footballdb.com/teams/nfl/{teamLinkName}/results/{season}";
            var web = new HtmlWeb();
            var doc = web.Load(scheduleLink);
            var scheduleNodes = doc.DocumentNode.SelectNodes(".//div[@class='games-container']");

            if (scheduleNodes is null)
                return Result<List<string>, SystemError<GameDataProviderService>>.Err(new SystemError<GameDataProviderService>
                {
                    ErrorMessage = $"no schedule found for ``{teamName}``",
                    ErrorType = ErrorType.INFORMATION,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this
                });

            HtmlNode? curNode = null;

            curNode = scheduleNodes.Count == 2 ? scheduleNodes[0] : scheduleNodes[1];

            for (var i = 1; i < curNode.ChildNodes.Count; i += 2)
            {
                var date = curNode.ChildNodes[i].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText;
                var awayName = curNode.ChildNodes[i].ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[0].InnerText;
                var homeName = curNode.ChildNodes[i].ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[3].ChildNodes[1].ChildNodes[0].InnerText;
                if (!string.Equals(awayName.ToLower(), teamName.ToLower(), StringComparison.Ordinal))
                    scheduleList.Add($"{awayName} - {date}");
                if (!string.Equals(homeName.ToLower(), teamName.ToLower(), StringComparison.Ordinal))
                    scheduleList.Add($"{homeName} - {date}");
            }
            return Result<List<string>, SystemError<GameDataProviderService>>.Ok(scheduleList);
        }

        #endregion
    }
}
