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
    public class ForumChannel : DiscordChannel
    {
        private DefaultForumChannelSettings _defaultForumChannelSettings = new DefaultForumChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultForumChannelSettings").Bind(_defaultForumChannelSettings);
        }
    }
}