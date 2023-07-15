using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Channels
{
    public class ChannelDictionary
    {
        public static readonly Dictionary<ChannelType, Type> ChannelTypeMap = new Dictionary<ChannelType, Type>
        {
            { ChannelType.Text, typeof(TextChannel) },
            { ChannelType.DM, typeof(DMChannel) },
            { ChannelType.Voice, typeof(VoiceChannel) },
            { ChannelType.Group, typeof(GroupChannel) },
            { ChannelType.Category, typeof(CategoryChannel) },
            { ChannelType.News, typeof(NewsChannel) },
            { ChannelType.Store, typeof(StoreChannel) },
            { ChannelType.NewsThread, typeof(NewsThreadChannel) },
            { ChannelType.PublicThread, typeof(PublicThreadChannel) },
            { ChannelType.PrivateThread, typeof(PrivateThreadChannel) },
            { ChannelType.Stage, typeof(StageChannel) },
            { ChannelType.Forum, typeof(ForumChannel) }
        };
    }
}
