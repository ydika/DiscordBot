using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.ConfigManagers
{
    public class JsonConfigManager
    {
        private IConfigurationRoot _config;
        private ChannelFactory _channelFactory;
        private string _configPath;
        private ulong _serverId;
        private JsonSerializerSettings _jsonOptions;

        public JsonConfigManager(IConfigurationRoot config, AppSettings appSettings, ulong serverId)
        {
            _config = config;
            _channelFactory = new ChannelFactory(config);
            _configPath = $".{appSettings.ServerConfigsPath}\\{serverId}.json";
            _serverId = serverId;
            _jsonOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public void CreateConfigFile(string serverName, IEnumerable<SocketGuildChannel> channels)
        {
            if (!File.Exists(_configPath))
            {
                var serverConfig = new Server
                {
                    ServerId = _serverId,
                    Name = serverName,
                    DiscordChannels = _channelFactory.CreateChannels(channels).OrderBy(x => x.Type).ToList()
                };

                File.WriteAllText(_configPath, JsonConvert.SerializeObject(serverConfig, _jsonOptions));
            }
        }

        public Server ReadConfigFile()
        {
            FileExistsChecking();

            return JsonConvert.DeserializeObject<Server>(File.ReadAllText(_configPath), _jsonOptions);
        }

        public void UpdateConfigFile(Server serverConfig)
        {
            FileExistsChecking();

            File.WriteAllText(_configPath, JsonConvert.SerializeObject(serverConfig, _jsonOptions));
        }

        public void DeleteConfigFile()
        {
            FileExistsChecking();

            File.Delete(_configPath);
        }

        public void AddChannelToConfigFile(DiscordChannel discordChannel)
        {
            if (discordChannel is null)
            {
                return;
            }

            discordChannel.SetDefaultValues(_config);

            var serverConfig = ReadConfigFile();
            serverConfig.DiscordChannels.Add(discordChannel);
            serverConfig.DiscordChannels = serverConfig.DiscordChannels.OrderBy(x => x.Type).ToList();
            UpdateConfigFile(serverConfig);
        }

        private void FileExistsChecking()
        {
            if (!File.Exists(_configPath))
            {
                throw new FileNotFoundException($"Config file not found. Path: {_configPath}");
            }
        }
    }
}
