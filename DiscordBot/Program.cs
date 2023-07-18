using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
    private Dictionary<ulong, SocketGuild> _guilds;

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
        _guilds = new Dictionary<ulong, SocketGuild>();

        _client.ChannelCreated += ChannelCreated;
        _client.ChannelDestroyed += ChannelDestroyed;
        _client.JoinedGuild += JoinedGuild;
        _client.LeftGuild += LeftGuild;
        _client.Log += Log;

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
        _guilds.Add(guild.Id, guild);
        _configManager.CreateConfigFile(guild.Id, guild.Name, guild.Channels);
        return Task.CompletedTask;
    }

    private Task LeftGuild(SocketGuild guild)
    {
        _guilds.Remove(guild.Id);
        return Task.CompletedTask;
    }

    private Task Log(LogMessage logMsg)
    {
        Console.WriteLine(logMsg);
        return Task.CompletedTask;
    }
}
