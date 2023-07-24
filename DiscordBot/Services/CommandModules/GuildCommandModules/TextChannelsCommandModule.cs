using Discord;
using Discord.Commands;
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

        [Command("get_channel_settings")]
        [Summary("returns channel settings")]
        public async Task GetChannelSettingsCommand()
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

            await ReplyAsync(properties.ToString());
        }

        [Command("get_allchannel_settings")]
        [Summary("returns all channel settings")]
        public async Task GetAllChannelSettingsCommand()
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

            await ReplyAsync(properties.ToString());
        }

        [Command("delete_all_messages")]
        [Summary("deletes all messages on the channel")]
        public async Task DeleteAllMessagesCommand()
        {
            var textChannel = (ITextChannel)Context.Channel;
            var messages = await textChannel.GetMessagesAsync().FlattenAsync();
            await textChannel.DeleteMessagesAsync(messages);
        }

        [Command("turn_on_broom")]
        [Summary("starts deleting messages on the channel periodically")]
        public async Task TurnOnBroomCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, true, -1);
            await ReplyAsync($"Broom for channel **{Context.Channel.Name}** turned on!");
        }

        [Command("turn_off_broom")]
        [Summary("stops deleting messages on the channel periodically")]
        public async Task TurnOfBroomCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, -1);
            await ReplyAsync($"Broom for channel **{Context.Channel.Name}** turned off!");
        }

        [Command("set_message_age")]
        [Summary("sets the age of the message in minutes after which it will be deleted (cannot be less than or equal to zero)")]
        public async Task SetMessageAgeCommand(int minutes)
        {
            if (minutes <= 0)
            {
                await ReplyAsync($"Message age of **{minutes}** minutes cannot be less than or equal to zero!");
                return;
            }

            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, minutes);
            await ReplyAsync($"Message age of **{minutes}** minutes for channel **{Context.Channel.Name}** is set!");
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
