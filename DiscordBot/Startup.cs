﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Startup
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;

        private readonly AppSettings _appSettings;
        private readonly JsonConfigRepository _configRepository;
        private readonly MessagesHandler _messagesHandler;

        public Startup(IServiceProvider services, DiscordSocketClient client, InteractionService interactions,
            AppSettings appSettings, JsonConfigRepository configRepository, MessagesHandler messagesHandler)
        {
            _services = services;
            _client = client;
            _interactions = interactions;
            _appSettings = appSettings;
            _configRepository = configRepository;
            _messagesHandler = messagesHandler;
        }

        public async Task RunAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            RegisterInteractionsEvents();
            RegisterClientEvents();

            _ = Task.Run(() => PeriodicallyDeleteMessages());

            await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
            await _client.StartAsync();
        }

        private async Task PeriodicallyDeleteMessages(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _messagesHandler.DeleteMessagesFromTextChannelsAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private void RegisterInteractionsEvents()
        {
            _interactions.SlashCommandExecuted += SlashCommandExecuted;
        }

        private async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            var embed = new EmbedBuilder()
            {
                Color = (await _configRepository.GetGuildConfigAsync((SocketGuild)context.Guild)).EmbedColor,
                Title = result.ErrorReason
            };

            if (!result.IsSuccess)
            {
                await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
            }
        }

        private void RegisterClientEvents()
        {
            _client.ChannelCreated += ChannelCreated;
            _client.ChannelDestroyed += ChannelDestroyed;
            _client.ChannelUpdated += ChannelUpdated;
            _client.GuildUpdated += GuildUpdated;
            _client.InteractionCreated += HandleInteractionAsync;
            _client.JoinedGuild += JoinedGuild;
            _client.LeftGuild += LeftGuild;
            _client.Log += Log;
            _client.Ready += Ready;
            _client.ThreadCreated += ThreadCreated;
            _client.ThreadDeleted += ThreadDeleted;
            _client.ThreadUpdated += ThreadUpdated;
        }

        private async Task ChannelCreated(SocketChannel channel)
        {
            await _configRepository.AddChannelToConfigFileAsync((SocketGuildChannel)channel);
        }

        private async Task ChannelDestroyed(SocketChannel channel)
        {
            await _configRepository.DeleteChannelFromConfigFileAsync((SocketGuildChannel)channel);
        }

        private async Task ChannelUpdated(SocketChannel oldChannel, SocketChannel updatedChannel)
        {
            var guildChannel = (SocketGuildChannel)updatedChannel;
            var guildConfig = await _configRepository.GetGuildConfigAsync(guildChannel.Guild);
            var channelConfig = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == oldChannel.Id);
            if (channelConfig is null)
            {
                return;
            }

            channelConfig.Name = guildChannel.Name;

            await _configRepository.UpdateChannelInConfigFileAsync((SocketGuildChannel)oldChannel, channelConfig);
        }

        private async Task GuildUpdated(SocketGuild oldGuild, SocketGuild updatedGuild)
        {
            await _configRepository.UpdateGuildInConfigFileAsync(updatedGuild);
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            await _interactions.ExecuteCommandAsync(new SocketInteractionContext(_client, interaction), _services);
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            await _configRepository.CreateConfigFileAsync(guild);
            await _interactions.RegisterCommandsToGuildAsync(guild.Id, true);
        }

        private Task LeftGuild(SocketGuild guild)
        {
            _configRepository.GuildConfigs.Remove(guild.Id);
            return Task.CompletedTask;
        }

        private Task Log(LogMessage logMsg)
        {
            Console.WriteLine(logMsg);
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            await _configRepository.SetConnectedGuildConfigsAsync(_client.Guilds);
            foreach (var guild in _client.Guilds)
            {
                await _interactions.RegisterCommandsToGuildAsync(guild.Id, true);
            }
        }

        private async Task ThreadCreated(SocketThreadChannel channel)
        {
            await _configRepository.AddChannelToConfigFileAsync(channel);
        }

        private async Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> channel)
        {
            await _configRepository.DeleteChannelFromConfigFileAsync(channel.Value);
        }

        private async Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> oldChannel, SocketThreadChannel updatedChannel)
        {
            var guildConfig = await _configRepository.GetGuildConfigAsync(updatedChannel.Guild);
            var channelConfig = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == oldChannel.Id);
            if (channelConfig is null)
            {
                return;
            }

            channelConfig.Name = updatedChannel.Name;

            await _configRepository.UpdateChannelInConfigFileAsync(oldChannel.Value, channelConfig);
        }
    }
}
