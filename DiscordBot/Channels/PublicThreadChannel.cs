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
    public class PublicThreadChannel : DiscordChannel
    {
        private DefaultPublicThreadChannelSettings _defaultPublicThreadChannelSettings = new DefaultPublicThreadChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultPublicThreadChannelSettings").Bind(_defaultPublicThreadChannelSettings);
        }
    }
}