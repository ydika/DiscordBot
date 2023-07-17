using Discord;
using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigManagers;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private IConfigurationRoot _config;
    private DiscordSocketClient _client;

    private AppSettings _appSettings = new AppSettings();

    private static Task Main(string[] args) => new Program().RunAsync();

    public async Task RunAsync()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("defaultchannelsettings.json")
            .AddEnvironmentVariables()
            .Build();
        _config.GetSection("AppSettings").Bind(_appSettings);

        _client = new DiscordSocketClient();

        _client.JoinedGuild += JoinedGuild;
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
        await _client.StartAsync();

        //var timer = new Timer(async callback =>
        //{
        //    await DeleteMessagesAsync(await channel.GetMessagesAsync(int.MaxValue).ToArrayAsync());
        //}, null, TimeSpan.Zero, TimeSpan.FromHours(int.Parse(config.GetSection("AppConfig:RemovalFrequency").Value)));

        await Task.Delay(Timeout.Infinite);
    }

    private Task JoinedGuild(SocketGuild arg)
    {
        new JsonConfigManager(_config, _appSettings, arg.Id).CreateConfigFile(arg.Name, arg.Channels);
        return Task.CompletedTask;
    }

    private Task Log(LogMessage logMsg)
    {
        Console.WriteLine(logMsg);
        return Task.CompletedTask;
    }

    //private async Task DeleteMessagesAsync(IReadOnlyCollection<IMessage>[] messagePages)
    //{
    //    foreach (var messagePage in messagePages)
    //    {
    //        foreach (var message in messagePage)
    //        {
    //            if ((DateTime.Now - message.CreatedAt).Days > int.Parse(_config.GetSection("AppConfig:MessageAgeToDelete").Value))
    //            {
    //                await message.DeleteAsync();
    //            }
    //        }
    //    }
    //}
}