using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ConfigModels
{
    public class TextChannel : DiscordChannel
    {
        public bool IsDeleteMessages { get; set; }
        public int MessageAgeToDelete { get; set; }

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultTextChannelSettings").Bind(this);
        }
    }
}
