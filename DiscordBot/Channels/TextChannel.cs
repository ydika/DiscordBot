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
    public class TextChannel : DiscordChannel
    {
        private DefaultTextChannelSettings _defaultTextChannelSettings = new DefaultTextChannelSettings();

        public bool IsDeleteMessages { get; set; }
        public int MessageAgeToDelete { get; set; }
        public int RemovalFrequency { get; set; }

        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultTextChannelSettings").Bind(_defaultTextChannelSettings);

            IsDeleteMessages = _defaultTextChannelSettings.IsDeleteMessages;
            MessageAgeToDelete = _defaultTextChannelSettings.MessageAgeToDelete;
            RemovalFrequency = _defaultTextChannelSettings.RemovalFrequency;
        }
    }
}
