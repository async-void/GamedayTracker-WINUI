﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GamedayTracker.Services.Models
{
    public class ConfigJson
    {
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStrings? ConnectionStrings { get; set; }
    }
}
