using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamedayTracker.Services.Models
{
    public class Opponent
    {
        public int Id { get; set; }
        public required Team AwayTeam { get; set; }
        public required Team HomeTeam { get; set; }
        public int MatchUpId { get; set; }
        public Matchup Matchup { get; set; }
    }
}
