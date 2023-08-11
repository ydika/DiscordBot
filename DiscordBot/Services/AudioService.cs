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
        private Dictionary<ulong, Queue<ProcessStartInfo>> _audioQueues;

        public AudioService()
        {
            _ytdlpPath = Path.GetFullPath($".\\yt-dlp.exe");
            _ffmpegPath = Path.GetFullPath($".\\ffmpeg.exe");
            _audioQueues = new Dictionary<ulong, Queue<ProcessStartInfo>>();
        }

        public async Task PlayAudioAsync(ulong guildId, IVoiceChannel voiceChannel, string path)
        {
            var audioProcessInfo = CreateAudioProcessStartInfo(path);
            if (!_audioQueues.TryGetValue(guildId, out var audioQueue))
            {
                _audioQueues.Add(guildId, new Queue<ProcessStartInfo>(new ProcessStartInfo[] { audioProcessInfo }));

                var audioClient = await voiceChannel.ConnectAsync();
                audioClient.Disconnected += (exception) =>
                {
                    _audioQueues.Remove(guildId);
                    return Task.CompletedTask;
                };

                _ = Task.Run(async () =>
                {
                    await CreateAudioStreamAsync(_audioQueues[guildId], audioClient, audioProcessInfo);

                    await voiceChannel.DisconnectAsync();
                });
            }
            else
            {
                audioQueue.Enqueue(audioProcessInfo);
            }
        }

        private ProcessStartInfo CreateAudioProcessStartInfo(string path) => new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {_ytdlpPath} -o - {path} | {_ffmpegPath} -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
            CreateNoWindow = true,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        private async Task CreateAudioStreamAsync(Queue<ProcessStartInfo> audioQueue, IAudioClient audioClient, ProcessStartInfo audioProcessInfo)
        {
            using (var outStream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                using var process = Process.Start(audioProcessInfo);
                await process.StandardOutput.BaseStream.CopyToAsync(outStream);

                audioQueue.Dequeue();
                await outStream.FlushAsync();
            }

            if (audioQueue.Count > 0)
            {
                await CreateAudioStreamAsync(audioQueue, audioClient, audioQueue.Peek());
            }
        }
    }
}
