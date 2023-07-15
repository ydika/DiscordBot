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
    public class CategoryChannel : DiscordChannel
    {
        private DefaultCategoryChannelSettings _defaultCategoryChannelSettings = new DefaultCategoryChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultCategoryChannelSettings").Bind(_defaultCategoryChannelSettings);
        }
    }
}