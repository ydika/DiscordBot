using Discord;
using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services
{
    public class JsonConfigManager
    {
        private AppSettings _appSettings;
        private ChannelFactory _channelFactory;
        private JsonSerializerSettings _jsonOptions;

        public Dictionary<SocketGuild, Guild> GuildConfigs { get; set; }

        public JsonConfigManager(IConfigurationRoot config, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _channelFactory = new ChannelFactory(config);
            _jsonOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            GuildConfigs = new Dictionary<SocketGuild, Guild>();
        }

        public async Task SetConnectedGuildConfigs(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                GuildConfigs.Add(guild, await ReadConfigFile(guild.Id));
            }
        }

        public async Task CreateConfigFile(ulong guildId, string guildName, IEnumerable<SocketGuildChannel> channels)
        {
            var configPath = GetConfigPathString(guildId);
            if (!File.Exists(configPath))
            {
                var guildConfig = new Guild
                {
                    GuildId = guildId,
                    Name = guildName,
                    DiscordChannels = _channelFactory.CreateChannels(channels).OrderBy(x => x.Type).ToList()
                };
                GuildConfigs.Add(channels.FirstOrDefault().Guild, guildConfig);

                await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
            }
        }

        public async Task<Guild> ReadConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            return JsonConvert.DeserializeObject<Guild>(await File.ReadAllTextAsync(configPath), _jsonOptions);
        }

        public async Task UpdateConfigFile(Guild guildConfig)
        {
            var configPath = GetConfigPathString(guildConfig.GuildId);
            FileExistsChecking(configPath);

            var oldGuildConfig = GuildConfigs.FirstOrDefault(x => x.Value.GuildId == guildConfig.GuildId);
            GuildConfigs.Remove(oldGuildConfig.Key);
            GuildConfigs.Add(oldGuildConfig.Key, guildConfig);

            await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
        }

        public void DeleteConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            File.Delete(configPath);
        }

        public async Task AddChannelToConfigFile(SocketChannel socketChannel)
        {
            if (socketChannel is null)
            {
                return;
            }

            var guildChannel = (SocketGuildChannel)socketChannel;
            var channelType = (ChannelType)socketChannel.GetChannelType();
            if (ChannelDictionary.ChannelTypeMap.TryGetValue(channelType, out var channelClass))
            {
                var discordChannel = _channelFactory.CreateChannel(channelClass, guildChannel);

                var guildConfig = await ReadConfigFile(guildChannel.Guild.Id);
                guildConfig.DiscordChannels.Add(discordChannel);
                guildConfig.DiscordChannels = guildConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                await UpdateConfigFile(guildConfig);

                GuildConfigs.Remove(guildChannel.Guild);
                GuildConfigs.Add(guildChannel.Guild, guildConfig);
            }
            else
            {
                throw new NotSupportedException($"Channel type \"{channelType}\" not supported.");
            }
        }

        public async Task DeleteChannelFromConfigFile(SocketChannel socketChannel)
        {
            if (socketChannel is null)
            {
                return;
            }

            var guildChannel = (SocketGuildChannel)socketChannel;
            var guildConfig = await ReadConfigFile(guildChannel.Guild.Id);
            var foundChannel = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == socketChannel.Id);
            if (foundChannel is not null)
            {
                guildConfig.DiscordChannels.Remove(foundChannel);
                guildConfig.DiscordChannels = guildConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                await UpdateConfigFile(guildConfig);

                GuildConfigs.Remove(guildChannel.Guild);
            }
        }

        private string GetConfigPathString(ulong guildId)
        {
            return $".{_appSettings.GuildConfigsPath}\\{guildId}.json";
        }

        private void FileExistsChecking(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Config file not found. Path: {configPath}");
            }
        }
    }
}
