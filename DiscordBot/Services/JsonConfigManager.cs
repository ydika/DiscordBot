﻿using Discord;
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

        public async Task SetConnectedGuildConfigsAsync(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                GuildConfigs.Add(guild, await ReadConfigFileAsync(guild.Id));
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
                GuildConfigs.Add(guild.Channels.FirstOrDefault().Guild, guildConfig);

                await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
            }
        }

        public async Task<Guild> ReadConfigFileAsync(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            return JsonConvert.DeserializeObject<Guild>(await File.ReadAllTextAsync(configPath), _jsonOptions);
        }

        public async Task UpdateConfigFileAsync(Guild guildConfig)
        {
            var configPath = GetConfigPathString(guildConfig.GuildId);
            FileExistsChecking(configPath);

            await File.WriteAllTextAsync(configPath, JsonConvert.SerializeObject(guildConfig, _jsonOptions));
        }

        public void DeleteConfigFile(ulong guildId)
        {
            var configPath = GetConfigPathString(guildId);
            FileExistsChecking(configPath);

            File.Delete(configPath);
        }

        public async Task UpdateGuildInConfigFileAsync(SocketGuild guild)
        {
            if (guild is null)
            {
                return;
            }

            if (!GuildConfigs.TryGetValue(guild, out var guildConfig))
            {
                return;
            }

            guildConfig.Name = guild.Name;
            await UpdateConfigFileAsync(guildConfig);
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
                if (!GuildConfigs.TryGetValue(channel.Guild, out var guildConfig))
                {
                    return;
                }

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

            if (!GuildConfigs.TryGetValue(channel.Guild, out var guildConfig))
            {
                return;
            }

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

            if (!GuildConfigs.TryGetValue(channel.Guild, out var guildConfig))
            {
                return;
            }

            var foundChannel = guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            if (foundChannel is not null)
            {
                guildConfig.DiscordChannels.Remove(foundChannel);
                await UpdateConfigFileAsync(guildConfig);
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
