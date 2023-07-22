using Discord;
using Discord.Commands;
using Discord.Rest;
using DiscordBot.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services.BotFunctionality.CommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class TextChannelsCommandModule : ModuleBase
    {
        private JsonConfigManager _jsonConfigManager;

        public TextChannelsCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [Command("delete_all_messages")]
        [Summary("deletes all messages on the channel")]
        public async Task DeleteAllMessagesCommand()
        {
            var textChannel = (ITextChannel)Context.Channel;
            var messages = await textChannel.GetMessagesAsync().FlattenAsync();
            await textChannel.DeleteMessagesAsync(messages);
            await ReplyAsync($"Broom for channel **{Context.Channel.Name}** turned on!");
        }

        [Command("turn_on_broom")]
        [Summary("starts deleting messages on the channel periodically")]
        public async Task TurnOnBroomCommand()
        {
            await SetIsDeleteMessagesValue(Context.Guild.Id, Context.Channel.Id, true);
            await ReplyAsync($"Broom for channel **{Context.Channel.Name}** turned on!");
        }

        [Command("turn_off_broom")]
        [Summary("stops deleting messages on the channel periodically")]
        public async Task TurnOfBroomCommand()
        {
            await SetIsDeleteMessagesValue(Context.Guild.Id, Context.Channel.Id, false);
            await ReplyAsync($"Broom for channel **{Context.Channel.Name}** turned off!");
        }

        private async Task SetIsDeleteMessagesValue(ulong guildId, ulong channelId, bool value)
        {
            var guildConfig = _jsonConfigManager.GuildConfigs.FirstOrDefault(x => x.Value.GuildId == guildId).Value;
            if (guildConfig is null)
            {
                return;
            }

            var channelConfig = (TextChannel)guildConfig.DiscordChannels.FirstOrDefault(x => x.Id == channelId);
            if (channelConfig is null)
            {
                return;
            }
            channelConfig.IsDeleteMessages = value;

            await _jsonConfigManager.UpdateConfigFile(guildConfig);
        }
    }
}
