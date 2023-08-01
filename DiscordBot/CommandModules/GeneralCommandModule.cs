using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.CommandModules
{
    public class GeneralCommandModule : InteractionModuleBase
    {
        private InteractionService _interactions;
        private JsonConfigManager _jsonConfigManager;

        public GeneralCommandModule(InteractionService interactions, JsonConfigManager jsonConfigManager)
        {
            _interactions = interactions;
            _jsonConfigManager = jsonConfigManager;
        }

        [SlashCommand("help", "returns all available commands with description")]
        public async Task HelpCommand()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Commands"
            };

            var commands = new List<SlashCommandInfo>();
            if (Context.Channel is SocketDMChannel)
            {
                commands = GetCommandsCollection(ContextType.DM);
            }
            else if (Context.Channel is SocketGroupChannel)
            {
                commands = GetCommandsCollection(ContextType.Group);
            }
            else if (Context.Channel is SocketGuildChannel)
            {
                embed.Color = new Color((await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor);
                commands = GetCommandsCollection(ContextType.Guild);
            }
            else
            {
                embed.AddField("Error", "Unknown channel type");
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            if (commands.Count == 0)
            {
                embed.AddField("Error", "Commands not found");
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            var appCommands = await Context.Guild.GetApplicationCommandsAsync();
            foreach (var command in commands.OrderBy(x => x.Name))
            {
                embed.AddField($"</{command.Name}:{appCommands.FirstOrDefault(x => x.Name == command.Name).Id}>", $"{command.Description}");
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        private List<SlashCommandInfo> GetCommandsCollection(ContextType contextType)
        {
            return _interactions.Modules
                .Where(module => module.Preconditions.OfType<RequireContextAttribute>().Any(x => x.Contexts == contextType) ||
                       module.Preconditions.OfType<RequireContextAttribute>().FirstOrDefault() is null)
                .SelectMany(module => module.SlashCommands)
                .ToList();
        }
    }
}
