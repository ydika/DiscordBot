using Discord;
using DiscordBot.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services
{
    public class MessagesManager
    {
        private JsonConfigManager _configManager;

        public MessagesManager(JsonConfigManager configManager)
        {
            _configManager = configManager;
        }

        public async Task DeleteMessagesFromTextChannelsAsync()
        {
            foreach (var guildPair in _configManager.GuildConfigs)
            {
                var textChannels = guildPair.Value.DiscordChannels.OfType<TextChannel>().Where(x => x.IsDeleteMessages).ToList();
                foreach (var textChannel in textChannels)
                {
                    var channel = (ITextChannel)guildPair.Key.GetChannel(textChannel.Id);
                    await DeleteMessagesFromTextChannelAsync(channel, await channel.GetMessagesAsync().ToArrayAsync(), textChannel.MessageAgeToDelete);
                }
            }
        }

        private async Task DeleteMessagesFromTextChannelAsync(ITextChannel channel, IEnumerable<IMessage>[] messagePages, int messageAgeToDelete)
        {
            var messagesToDelete = new List<IMessage>();
            foreach (var messagePage in messagePages)
            {
                foreach (var message in messagePage)
                {
                    if ((DateTime.Now - message.CreatedAt).Minutes > TimeSpan.FromSeconds(messageAgeToDelete).Minutes)
                    {
                        messagesToDelete.Add(message);
                    }
                }
            }
            await channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}
