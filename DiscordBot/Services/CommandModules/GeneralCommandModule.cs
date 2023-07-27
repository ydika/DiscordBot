using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services.BotFunctionality.CommandModules
{
    public class GeneralCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionService _interactions;
        private AppSettings _appSettings;

        public GeneralCommandModule(InteractionService interactions, AppSettings appSettings)
        {
            _interactions = interactions;
            _appSettings = appSettings;
        }

        [SlashCommand("help", "returns all available commands with description")]
        public async Task HelpCommand()
        {
            var guildCommands = new List<SlashCommandInfo>();
            if (Context.Channel is SocketDMChannel)
            {
                guildCommands = GetCommandsCollection(ContextType.DM);
            }
            else if (Context.Channel is SocketGroupChannel)
            {
                guildCommands = GetCommandsCollection(ContextType.Group);
            }
            else if (Context.Channel is SocketGuildChannel)
            {
                guildCommands = GetCommandsCollection(ContextType.Guild);
            }
            else
            {
                await RespondAsync("> # Error: Unknown channel type");
                return;
            }

            if (guildCommands.Count == 0)
            {
                await RespondAsync("> Commands not found");
                return;
            }

            var counter = 1;
            var helpMessage = new StringBuilder(512).Append($"> # Commands\n> **```md\n> Command prefix: {_appSettings.CommandPrefix}\n> \n");
            foreach (var command in guildCommands)
            {
                helpMessage.Append($"> {counter}. {command.Name} ");
                foreach (var parameter in command.Parameters)
                {
                    helpMessage.Append($"[{parameter.ParameterType.Name} {parameter.Name}]");
                }
                helpMessage.Append($"\n> { (!string.IsNullOrEmpty(command.Description) ? $"   Description: {command.Description}" : "no description")}\n");
                counter++;
            }
            helpMessage.Append("> ```**");

            await RespondAsync(helpMessage.ToString());
        }

        private List<SlashCommandInfo> GetCommandsCollection(ContextType contextType)
        {
            return _interactions.Modules
                .Where(module => module.Preconditions.OfType<RequireContextAttribute>().Any(x => x.Contexts == contextType))
                .SelectMany(module => module.SlashCommands).ToList();
        }
    }
}
