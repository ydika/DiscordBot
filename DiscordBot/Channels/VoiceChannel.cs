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
    public class VoiceChannel : DiscordChannel
    {
        private DefaultVoiceChannelSettings _defaultVoiceChannelSettings = new DefaultVoiceChannelSettings();

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultVoiceChannelSettings").Bind(_defaultVoiceChannelSettings);
        }
    }
}