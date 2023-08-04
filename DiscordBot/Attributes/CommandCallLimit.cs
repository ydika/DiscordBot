using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Attributes
{
    public class CommandCallLimit
    {
        private int _limit;
        private TimeSpan _interval;
        private DateTime _lastExecuted;
        private int _executedCount;

        public CommandCallLimit(int limit, TimeSpan interval)
        {
            _limit = limit;
            _interval = interval;
            _lastExecuted = DateTime.MinValue;
            _executedCount = 0;
        }

        public void Execute()
        {
            _executedCount++;

            if (DateTime.Now >= _lastExecuted + _interval)
            {
                _lastExecuted = DateTime.Now;
                _executedCount = 1;
            }
        }

        public bool CanExecute()
        {
            return _executedCount < _limit || DateTime.Now >= _lastExecuted + _interval;
        }

        public DateTime GetNextExecution()
        {
            if (_executedCount >= _limit)
            {
                return _lastExecuted + _interval;
            }
            else
            {
                return DateTime.Now;
            }
        }
    }
}
