using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    public class EngineTimer
    {
        protected TimeSpan _timeSpan;
        protected DateTime _startTime;

        public EngineTimer(TimeSpan span)
        {
            _timeSpan = span;
            _startTime = DateTime.Now;
        }

        public void ResetTimer()
        {
            _startTime = DateTime.Now;
        }

        public void ResetTimer(TimeSpan newSpan)
        {
            _timeSpan = newSpan;
            _startTime = DateTime.Now;
        }

        public bool TestTimer()
        {
            if (DateTime.Now > _startTime + _timeSpan) return true;
            return false;
        }

        public TimeSpan GetTimeSpan()
        {
            return _timeSpan;
        }
    }
}
