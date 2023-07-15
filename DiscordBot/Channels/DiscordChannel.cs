using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Channels
{
    public abstract class DiscordChannel
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ChannelType Type { get; set; }

        public abstract void SetDefaultValues(IConfigurationRoot config);
    }
}
