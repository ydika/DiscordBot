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
    public class StoreChannel : DiscordChannel
    {
        private DefaultStoreChannelSettings _defaultStoreChannelSettings = new DefaultStoreChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultStoreChannelSettings").Bind(_defaultStoreChannelSettings);
        }
    }
}