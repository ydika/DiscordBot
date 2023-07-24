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
    public class StageChannel : TextChannel
    {
        private DefaultStageChannelSettings _defaultStageChannelSettings = new DefaultStageChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultStageChannelSettings").Bind(_defaultStageChannelSettings);
            base.SetDefaultValues(config);
        }
    }
}