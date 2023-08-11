using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.ConfigModels;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.CommandModules.GuildCommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class TextChannelSettingsCommandModule : InteractionModuleBase
    {
        private readonly JsonConfigRepository _jsonConfigRepository;

        public TextChannelSettingsCommandModule(JsonConfigRepository jsonConfigRepository)
        {
            _jsonConfigRepository = jsonConfigRepository;
        }

        [CallLimit(2, 60)]
        [SlashCommand("broom-off", "stops deleting messages on the channel periodically")]
        public async Task BroomOffCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, false, -1);
            await RespondAsync(embed: new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"Broom for channel *{Context.Channel.Name}* turned off!"
            }.Build());
        }

        [CallLimit(2, 60)]
        [SlashCommand("broom-on", "starts deleting messages on the channel periodically")]
        public async Task BroomOnCommand()
        {
            await SetTextChannelSettings((SocketGuildChannel)Context.Channel, true, -1);
            await RespondAsync(embed: new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"Broom for channel *{Context.Channel.Name}* turned on!"
            }.Build());
        }

        [CallLimit(2, 60)]
        [SlashCommand("ch-config", "returns channel config")]
        public async Task ChannelConfigCommand()
        {
            var channelConfig = await _jsonConfigRepository.GetTextChannelConfigAsync((SocketGuildChannel)Context.Channel);
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"*{channelConfig.Name}* Channel Config"
            };

            foreach (var property in channelConfig.GetType().GetProperties())
            {
                if (property.DeclaringType != typeof(DiscordChannel))
                {
                    embed.AddField(property.Name, property.GetValue(channelConfig));
                }
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [CallLimit(2, 60)]
        [SlashCommand("ch-configs", "returns all channel configs")]
        public async Task ChannelConfigsCommand()
        {
            var channelConfigs = await _jsonConfigRepository.GetTextChannelConfigsAsync((SocketGuildChannel)Context.Channel);
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"All Channel Configs"
            };

            var properties = new StringBuilder(256);
            foreach (var channelConfig in channelConfigs)
            {
                foreach (var property in channelConfig.GetType().GetProperties())
                {
                    if (property.DeclaringType != typeof(DiscordChannel))
                    {
                        properties.Append($"{property.Name} : {property.GetValue(channelConfig)}\n");
                    }
                }

                embed.AddField(channelConfig.Name, properties, true);
                properties.Clear();
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [CallLimit(2, 60)]
        [SlashCommand("message-age", "sets the age of the message in minutes after which it will be deleted")]
        public async Task MessageAgeCommand([MinValue(1)] int minutes)
        {
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
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
            var channelConfig = await _jsonConfigRepository.GetTextChannelConfigAsync(channel);

            if (messageAge > 0)
            {
                channelConfig.MessageAgeToDelete = messageAge;
            }
            else
            {
                channelConfig.IsDeleteMessages = isDeleteMessages;
            }

            await _jsonConfigRepository.UpdateChannelInConfigFileAsync(channel, channelConfig);
        }
    }
}
