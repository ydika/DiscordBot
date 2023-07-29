using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ConfigModels
{
    public class PublicThreadChannel : TextChannel
    {
        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultPublicThreadChannelSettings").Bind(this);
            base.SetDefaultValues(config);
        }
    }
}