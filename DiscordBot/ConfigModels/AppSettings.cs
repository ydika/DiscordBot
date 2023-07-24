using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ConfigModels
{
    public class AppSettings
    {
        public string CommandPrefix { get; set; }
        public string GuildConfigsPath { get; set; } 
        public string Token { get; set; }
    }
}
