using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Channels;

namespace DiscordBot.ConfigModels
{
    public class Guild
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public List<DiscordChannel> DiscordChannels { get; set; }
    }
}
