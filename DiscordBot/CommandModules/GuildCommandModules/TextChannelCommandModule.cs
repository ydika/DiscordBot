using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.CommandModules.GuildCommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public class TextChannelCommandModule : InteractionModuleBase
    {
        private JsonConfigManager _jsonConfigManager;

        public TextChannelCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [SlashCommand("clear", "deletes all messages on the channel")]
        public async Task ClearCommand()
        {
            await DeferAsync();

            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            var textChannel = (ITextChannel)Context.Channel;
            var messages = await textChannel.GetMessagesAsync(int.MaxValue).FlattenAsync();
            if (messages.Count() == 0)
            {
                embed.Title = "Messages not found";
                await FollowupAsync(embed: embed.Build());
                return;
            }

            await ClearMessages(embed, textChannel, messages, "All messages have been deleted!");
        }

        [SlashCommand("clear-user", "deletes all messages of the user on the channel")]
        public async Task ClearUserCommand(SocketGuildUser user)
        {
            await DeferAsync();

            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            var textChannel = (ITextChannel)Context.Channel;
            var userMessages = (await textChannel.GetMessagesAsync(int.MaxValue).FlattenAsync()).Where(x => x.Author.Id == user.Id);
            if (userMessages.Count() == 0)
            {
                embed.Title = "User messages not found";
                await FollowupAsync(embed: embed.Build());
                return;
            }

            await ClearMessages(embed, textChannel, userMessages, $"All messages of the user *{user.Username}* on the channel are deleted!");
        }

        private async Task ClearMessages(EmbedBuilder embed, ITextChannel textChannel, IEnumerable<IMessage> messages, string title)
        {
            await textChannel.DeleteMessagesAsync(messages.Where(x => (DateTime.Now - x.CreatedAt).Days <= 14));

            foreach (var message in messages.Where(x => (DateTime.Now - x.CreatedAt).Days > 14))
            {
                await textChannel.DeleteMessageAsync(message);
            }

            embed.Title = $"{title}\n{messages.Count()} messages deleted\n*This message will be deleted in 10 seconds*";
            await FollowupAsync(embed: embed.Build());
            messages = await textChannel.GetMessagesAsync().FlattenAsync();

            await Task.Delay(10000);
            await messages.FirstOrDefault(x => x.Author.IsBot && x.Embeds.Any(x => (Embed)x == embed.Build())).DeleteAsync();
        }
    }
}
