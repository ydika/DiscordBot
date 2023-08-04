using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.CommandModules.GuildCommandModules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class GuildSettingsCommandModule : InteractionModuleBase
    {
        private readonly JsonConfigManager _jsonConfigManager;

        public GuildSettingsCommandModule(JsonConfigManager jsonConfigManager)
        {
            _jsonConfigManager = jsonConfigManager;
        }

        [CallLimit(1, 60)]
        [SlashCommand("embed-color", "sets the color for embed")]
        public async Task EmbedColorCommand([MinLength(6)][MaxLength(7)] string hex)
        {
            var guildConfig = await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild);
            var embed = new EmbedBuilder()
            {
                Color = guildConfig.EmbedColor,
            };

            var hexString = hex.Length == 7 ? hex.Substring(1) : hex;
            if (!uint.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out var embedColor))
            {
                embed.Title = "Embed color not changed";
                await RespondAsync(embed: embed.Build());
                return;
            }

            guildConfig.EmbedColor = embedColor;
            await _jsonConfigManager.UpdateConfigFileAsync(guildConfig);

            embed.Color = embedColor;
            embed.Title = "Embed color changed";
            await RespondAsync(embed: embed.Build());
        }

        [CallLimit(2, 60)]
        [SlashCommand("guild-config", "returns guild config")]
        public async Task GuildConfigCommand()
        {
            var guildConfig = await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild);
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigManager.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor,
                Title = $"*{guildConfig.Name}* Guild Config"
            };

            foreach (var property in guildConfig.GetType().GetProperties())
            {
                if (property.PropertyType.IsGenericType)
                {
                    continue;
                }

                if (property.DeclaringType == guildConfig.GetType())
                {
                    embed.AddField($"{property.Name}", $"{property.GetValue(guildConfig)}");
                }
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
