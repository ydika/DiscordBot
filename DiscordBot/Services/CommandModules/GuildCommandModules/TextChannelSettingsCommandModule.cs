using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services.CommandModules.GuildCommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class TextChannelSettingsCommandModule : InteractionModuleBase
    {
        private JsonConfigManager _jsonConfigManager;

        public TextChannelSettingsCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [SlashCommand("broom-off", "stops deleting messages on the channel periodically")]
        public async Task BroomOffCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, -1);
            await RespondAsync(embed: new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"Broom for channel *{Context.Channel.Name}* turned off!"
            }.Build());
        }

        [SlashCommand("broom-on", "starts deleting messages on the channel periodically")]
        public async Task BroomOnCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, true, -1);
            await RespondAsync(embed: new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"Broom for channel *{Context.Channel.Name}* turned on!"
            }.Build());
        }

        [SlashCommand("ch-config", "returns channel config")]
        public async Task ChannelConfigCommand()
        {
            var channelConfig = await _jsonConfigManager.GetTextChannelConfigAsync((SocketGuildChannel)Context.Channel);
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"*{channelConfig.Name}* Channel Config"
            };

            foreach (var property in channelConfig.GetType().GetProperties())
            {
                if (property.DeclaringType == channelConfig.GetType())
                {
                    embed.AddField($"{property.Name}", $"{property.GetValue(channelConfig)}");
                }
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("ch-configs", "returns all channel configs")]
        public async Task ChannelConfigsCommand()
        {
            var channelConfigs = await _jsonConfigManager.GetTextChannelConfigsAsync((SocketGuildChannel)Context.Channel);
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"All Channel Configs"
            };

            var properties = new StringBuilder(256);
            foreach (var channelConfig in channelConfigs)
            {
                foreach (var property in channelConfig.GetType().GetProperties())
                {
                    if (property.DeclaringType == channelConfig.GetType())
                    {
                        properties.Append($"{property.Name} : {property.GetValue(channelConfig)}\n");
                    }
                }

                embed.AddField($"{channelConfig.Name}", $"{(!string.IsNullOrEmpty(properties.ToString()) ? properties : "default config missing")}", true);
                properties.Clear();
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("message-age", "sets the age of the message in minutes after which it will be deleted")]
        public async Task MessageAgeCommand([MinValue(1)]int minutes)
        {
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            if (minutes <= 0)
            {
                embed.Title = $"Message age of *{minutes}* minutes cannot be less than or equal to zero!";
                await RespondAsync(embed: embed.Build());
                return;
            }

            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, minutes);

            embed.Title = $"Message age of *{minutes}* minutes for channel *{Context.Channel.Name}* is set!";
            await RespondAsync(embed: embed.Build());
        }

        private async Task SetTextChannelSettings(SocketGuildChannel channel, bool isDeleteMessages, int messageAge)
        {
            var channelConfig = await _jsonConfigManager.GetTextChannelConfigAsync(channel);

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
