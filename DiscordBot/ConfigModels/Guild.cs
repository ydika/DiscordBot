using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.ConfigModels
{
    public class Guild
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public uint EmbedColor { get; set; }
        public List<DiscordChannel> DiscordChannels { get; set; }

        public void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultGuildSettings").Bind(this);
        }
    }
}
