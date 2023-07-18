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

        public JsonConfigManager(IConfigurationRoot config, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _channelFactory = new ChannelFactory(config);
            _jsonOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public void CreateConfigFile(ulong serverId, string serverName, IEnumerable<SocketGuildChannel> channels)
        {
            var configPath = GetConfigPathString(serverId);
            if (!File.Exists(configPath))
            {
                var serverConfig = new Server
                {
                    ServerId = serverId,
                    Name = serverName,
                    DiscordChannels = _channelFactory.CreateChannels(channels).OrderBy(x => x.Type).ToList()
                };

                File.WriteAllText(configPath, JsonConvert.SerializeObject(serverConfig, _jsonOptions));
            }
        }

        public Server ReadConfigFile(ulong serverId)
        {
            var configPath = GetConfigPathString(serverId);
            FileExistsChecking(configPath);

            return JsonConvert.DeserializeObject<Server>(File.ReadAllText(configPath), _jsonOptions);
        }

        public void UpdateConfigFile(Server serverConfig)
        {
            var configPath = GetConfigPathString(serverConfig.ServerId);
            FileExistsChecking(configPath);

            File.WriteAllText(configPath, JsonConvert.SerializeObject(serverConfig, _jsonOptions));
        }

        public void DeleteConfigFile(ulong serverId)
        {
            var configPath = GetConfigPathString(serverId);
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

                var serverConfig = ReadConfigFile(guildChannel.Guild.Id);
                serverConfig.DiscordChannels.Add(discordChannel);
                serverConfig.DiscordChannels = serverConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                UpdateConfigFile(serverConfig);
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
            var serverConfig = ReadConfigFile(guildChannel.Guild.Id);
            var foundChannel = serverConfig.DiscordChannels.FirstOrDefault(x => x.Id == socketChannel.Id);
            if (foundChannel is not null)
            {
                serverConfig.DiscordChannels.Remove(foundChannel);
                serverConfig.DiscordChannels = serverConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
                UpdateConfigFile(serverConfig);
            }
        }

        private string GetConfigPathString(ulong serverId)
        {
            return $".{_appSettings.ServerConfigsPath}\\{serverId}.json";
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
