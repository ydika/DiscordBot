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
    public class DMChannel : DiscordChannel
    {
        private DefaultDMChannelSettings _defaultDMChannelSettings = new DefaultDMChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultDMChannelSettings").Bind(_defaultDMChannelSettings);
        }
    }
}
