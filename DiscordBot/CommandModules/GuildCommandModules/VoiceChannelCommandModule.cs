using Discord.Audio;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DiscordBot.Services;
using System.Threading.Channels;
using DiscordBot.Attributes;
using Discord.WebSocket;

namespace DiscordBot.CommandModules.GuildCommandModules
{
    public class VoiceChannelCommandModule : InteractionModuleBase
    {
        private readonly AudioService _audioService;
        private readonly JsonConfigRepository _jsonConfigRepository;

        public VoiceChannelCommandModule(AudioService audioService, JsonConfigRepository jsonConfigRepository)
        {
            _audioService = audioService;
            _jsonConfigRepository = jsonConfigRepository;
        }

        [CallLimit(3, 60)]
        [SlashCommand("play", "plays music in specified path")]
        public async Task PlayCommandAsync(string path)
        {
            await DeferAsync();

            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            var voiceChannel = ((IGuildUser)Context.User).VoiceChannel;
            if (voiceChannel is null)
            {
                embed.Title = "Enter the voice channel and call the command again";
                await FollowupAsync(embed: embed.Build());
                return;
            }

            await _audioService.PlayAudioAsync(Context.Guild.Id, voiceChannel, path);

            embed.Title = $"Added to queue\n{path}";
            await FollowupAsync(embed: embed.Build());
        }
    }
}
