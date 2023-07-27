using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services.CommandModules.GuildCommandModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services.BotFunctionality.CommandModules
{
    public class TextChannelsCommandModule : GeneralGuildCommandModule
    {
        private JsonConfigManager _jsonConfigManager;

        public TextChannelsCommandModule(JsonConfigManager jsonConfigManager) : base(jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [SlashCommand("settings", "returns channel settings")]
        public async Task SettingsCommand()
        {
            var channelConfig = await GetTextChannelConfigAsync((SocketGuildChannel)Context.Channel);
            var properties = new StringBuilder(256).Append($"> # {channelConfig.Name} Channel Settings\n");
            foreach (var property in channelConfig.GetType().GetProperties())
            {
                if (property.DeclaringType == channelConfig.GetType())
                {
                    properties.Append($"> {property.Name} : {property.GetValue(channelConfig)}\n");
                }
            }

            await RespondAsync(properties.ToString());
        }

        [SlashCommand("all-settings", "returns all channel settings")]
        public async Task AllSettingsCommand()
        {
            var channelConfigs = await GetTextChannelConfigsAsync((SocketGuildChannel)Context.Channel);
            var properties = new StringBuilder(256);
            foreach (var channelConfig in channelConfigs)
            {
                properties.Append($"> # {channelConfig.Name} Channel Settings\n");
                foreach (var property in channelConfig.GetType().GetProperties())
                {
                    if (property.DeclaringType == channelConfig.GetType())
                    {
                        properties.Append($"> {property.Name} : {property.GetValue(channelConfig)}\n");
                    }
                } 
            }

            await RespondAsync(properties.ToString());
        }

        [SlashCommand("clear", "deletes all messages on the channel")]
        public async Task ClearCommand()
        {
            var textChannel = (ITextChannel)Context.Channel;
            var messages = await textChannel.GetMessagesAsync().FlattenAsync();
            await textChannel.DeleteMessagesAsync(messages);

            var respondMessage = "All messages have been deleted!\n*This message will be deleted in 10 seconds*";
            await RespondAsync(respondMessage);
            messages = await textChannel.GetMessagesAsync().FlattenAsync();

            await Task.Delay(10000);
            await messages.FirstOrDefault(x => x.Author.IsBot && x.Content == respondMessage).DeleteAsync();
        }

        [SlashCommand("broom-on", "starts deleting messages on the channel periodically")]
        public async Task BroomOnCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, true, -1);
            await RespondAsync($"Broom for channel **{Context.Channel.Name}** turned on!");
        }

        [SlashCommand("broom-off", "stops deleting messages on the channel periodically")]
        public async Task BroomOffCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, -1);
            await RespondAsync($"Broom for channel **{Context.Channel.Name}** turned off!");
        }

        [SlashCommand("message-age", "sets the age of the message in minutes after which it will be deleted")]
        public async Task MessageAgeCommand(int minutes)
        {
            if (minutes <= 0)
            {
                await RespondAsync($"Message age of **{minutes}** minutes cannot be less than or equal to zero!");
                return;
            }

            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, minutes);
            await RespondAsync($"Message age of **{minutes}** minutes for channel **{Context.Channel.Name}** is set!");
        }

        private async Task SetTextChannelSettings(SocketGuildChannel channel, bool isDeleteMessages, int messageAge)
        {
            var channelConfig = await GetTextChannelConfigAsync(channel);

            if (messageAge > 0)
            {
                channelConfig.MessageAgeToDelete = messageAge;
            }
            else
            {
                channelConfig.IsDeleteMessages = isDeleteMessages;
            }

            await _jsonConfigManager.UpdateChannelInConfigFileAsync(channel, channelConfig);
        }
    }
}
