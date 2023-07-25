using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigModels;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

internal class Program
{
    private IServiceProvider _services;
    private CommandService _commandService;

    private AppSettings _appSettings;
    private JsonConfigManager _configManager;
    private MessagesManager _messagesManager;

    private DiscordSocketClient _client;

    private static Task Main(string[] args) => new Program().RunAsync();

    public async Task RunAsync()
    {
        var serviceCollection = new ServiceCollection();
        new ServiceConfigurator().ConfigureServices(serviceCollection);
        _services = serviceCollection.BuildServiceProvider();
        _commandService = new CommandService();
        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _appSettings = _services.GetService<AppSettings>();
        _configManager = _services.GetService<JsonConfigManager>();
        _messagesManager = _services.GetService<MessagesManager>();
        _ = Task.Run(() => PeriodicallyDeleteMessages());

        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(discordSocketConfig);
        RegisterEvents();

        await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    public async Task PeriodicallyDeleteMessages(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _messagesManager.DeleteMessagesFromTextChannelsAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }

    private void RegisterEvents()
    {
        _client.ChannelCreated += ChannelCreated;
        _client.ChannelDestroyed += ChannelDestroyed;
        _client.ChannelUpdated += ChannelUpdated;
        _client.GuildUpdated += GuildUpdated;
        _client.JoinedGuild += JoinedGuild;
        _client.LeftGuild += LeftGuild;
        _client.Log += Log;
        _client.MessageReceived += HandleCommandAsync;
        _client.Ready += Ready;
        _client.ThreadCreated += ThreadCreated;
        _client.ThreadDeleted += ThreadDeleted;
        _client.ThreadUpdated += ThreadUpdated;
    }

    private async Task ChannelCreated(SocketChannel channel)
    {
        await _configManager.AddChannelToConfigFileAsync((SocketGuildChannel)channel);
    }

    private async Task ChannelDestroyed(SocketChannel channel)
    {
        await _configManager.DeleteChannelFromConfigFileAsync((SocketGuildChannel)channel);
    }

    private async Task ChannelUpdated(SocketChannel oldChannel, SocketChannel updatedChannel)
    {
        var guildChannel = (SocketGuildChannel)updatedChannel;
        var channelConfig = _configManager.GuildConfigs[guildChannel.Guild].DiscordChannels.FirstOrDefault(x => x.Id == oldChannel.Id);
        if (channelConfig is null)
        {
            return;
        }

        channelConfig.Name = guildChannel.Name;

        await _configManager.UpdateChannelInConfigFileAsync((SocketGuildChannel)oldChannel, channelConfig);
    }

    private async Task GuildUpdated(SocketGuild oldGuild, SocketGuild updatedGuild)
    {
        await _configManager.UpdateGuildInConfigFileAsync(updatedGuild);
    }

    private async Task JoinedGuild(SocketGuild guild)
    {
        await _configManager.CreateConfigFileAsync(guild);
    }

    private Task LeftGuild(SocketGuild guild)
    {
        _configManager.GuildConfigs.Remove(guild);
        return Task.CompletedTask;
    }

    private Task Log(LogMessage logMsg)
    {
        Console.WriteLine(logMsg);
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        if (message.Author.IsBot)
        {
            return;
        }

        var argPos = 0;
        var userMessage = (SocketUserMessage)message;
        var context = new SocketCommandContext(_client, userMessage);
        if (userMessage.HasStringPrefix(_appSettings.CommandPrefix, ref argPos))
        {
            var result = await _commandService.ExecuteAsync(context, argPos, _services);
            if (result is not null && !result.IsSuccess)
            {
                Console.WriteLine(result.ErrorReason);
            }
        }
    }

    private async Task Ready()
    {
        await _configManager.SetConnectedGuildConfigsAsync(_client.Guilds);
    }

    private async Task ThreadCreated(SocketThreadChannel channel)
    {
        await _configManager.AddChannelToConfigFileAsync(channel);
    }

    private async Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> channel)
    {
        await _configManager.DeleteChannelFromConfigFileAsync(channel.Value);
    }

    private async Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> oldChannel, SocketThreadChannel updatedChannel)
    {
        var channelConfig = _configManager.GuildConfigs[updatedChannel.Guild].DiscordChannels.FirstOrDefault(x => x.Id == oldChannel.Id);
        if (channelConfig is null)
        {
            return;
        }

        channelConfig.Name = updatedChannel.Name;

        await _configManager.UpdateChannelInConfigFileAsync(oldChannel.Value, channelConfig);
    }
}
