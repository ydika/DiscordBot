using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordBot.Config
{
    public class JsonConfigManager
    {
        private ChannelFactory _channelFactory;
        private string _configPath;
        private ulong _serverId;

        public JsonConfigManager(IConfigurationRoot config, AppSettings appSettings, ulong serverId)
        {
            _channelFactory = new ChannelFactory(config);
            _configPath = $".{appSettings.ServerConfigsPath}\\{serverId}.json";
            _serverId = serverId;
        }

        public void CreateConfigFile(string name, IEnumerable<SocketGuildChannel> channels)
        {
            if (!File.Exists(_configPath))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var serverConfig = new Server
                {
                    ServerId = _serverId,
                    Name = name,
                    DiscordChannels = _channelFactory.CreateChannels(channels).OrderBy(x => x.Type).ToList()
                };

                var jsonText = new StringBuilder(256);
                foreach (var channel in serverConfig.DiscordChannels)
                {
                    jsonText.Append(JsonSerializer.Serialize(channel, channel.GetType(), jsonOptions));
                }
                File.WriteAllText(_configPath, jsonText.ToString());
            }
        }

        public void ReadConfigFile()
        {
        }

        public void UpdateConfigFile()
        {
        }

        public void DeleteConfigFile()
        {
        }
    }
}
