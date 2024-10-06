namespace WaveTracker.Audio {
    public class TickEvent {
        public TickEventType eventType;
        public int value;
        public int value2;
        public int countdown;

        public TickEvent(TickEventType eventType, int val, int val2, int timer) {
            this.eventType = eventType;
            value = val;
            value2 = val2;
            countdown = timer;
        }

        public void Update() {
            countdown--;
        }
    }
    public enum TickEventType {
        Note, Instrument, Volume, Effect
    }
}
