using Discord;
using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigManagers;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.BotFunctionality.MessageManagement
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
            var timer = new Timer(async callback =>
            {
                foreach (var guildPair in _configManager.GuildConfigs)
                {
                    var textChannels = guildPair.Value.DiscordChannels.OfType<TextChannel>().Where(x => x.IsDeleteMessages).ToList();
                    foreach (var textChannel in textChannels)
                    {
                        var channel = (IMessageChannel)guildPair.Key.GetChannel(textChannel.Id);
                        await DeleteMessagesFromTextChannelsAsync(await channel.GetMessagesAsync().ToArrayAsync(), textChannel);
                    }
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            await Task.Delay(Timeout.Infinite);
        }

        private async Task DeleteMessagesFromTextChannelsAsync(IEnumerable<IMessage>[] messagePages, TextChannel textChannel)
        {
            foreach (var messagePage in messagePages)
            {
                foreach (var message in messagePage)
                {
                    if ((DateTime.Now - message.CreatedAt).Minutes > textChannel.MessageAgeToDelete)
                    {
                        await message.DeleteAsync();
                    }
                }
            }
        }
    }
}
