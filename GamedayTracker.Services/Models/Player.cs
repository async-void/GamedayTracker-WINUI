using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamedayTracker.Services.Models
{
    public class Player
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Company { get; set; }
    }
}
