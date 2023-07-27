using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Channels;
using DiscordBot.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services.CommandModules.GuildCommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class GeneralGuildCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private JsonConfigManager _jsonConfigManager;

        public GeneralGuildCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        public async Task<TextChannel> GetTextChannelConfigAsync(SocketGuildChannel channel)
        {
            var guildConfig = await GetGuildConfigAsync(channel);
            var channelConfig = (TextChannel)guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            if (channelConfig is null)
            {
                await _jsonConfigManager.AddChannelToConfigFileAsync(channel);
                channelConfig = (TextChannel)guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channel.Id);
            }

            return channelConfig;
        }

        public async Task<List<TextChannel>> GetTextChannelConfigsAsync(SocketGuildChannel channel)
        {
            var guildConfig = await GetGuildConfigAsync(channel);
            return guildConfig.DiscordChannels.OfType<TextChannel>().ToList();
        }

        public async Task<Guild> GetGuildConfigAsync(SocketGuildChannel channel)
        {
            if (!_jsonConfigManager.GuildConfigs.TryGetValue(channel.Guild, out var guildConfig))
            {
                await _jsonConfigManager.CreateConfigFileAsync(channel.Guild);
                guildConfig = _jsonConfigManager.GuildConfigs[channel.Guild];
            }

            return guildConfig;
        }
    }
}
