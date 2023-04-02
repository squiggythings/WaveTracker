using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.Audio
{
    public class TickEvent
    {
        public TickEventType eventType;
        public int value;
        public int value2;
        public int countdown;

        public TickEvent(TickEventType eventType, int val, int val2, int timer)
        {
            this.eventType = eventType;
            this.value = val;
            this.value2 = val2;
            this.countdown = timer;
        }

        public void Update()
        {
            countdown--;
        }
    }
    public enum TickEventType
    {
        Note, Instrument, Volume, Effect
    }
}
