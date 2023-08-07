using Discord;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services
{
    public class MessagesHandler
    {
        private readonly DiscordSocketClient _client;

        private readonly JsonConfigRepository _configRepository;

        public MessagesHandler(DiscordSocketClient client, JsonConfigRepository configRepository)
        {
            _client = client;
            _configRepository = configRepository;
        }

        public async Task DeleteMessagesFromTextChannelsAsync()
        {
            foreach (var guildPair in _configRepository.GuildConfigs)
            {
                var textChannels = guildPair.Value.DiscordChannels.OfType<TextChannel>().Where(x => x.IsDeleteMessages).ToList();
                foreach (var textChannel in textChannels)
                {
                    var channel = (ITextChannel)_client.Guilds.FirstOrDefault(x => x.Id == guildPair.Key).GetChannel(textChannel.Id);
                    await DeleteMessagesFromTextChannelAsync(channel, await channel.GetMessagesAsync().FlattenAsync(), textChannel.MessageAgeToDelete);
                }
            }
        }

        private async Task DeleteMessagesFromTextChannelAsync(ITextChannel channel, IEnumerable<IMessage> messages, int messageAgeToDelete)
        {
            var messageAge = new TimeSpan();
            var messagesToDelete = new List<IMessage>();
            foreach (var message in messages)
            {
                messageAge = DateTime.Now - message.CreatedAt;
                if (messageAge.Minutes < messageAgeToDelete)
                {
                    continue;
                }

                if (messageAge.Days <= 14)
                {
                    messagesToDelete.Add(message);
                }
                else
                {
                    await channel.DeleteMessageAsync(message);
                    await Task.Delay(600);
                }
            }
            await channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}
