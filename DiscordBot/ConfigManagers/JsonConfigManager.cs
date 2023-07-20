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
using System.Threading.Channels;

namespace DiscordBot.ConfigManagers
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

        public void SetConnectedGuildConfigs(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                GuildConfigs.Add(guild, ReadConfigFile(guild.Id));
            }
        }

        public void CreateConfigFile(ulong guildId, string guildName, IEnumerable<SocketGuildChannel> channels)
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

                File.WriteAllText(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
            }
        }

        public Guild ReadConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            return JsonConvert.DeserializeObject<Guild>(File.ReadAllText(configPath), _jsonOptions);
        }

        public void UpdateConfigFile(Guild guildConfig)
        {
            var configPath = GetConfigPathString(guildConfig.GuildId);
            FileExistsChecking(configPath);

            File.WriteAllText(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
        }

        public void DeleteConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            File.Delete(configPath);
        }

        public void AddChannelToConfigFile(SocketChannel socketChannel)
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

                var guildConfig = ReadConfigFile(guildChannel.Guild.Id);
                guildConfig.DiscordChannels.Add(discordChannel);
                guildConfig.DiscordChannels = guildConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                UpdateConfigFile(guildConfig);

                GuildConfigs.Remove(guildChannel.Guild);
                GuildConfigs.Add(guildChannel.Guild, guildConfig);
            }
            else
            {
                throw new NotSupportedException($"Channel type \"{channelType}\" not supported.");
            }
        }

        public void DeleteChannelFromConfigFile(SocketChannel socketChannel)
        {
            if (socketChannel is null)
            {
                return;
            }

            var guildChannel = (SocketGuildChannel)socketChannel;
            var guildConfig = ReadConfigFile(guildChannel.Guild.Id);
            var foundChannel = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == socketChannel.Id);
            if (foundChannel is not null)
            {
                guildConfig.DiscordChannels.Remove(foundChannel);
                guildConfig.DiscordChannels = guildConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                UpdateConfigFile(guildConfig);

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
