using Discord.WebSocket;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Channels
{
    public class GroupChannel : DiscordChannel
    {
        private DefaultGroupChannelSettings _defaultGroupChannelSettings = new DefaultGroupChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultGroupChannelSettings").Bind(_defaultGroupChannelSettings);
        }
    }
}