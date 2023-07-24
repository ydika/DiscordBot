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
    public class NewsChannel : TextChannel
    {
        private DefaultNewsChannelSettings _defaultNewsChannelSettings = new DefaultNewsChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultNewsChannelSettings").Bind(_defaultNewsChannelSettings);
            base.SetDefaultValues(config);
        }
    }
}