using DiscordBot;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        var serviceCollection = new ServiceCollection();
        new ServiceConfigurator().ConfigureServices(serviceCollection);
        var services = serviceCollection.BuildServiceProvider();

        await services.GetRequiredService<Startup>().RunAsync();

        await Task.Delay(Timeout.Infinite);
    }
}
