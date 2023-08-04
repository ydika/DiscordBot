using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Attributes;
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
        private readonly JsonConfigManager _jsonConfigManager;

        public TextChannelCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [CallLimit(1, 60)]
        [SlashCommand("clear", "deletes all messages on the channel")]
        public async Task ClearCommand()
        {
            await DeferAsync();

            var textChannel = (ITextChannel)Context.Channel;
            var messages = await textChannel.GetMessagesAsync(int.MaxValue).FlattenAsync();

            await ClearMessagesOnChannel(Context.Interaction.Id, textChannel, messages.ToList(), "All messages have been deleted!");
        }

        [CallLimit(1, 60)]
        [SlashCommand("clear-user", "deletes all messages of the user on the channel")]
        public async Task ClearUserCommand(SocketGuildUser user)
        {
            await DeferAsync();

            var textChannel = (ITextChannel)Context.Channel;
            var userMessages = (await textChannel.GetMessagesAsync(int.MaxValue).FlattenAsync()).Where(x => x.Author.Id == user.Id);

            await ClearMessagesOnChannel(Context.Interaction.Id, textChannel, userMessages.ToList(), $"All messages of the user *{user.Username}* on the channel are deleted!");
        }

        private async Task ClearMessagesOnChannel(ulong interactionId, ITextChannel textChannel, List<IMessage> messages, string title)
        {
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            var botMessage = messages.FirstOrDefault(x => x.Interaction?.Id == interactionId);
            if (messages.Count == 0 || messages.Count == 1 && botMessage is not null)
            {
                embed.Title = "Messages not found";
                await FollowupAsync(embed: embed.Build());
                return;
            }
            
            messages.Remove(botMessage);

            await textChannel.DeleteMessagesAsync(messages.Where(x => (DateTime.Now - x.CreatedAt).Days <= 14));
            foreach (var message in messages.Where(x => (DateTime.Now - x.CreatedAt).Days > 14))
            {
                await textChannel.DeleteMessageAsync(message);
                await Task.Delay(600);
            }

            embed.Title = $"{title}\n{messages.Count} messages deleted\n*This message will be deleted in 10 seconds*";

            var response = await FollowupAsync(embed: embed.Build());
            await Task.Delay(10000);
            await response.DeleteAsync();
        }
    }
}
