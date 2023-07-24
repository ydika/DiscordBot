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
    public class NewsThreadChannel : TextChannel
    {
        private DefaultNewsThreadChannelSettings _defaultNewsThreadChannelSettings = new DefaultNewsThreadChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultNewsThreadChannelSettings").Bind(_defaultNewsThreadChannelSettings);
            base.SetDefaultValues(config);
        }
    }
}