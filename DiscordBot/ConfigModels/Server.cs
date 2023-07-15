using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Channels;

namespace DiscordBot.ConfigModels
{
    public class Server
    {
        public ulong ServerId { get; set; }
        public string Name { get; set; }
        public List<DiscordChannel> DiscordChannels { get; set; }
    }
}
