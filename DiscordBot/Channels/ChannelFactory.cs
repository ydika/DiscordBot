using Discord;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Channels
{
    public class ChannelFactory
    {
        private readonly IConfigurationRoot _config;

        public ChannelFactory(IConfigurationRoot config)
        {
            _config = config;
        }

        public IEnumerable<DiscordChannel> CreateChannels(IEnumerable<SocketGuildChannel> channels)
        {
            var discordChannels = new List<DiscordChannel>();

            foreach (var channel in channels)
            {
                var channelType = (ChannelType)channel.GetChannelType();
                if (ChannelDictionary.ChannelTypeMap.TryGetValue(channelType, out var channelClass))
                {
                    discordChannels.Add(CreateChannel(channelClass, channel));
                }
                else
                {
                    throw new NotSupportedException($"Channel type \"{channelType}\" not supported.");
                }
            }

            return discordChannels;
        }

        private DiscordChannel CreateChannel(Type channelClass, SocketGuildChannel channel)
        {
            var discordChannel = (DiscordChannel)Activator.CreateInstance(channelClass);
            discordChannel.Id = channel.Id;
            discordChannel.Name = channel.Name;
            discordChannel.Type = (ChannelType)channel.GetChannelType();
            discordChannel.SetDefaultValues(_config);
            return discordChannel;
        }
    }
}
