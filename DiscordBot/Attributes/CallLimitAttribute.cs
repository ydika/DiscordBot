using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CallLimitAttribute : PreconditionAttribute
    {
        private readonly Dictionary<ulong, CommandCallLimit> _callLimits;
        private readonly int _limit;
        private readonly TimeSpan _interval;

        public CallLimitAttribute(int limit, int seconds)
        {
            _callLimits = new Dictionary<ulong, CommandCallLimit>();
            _limit = limit;
            _interval = TimeSpan.FromSeconds(seconds);
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (!_callLimits.TryGetValue(context.User.Id, out var callLimit))
            {
                _callLimits.Add(context.User.Id, new CommandCallLimit(_limit, _interval));
            }

            if (callLimit is null)
            {
                _callLimits.TryGetValue(context.User.Id, out callLimit);
            }

            if (!callLimit.CanExecute())
            {
                return PreconditionResult.FromError($"You have reached the command call limit. Repeat after *{(callLimit.GetNextExecution() - DateTime.Now).Seconds}* seconds");
            }

            callLimit.Execute();

            return PreconditionResult.FromSuccess();
        }
    }
}
