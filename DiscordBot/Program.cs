// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var client = new DiscordSocketClient();

await RunAsync();

async Task RunAsync()
{
    client.Log += Log;

    await client.LoginAsync(TokenType.Bot, config.GetRequiredSection("AppConfig:Token").Value);
    await client.StartAsync();
    var channel = (IMessageChannel)await client.GetChannelAsync(ulong.Parse(config.GetRequiredSection("DiscordChannels:China").Value));

    var timer = new Timer(async callback =>
    {
        await DeleteMessagesAsync(await channel.GetMessagesAsync(int.MaxValue).ToArrayAsync());
    }, null, TimeSpan.Zero, TimeSpan.FromHours(int.Parse(config.GetSection("AppConfig:RemovalFrequency").Value)));

    await Task.Delay(Timeout.Infinite);
}

Task Log(LogMessage logMsg)
{
    Console.WriteLine(logMsg);
    return Task.CompletedTask;
}

async Task DeleteMessagesAsync(IReadOnlyCollection<IMessage>[] messagePages)
{
    foreach (var messagePage in messagePages)
    {
        foreach (var message in messagePage)
        {
            if ((DateTime.Now - message.CreatedAt).Days > int.Parse(config.GetSection("AppConfig:MessageAgeToDelete").Value))
            {
                await message.DeleteAsync();
            }
        }
    }
}