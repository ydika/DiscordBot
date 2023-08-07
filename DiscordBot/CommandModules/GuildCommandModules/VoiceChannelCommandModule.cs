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

        [CallLimit(1, 60)]
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

            _audioService.CloseAudioStream(Context.Guild.Id);
            await _audioService.PlayAudioAsync(Context.Guild.Id, await voiceChannel.ConnectAsync(), path);

            embed.Title = "Music playback completed";
            await FollowupAsync(embed: embed.Build());
        }

        [CallLimit(1, 60)]
        [SlashCommand("leave", "leaves the voice channel the user is in")]
        public async Task LeaveCommandAsync()
        {
            var embed = new EmbedBuilder()
            {
                Color = (await _jsonConfigRepository.GetGuildConfigAsync((SocketGuild)Context.Guild)).EmbedColor
            };

            var voiceChannel = ((IGuildUser)Context.User).VoiceChannel;
            if (voiceChannel is null || voiceChannel != (await Context.Guild.GetCurrentUserAsync()).VoiceChannel)
            {
                embed.Title = "Bot is not in the voice channel";
                await RespondAsync(embed: embed.Build());
                return;
            }

            _audioService.CloseAudioStream(Context.Guild.Id);

            await voiceChannel.DisconnectAsync();

            embed.Title = $"Bot left the channel *{Context.Channel.Name}*";
            await RespondAsync(embed: embed.Build());
        }
    }
}
