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
    public class PrivateThreadChannel : TextChannel
    {
        private DefaultPrivateThreadChannelSettings _defaultPrivateThreadChannelSettings = new DefaultPrivateThreadChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultPrivateThreadChannelSettings").Bind(_defaultPrivateThreadChannelSettings);
            base.SetDefaultValues(config);
        }
    }
}