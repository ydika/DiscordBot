using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services.BotFunctionality.CommandModules
{
    public class GeneralCommandModule : ModuleBase
    {
        private CommandService _commandService;
        private AppSettings _appSettings;

        public GeneralCommandModule(CommandService commandService, AppSettings appSettings)
        {
            _commandService = commandService;
            _appSettings = appSettings;
        }

        [Command("help")]
        public async Task HelpCommand()
        {
            var guildCommands = new List<CommandInfo>();
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
                await ReplyAsync("> # Error: Unknown channel type");
                return;
            }

            if (guildCommands.Count == 0)
            {
                await ReplyAsync("> Commands not found");
                return;
            }

            var counter = 1;
            var helpMessage = new StringBuilder(512).Append($"> # Commands\n> **```md\n> Command prefix: {_appSettings.CommandPrefix}\n> \n");
            foreach (var command in guildCommands)
            {
                helpMessage.Append($"> {counter}. {command.Name} ");
                foreach (var parameter in command.Parameters)
                {
                    helpMessage.Append($"[{parameter.Type.Name} {parameter.Name}]");
                }
                helpMessage.Append($"\n> { (!string.IsNullOrEmpty(command.Summary) ? $"   Summary: {command.Summary}" : "no summary")}\n");
                counter++;
            }
            helpMessage.Append("> ```**");

            await ReplyAsync(helpMessage.ToString());
        }

        private List<CommandInfo> GetCommandsCollection(ContextType contextType)
        {
            return _commandService.Modules
                .Where(module => module.Preconditions.OfType<RequireContextAttribute>().Any(x => x.Contexts == contextType))
                .SelectMany(module => module.Commands).ToList();
        }
    }
}
