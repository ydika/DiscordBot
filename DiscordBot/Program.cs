using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.BotFunctionality.MessageManagement;
using DiscordBot.Channels;
using DiscordBot.ConfigManagers;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Channels;
using System.Windows.Input;

internal class Program
{
    private IConfigurationRoot _config;
    private AppSettings _appSettings;
    private JsonConfigManager _configManager;

    private DiscordSocketClient _client;
    private MessagesManager _messageManager;

    private static Task Main(string[] args) => new Program().RunAsync();

    public async Task RunAsync()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("defaultchannelsettings.json")
            .AddEnvironmentVariables()
            .Build();
        _appSettings = new AppSettings();
        _config.GetSection("AppSettings").Bind(_appSettings);
        _configManager = new JsonConfigManager(_config, _appSettings);

        _client = new DiscordSocketClient();
        _client.ChannelCreated += ChannelCreated;
        _client.ChannelDestroyed += ChannelDestroyed;
        _client.JoinedGuild += JoinedGuild;
        _client.LeftGuild += LeftGuild;
        _client.Log += Log;
        _client.Ready += Ready;

        await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        _configManager.AddChannelToConfigFile(channel);
        return Task.CompletedTask;
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        _configManager.DeleteChannelFromConfigFile(channel);
        return Task.CompletedTask;
    }

    private Task JoinedGuild(SocketGuild guild)
    {
        _configManager.CreateConfigFile(guild.Id, guild.Name, guild.Channels);
        return Task.CompletedTask;
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

    private async Task Ready()
    {
        _configManager.SetConnectedGuildConfigs(_client.Guilds);
        _messageManager = new MessagesManager(_configManager);
        await _messageManager.DeleteMessagesFromTextChannelsAsync();
    }
}
