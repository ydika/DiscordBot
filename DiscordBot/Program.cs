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
        var timer = new Timer(async callback =>
        {
            await _messagesManager.DeleteMessagesFromTextChannelsAsync();
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(discordSocketConfig);
        _client.ChannelCreated += ChannelCreated;
        _client.ChannelDestroyed += ChannelDestroyed;
        _client.JoinedGuild += JoinedGuild;
        _client.LeftGuild += LeftGuild;
        _client.Log += Log;
        _client.MessageReceived += HandleCommandAsync;
        _client.Ready += Ready;

        await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task ChannelCreated(SocketChannel channel)
    {
        await _configManager.AddChannelToConfigFile(channel);
    }

    private async Task ChannelDestroyed(SocketChannel channel)
    {
        await _configManager.DeleteChannelFromConfigFile(channel);
    }

    private async Task JoinedGuild(SocketGuild guild)
    {
        await _configManager.CreateConfigFile(guild.Id, guild.Name, guild.Channels);
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
        if (userMessage.HasStringPrefix("!", ref argPos))
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
        await _configManager.SetConnectedGuildConfigs(_client.Guilds);
    }
}
