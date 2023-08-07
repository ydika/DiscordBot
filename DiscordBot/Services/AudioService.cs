using Discord.Audio;
using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.ConfigModels;

namespace DiscordBot.Services
{
    public class AudioService
    {
        private readonly string _ytdlpPath;
        private readonly string _ffmpegPath;
        private Dictionary<ulong, Process> _audioProcesses;

        public AudioService()
        {
            _ytdlpPath = Path.GetFullPath($".\\yt-dlp.exe");
            _ffmpegPath = Path.GetFullPath($".\\ffmpeg.exe");
            _audioProcesses = new Dictionary<ulong, Process>();
        }

        public async Task PlayAudioAsync(ulong guildId, IAudioClient audioClient, string path)
        {
            var ffmpeg = CreateAudioStream(path);
            _audioProcesses.Add(guildId, ffmpeg);

            var discord = audioClient.CreatePCMStream(AudioApplication.Music);
            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(discord);
            await discord.FlushAsync();
        }

        private Process CreateAudioStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {_ytdlpPath} -o - {path} | {_ffmpegPath} -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }

        public void CloseAudioStream(ulong guildId)
        {
            if (!_audioProcesses.TryGetValue(guildId, out var ffmpeg))
            {
                return;
            }

            _audioProcesses.Remove(guildId);
            ffmpeg.Close();
        }
    }
}
