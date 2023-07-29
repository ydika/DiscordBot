﻿using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ConfigModels
{
    public class DMChannel : TextChannel
    {
        public override void SetDefaultValues(IConfigurationRoot config)
        {
            config.GetSection("DefaultDMChannelSettings").Bind(this);
            base.SetDefaultValues(config);
        }
    }
}