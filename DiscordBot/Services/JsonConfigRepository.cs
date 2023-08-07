using Discord;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services
{
    public class JsonConfigRepository
    {
        private readonly DiscordSocketClient _client;

        private readonly IConfigurationRoot _config;
        private readonly AppSettings _appSettings;
        private readonly ChannelFactory _channelFactory;
        private readonly JsonSerializerSettings _jsonOptions;

        public Dictionary<ulong, Guild> GuildConfigs { get; set; }

        public JsonConfigRepository(DiscordSocketClient client, IConfigurationRoot config, AppSettings appSettings)
        {
            _client = client;
            _config = config;
            _appSettings = appSettings;
            _channelFactory = new ChannelFactory(config);
            _jsonOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            GuildConfigs = new Dictionary<ulong, Guild>();

            DirectoryExistsChecking(_appSettings.GuildConfigsPath);
        }

        private void DirectoryExistsChecking(string guildConfigsPath)
        {
            if (!Directory.Exists(guildConfigsPath))
            {
                Directory.CreateDirectory(guildConfigsPath);
            }
        }

        public async Task SetConnectedGuildConfigsAsync(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                if (await GetGuildConfigAsync(guild) is null)
                {
                    GuildConfigs.Add(guild.Id, await ReadConfigFileAsync(guild.Id));
                }
            }
        }

        public async Task CreateConfigFileAsync(SocketGuild guild)
        {
            var configPath = GetConfigPathString(guild.Id);
            if (!File.Exists(configPath))
            {
                var guildConfig = new Guild
                {
                    GuildId = guild.Id,
                    Name = guild.Name,
                    DiscordChannels = _channelFactory.CreateChannels(guild.Channels).OrderBy(x => x.Type).ToList()
                };
                guildConfig.SetDefaultValues(_config);
                GuildConfigs.Add(guild.Id, guildConfig);

                await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
            }
            else
            {
                GuildConfigs.Add(guild.Id, await ReadConfigFileAsync(guild.Id));
            }
        }

        public async Task<Guild> ReadConfigFileAsync(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            await FileExistsChecking(guildId, configPath);

            return JsonConvert.DeserializeObject<Guild>(await File.ReadAllTextAsync(configPath), _jsonOptions);
        }

        public async Task UpdateConfigFileAsync(Guild guildConfig)
        {
            var configPath = GetConfigPathString(guildConfig.GuildId);
            await FileExistsChecking(guildConfig.GuildId, configPath);

            await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
        }

        public async Task DeleteConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            await FileExistsChecking(guildId, configPath);

            File.Delete(configPath);
        }

        private string GetConfigPathString(ulong guildId)
        {
            return $"{_appSettings.GuildConfigsPath}\\{guildId}.json";
        }

        private async Task FileExistsChecking(ulong guildId, string configPath)
        {
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Config file not found. Path: {configPath}");

                var guild = _client.GetGuild(guildId);
                GuildConfigs.Remove(guild.Id);
                await CreateConfigFileAsync(guild);

                Console.WriteLine($"Config file created. Path: {configPath}");
            }
        }

        public async Task UpdateGuildInConfigFileAsync(SocketGuild guild)
        {
            if (guild is null)
            {
                return;
            }

            var guildConfig = await GetGuildConfigAsync(guild);
            guildConfig.Name = guild.Name;

            await UpdateConfigFileAsync(guildConfig);
        }

        public async Task<Guild> GetGuildConfigAsync(SocketGuild guild)
        {
            if (!GuildConfigs.TryGetValue(guild.Id, out var guildConfig))
            {
                await CreateConfigFileAsync(guild);
                guildConfig = GuildConfigs[guild.Id];
            }

            return guildConfig;
        }

        public async Task AddChannelToConfigFileAsync(SocketGuildChannel channel)
        {
            if (channel is null)
            {
                return;
            }

            var channelType = (ChannelType)channel.GetChannelType();
            if (ChannelDictionary.ChannelTypeMap.TryGetValue(channelType, out var channelClass))
            {
                var guildConfig = await GetGuildConfigAsync(channel.Guild);

                var discordChannel = _channelFactory.CreateChannel(channelClass, channel);
                guildConfig.DiscordChannels.Add(discordChannel);
                guildConfig.DiscordChannels = guildConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                await UpdateConfigFileAsync(guildConfig);
            }
            else
            {
                throw new NotSupportedException($"Channel type \"{channelType}\" not supported.");
            }
        }

        public async Task UpdateChannelInConfigFileAsync(SocketGuildChannel channel, DiscordChannel channelConfig)
        {
            if (channel is null || channel.Id != channelConfig.Id)
            {
                return;
            }

            var guildConfig = await GetGuildConfigAsync(channel.Guild);

            var foundChannel = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            if (foundChannel is not null)
            {
                foundChannel = channelConfig;
                await UpdateConfigFileAsync(guildConfig);
            }
        }

        public async Task DeleteChannelFromConfigFileAsync(SocketGuildChannel channel)
        {
            if (channel is null)
            {
                return;
            }

            var guildConfig = await GetGuildConfigAsync(channel.Guild);

            var foundChannel = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            if (foundChannel is not null)
            {
                guildConfig.DiscordChannels.Remove(foundChannel);
                await UpdateConfigFileAsync(guildConfig);
            }
        }

        public async Task<TextChannel> GetTextChannelConfigAsync(SocketGuildChannel channel)
        {
            var guildConfig = await GetGuildConfigAsync(channel.Guild);
            var channelConfig = (TextChannel)guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            if (channelConfig is null)
            {
                await AddChannelToConfigFileAsync(channel);
                channelConfig = (TextChannel)guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            }

            return channelConfig;
        }

        public async Task<List<TextChannel>> GetTextChannelConfigsAsync(SocketGuildChannel channel)
        {
            var guildConfig = await GetGuildConfigAsync(channel.Guild);
            return guildConfig.DiscordChannels.OfType<TextChannel>().ToList();
        }
    }
}
