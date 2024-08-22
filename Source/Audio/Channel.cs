using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WaveTracker.Tracker;

namespace WaveTracker.Audio {
    public class Channel {
        private int id;
        public bool IsMuted { get; set; }
        public Wave CurrentWave {
            get {
                return App.CurrentModule.WaveBank[(WaveIndex + 100) % 100];
            }
        }

        public int WaveIndex { get { return waveIndex.Value; } }
        public List<TickEvent> tickEvents;

        private float TotalAmplitude {
            get { return tremoloMultiplier * (channelVolume / 99f) * (envelopePlayers[Envelope.EnvelopeType.Volume].Value / 99f); }
        }

        private float TotalPitch {
            get {
                return Math.Clamp(channelNotePorta + bendOffset + vibratoOffset + pitchFallOffset + arpeggioOffset + detuneOffset + envelopePlayers[Envelope.EnvelopeType.Arpeggio].Value, -12, 131);
            }
        }

        public float WaveMorphPosition {
            get {
                return waveBlendAmt.Value / 99f;
            }
        }

        private float bassBoost;
        private bool noteOn;
        private int arpeggioOffset;
        private int arpeggionote2;
        private int arpeggionote3;
        private int arpCounter;

        public float stereoPhaseOffset;
        private float vibratoOffset; // 4xx command
        private double vibratoTime;
        private int volumeSlideSpeed;
        private float vibratoSpeed;
        private float vibratoIntensity;
        private float tremoloMultiplier = 1f; // 7xx command
        private float tremoloTime;
        private float tremoloSpeed;
        private float tremoloIntensity;
        private float pitchFallOffset; // 1xx and 2xx commands
        private float pitchFallSpeed; // 1xx and 2xx commands
        private float portaSpeed; // 3xx command (in samples)
        private float portaTime;
        private float bendSpeed; // Qxy/Rxy command (in samples)
        private float bendOffset;
        private int targetBendAmt;
        private float channelNotePorta;
        private float detuneOffset; // Pxx command
        private struct EnvelopeParameter {
            public int patternValue;
            public int envelopeValue;
            public int Value {
                get { return envelopeValue == -1 ? patternValue : envelopeValue; }
            }

            public int AdditiveValue {
                get { return envelopeValue == -1 ? patternValue : (int)(patternValue + envelopeValue * (1 - patternValue / 100f)); }
            }

            public void Reset(int value) {
                patternValue = value;
                envelopeValue = -1;
            }
        }
        private EnvelopeParameter waveIndex;
        private EnvelopeParameter waveBlendAmt;
        private EnvelopeParameter waveFmAmt;
        private EnvelopeParameter waveStretchAmt;
        private EnvelopeParameter waveSyncAmt;
        private int downsampleFactor; //Xxx command
        private int downsampleCounter;
        private float lastSampleL;
        private float lastSampleR;
        private StereoBiQuadFilter stereoBiQuadFilter; // Hxy command
        private float _waveStretchSmooth;
        public int SampleStartOffset { get; private set; } // Yxx command

        private int channelVolume; // volume column
        private int channelNote; // notes column
        public Instrument CurrentInstrument { get; private set; } // instrument column
        private float lastNote;
        private SampleInstrument instrumentAsSample;
        public Dictionary<Envelope.EnvelopeType, EnvelopePlayer> envelopePlayers;
        public float _sampleVolume;
        private float lastPitch;
        private double _time;
        private float _leftAmp;
        private float _rightAmp;
        private float _fadeMultiplier = 1;
        private float _volumeSmooth;
        private float _fmSmooth;
        public float coarseSampleVolume;
        public int tickNum;
        private float filterResonance;
        private float targetFilterCutoffFrequency;
        private float _filterCutoffFrequency;
        private int currentInstrumentID;
        private enum VoiceState { On, Off, Release }
        private VoiceState _state;
        public bool IsPlaying {
            get {
                return _state == VoiceState.On;
            }
        }

        private static Envelope[] defaultEnvelopes;

        public Channel(int id) {
            this.id = id;
            tickEvents = [];
            envelopePlayers = [];
            foreach (Envelope.EnvelopeType envelopeType in Enum.GetValues(typeof(Envelope.EnvelopeType))) {
                envelopePlayers.Add(envelopeType, new EnvelopePlayer(envelopeType));
            }
            if (defaultEnvelopes == null) {
                defaultEnvelopes = new Envelope[Enum.GetValues(typeof(Envelope.EnvelopeType)).Length];
                for (int i = 0; i < defaultEnvelopes.Length; i++) {
                    defaultEnvelopes[i] = new Envelope((Envelope.EnvelopeType)i);
                }
            }
            stereoBiQuadFilter = new StereoBiQuadFilter();
            Reset();
        }

        public float SampleTime {
            get {
                return (float)_time;
            }
        }

        public int SamplePlaybackPosition { get; private set; }

        public void QueueEvent(TickEventType eventType, int val1, int val2, int delay) {
            if (delay < 0) {
                DoEvent(new TickEvent(eventType, val1, val2, delay));
            }
            else {
                tickEvents.Add(new TickEvent(eventType, val1, val2, delay));
            }
        }

        public void ApplyEffect(char command, int parameter) {
            switch (command) {
                case '4':
                    if (parameter == 0) {
                        vibratoTime = 0;
                    }

                    vibratoSpeed = (parameter + 1) / 16;
                    vibratoIntensity = parameter % 16;
                    break;
                case '7':
                    tremoloSpeed = (parameter + 1) / 16;
                    tremoloIntensity = parameter % 16;
                    break;
                case '8':
                    SetPanning(parameter / 100f);
                    break;
                case '1':
                    pitchFallSpeed = parameter / 1f;
                    break;
                case '2':
                    pitchFallSpeed = -parameter / 1f;
                    break;
                case '3':
                    portaSpeed = parameter * 1.15f;
                    break;
                case 'Q':
                    bendSpeed = parameter / 16 * 11 + 1;
                    targetBendAmt += parameter % 16;
                    break;
                case 'R':
                    bendSpeed = parameter / 16 * 11 + 1;
                    targetBendAmt -= parameter % 16;
                    break;
                case '0':
                    arpCounter = 0;
                    arpeggionote2 = parameter / 16;
                    arpeggionote3 = parameter % 16;
                    break;
                case '9':
                    stereoPhaseOffset = parameter / 200f;
                    break;
                case 'P':
                    detuneOffset = (parameter - 50) / 37.5f;
                    break;
                case 'V':
                    SetWave(parameter);
                    break;
                case 'A':
                    volumeSlideSpeed = -parameter;
                    break;
                case 'W':
                    volumeSlideSpeed = parameter;
                    break;
                case 'I':
                    waveBlendAmt.patternValue = parameter;
                    break;
                case 'M':
                    waveFmAmt.patternValue = parameter;
                    break;
                case 'J':
                    waveStretchAmt.patternValue = parameter;
                    break;
                case 'Y':
                    SampleStartOffset = parameter;
                    break;
                case 'K':
                    waveSyncAmt.patternValue = parameter;
                    break;
                case 'X':
                    downsampleFactor = parameter;
                    break;
                case 'H':
                    SetFilter(Helpers.Map(parameter / 16, 0, 16, 1, 0.3f), Helpers.Map(parameter % 16, 0, 16, 1, 8));
                    break;
            }
        }

        public void SetVolume(int vol) {
            channelVolume = vol;
        }

        private void ResetEnvelopePlayers() {
            int i = 0;
            foreach (EnvelopePlayer player in envelopePlayers.Values) {
                player.SetEnvelope(defaultEnvelopes[i]);
                i++;
            }
        }

        public void SetMacro(int id) {

            if (id < App.CurrentModule.Instruments.Count) {
                Instrument instrument = App.CurrentModule.Instruments[id];
                CurrentInstrument = instrument;
                ResetEnvelopePlayers();
                foreach (Envelope envelope in instrument.envelopes) {
                    envelopePlayers[envelope.Type].SetEnvelope(envelope);
                }
                if (currentInstrumentID != id) {
                    currentInstrumentID = id;
                    if (this.id < 0) {
                        ResetModulations();
                    }

                    if (instrument is SampleInstrument) {
                        _time = 0;
                        instrumentAsSample = CurrentInstrument as SampleInstrument;
                    }
                }
                foreach (Envelope envelope in instrument.envelopes) {
                    envelopePlayers[envelope.Type].Start();
                }
                if (CurrentInstrument is WaveInstrument) {
                    waveIndex.envelopeValue = envelopePlayers[Envelope.EnvelopeType.Wave].HasActiveEnvelopeData ? envelopePlayers[Envelope.EnvelopeType.Wave].Value : -1;
                    waveBlendAmt.envelopeValue = envelopePlayers[Envelope.EnvelopeType.WaveBlend].HasActiveEnvelopeData
                        ? envelopePlayers[Envelope.EnvelopeType.WaveBlend].Value
                        : -1;
                    waveStretchAmt.envelopeValue = envelopePlayers[Envelope.EnvelopeType.WaveStretch].HasActiveEnvelopeData
                        ? envelopePlayers[Envelope.EnvelopeType.WaveStretch].Value
                        : -1;
                    _waveStretchSmooth = waveStretchAmt.Value;
                    waveSyncAmt.envelopeValue = envelopePlayers[Envelope.EnvelopeType.WaveSync].HasActiveEnvelopeData
                        ? envelopePlayers[Envelope.EnvelopeType.WaveSync].Value
                        : -1;
                    waveFmAmt.envelopeValue = envelopePlayers[Envelope.EnvelopeType.WaveFM].HasActiveEnvelopeData ? envelopePlayers[Envelope.EnvelopeType.WaveFM].Value : -1;
                    _fmSmooth = waveFmAmt.Value;
                }
                else {
                    _time = 0;
                }
            }

        }

        public void Release() {
            if (_state == VoiceState.On) {
                foreach (Envelope envelope in CurrentInstrument.envelopes) {
                    envelopePlayers[envelope.Type].Release();
                }
                _state = VoiceState.Release;
            }
        }

        /// <summary>
        /// Releases the note if it has a volume envelope with a release, otherwise cut the note.
        /// </summary>
        public void PreviewCut() {
            if (envelopePlayers[Envelope.EnvelopeType.Volume].HasActiveEnvelopeData && envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeToPlay.HasRelease) {
                Release();
            }
            else {
                Cut();
                foreach (Envelope envelope in CurrentInstrument.envelopes) {
                    envelopePlayers[envelope.Type].Release();
                }
            }
        }

        public void Cut() {
            _state = VoiceState.Off;
        }

        public void OnNoteEnd() {
            vibratoTime = 0;
            arpCounter = 0;
            noteOn = false;
        }

        public void Reset() {
            CurrentInstrument = new WaveInstrument();
            currentInstrumentID = 0;
            tickEvents.Clear();
            noteOn = false;
            _state = VoiceState.Off;
            channelVolume = 99;
            detuneOffset = 0;
            pitchFallSpeed = 0;
            pitchFallOffset = 0;
            vibratoOffset = 0;
            volumeSlideSpeed = 0;
            _time = 0.0;
            arpCounter = 0;
            arpeggionote2 = 0;
            arpeggionote3 = 0;
            vibratoIntensity = 0;
            vibratoOffset = 0;
            vibratoSpeed = 0;
            vibratoTime = 0;
            tremoloIntensity = 0;
            tremoloMultiplier = 1f;
            tremoloSpeed = 0;
            tremoloTime = 0;
            stereoPhaseOffset = 0;
            waveIndex.Reset(0);
            waveBlendAmt.Reset(0);
            waveFmAmt.Reset(0);
            _fmSmooth = 0;
            waveStretchAmt.Reset(0);
            _waveStretchSmooth = 0;
            lastNote = channelNote;
            channelNotePorta = channelNote;
            portaSpeed = 0;
            portaTime = 0;
            SampleStartOffset = 0;
            waveSyncAmt.Reset(0);
            bendSpeed = 0;
            targetBendAmt = 0;
            bendOffset = 0;
            downsampleCounter = 0;
            downsampleFactor = 0;
            SetFilter(1, 1);
            SetPanning(0.5f);
        }

        public void SetFilter(float cutoffValue, float resonance) {
            targetFilterCutoffFrequency = Helpers.NoteToFrequency(cutoffValue * 135) / 22050f;
            filterResonance = resonance;
            UpdateFilter();
        }

        public void UpdateFilter() {
            _filterCutoffFrequency = targetFilterCutoffFrequency * Math.Min(20000, AudioEngine.SampleRate / 2f);
            stereoBiQuadFilter.SetLowpassFilter(AudioEngine.TrueSampleRate, _filterCutoffFrequency, filterResonance);
        }

        public void ResetModulations() {
            waveBlendAmt.Reset(0);
            _fmSmooth = 0;
            waveFmAmt.Reset(0);
            waveStretchAmt.Reset(0);
            _waveStretchSmooth = 0;
            waveSyncAmt.Reset(0);
        }

        public void SetWave(int waveIndex) {
            this.waveIndex.Reset(waveIndex);
        }

        public void TriggerNote(int midiNum) {
            pitchFallOffset = 0;
            if (!noteOn) {
                _fadeMultiplier = 1f;
            }
            if (_state == VoiceState.Off) {
                channelNotePorta = midiNum;
                _time = 0.0;
            }
            if (_state == VoiceState.Release && envelopePlayers[Envelope.EnvelopeType.Volume].Value == 0) {
                _time = 0.0;
            }
            targetBendAmt = 0;
            bendOffset = 0;

            if (channelNote != midiNum || _state != VoiceState.On) {
                lastNote = channelNotePorta;
                channelNote = midiNum;
                if (portaSpeed == 0) {
                    channelNotePorta = channelNote;
                }

                portaTime = 0;
                noteOn = true;
                _state = VoiceState.On;
            }
            CurrentFrequency = Helpers.NoteToFrequency(TotalPitch);
        }

        private void SetPanning(float val) {
            if (val > 0.98f) {
                val = 1.0f;
            }

            CurrentPan = val;
            _rightAmp = (float)Math.Sqrt(val);
            _leftAmp = (float)Math.Sqrt(1.0f - val);
        }

        /// <summary>
        /// The amplitude of this channel from 0.0 - 1.0
        /// </summary>
        public float CurrentAmplitude {
            get {
                return CurrentInstrument is WaveInstrument
                    ? TotalAmplitude * (_state == VoiceState.Off ? 0 : 1)
                    : TotalAmplitude * (_sampleVolume / 1.5f + 0.01f) * (_state == VoiceState.Off ? 0 : 1);
            }
        }

        public float CurrentAmplitudeAsWave {
            get {
                return TotalAmplitude * (_state == VoiceState.Off ? 0 : 1);
            }
        }

        /// <summary>
        /// Pan value from 0.0-1.0, 0.5f is center
        /// </summary>
        public float CurrentPan { get; private set; } = 0.5f;

        /// <summary>
        /// This channel's current MIDI pitch
        /// </summary>
        public float CurrentPitch { get { return TotalPitch; } }
        /// <summary>
        /// This channel's current frequency in hertz
        /// </summary>
        public float CurrentFrequency { get; private set; }

        public float EvaluateWave(float time) {
            float s = waveSyncAmt.AdditiveValue / 99f * 8 + 1;
            if (time is < 0 or >= 1) {
                time = Helpers.Mod(time, 1);
            }
            time *= s;
            if (time is < 0 or >= 1) {
                time = Helpers.Mod(time, 1);
            }
            return _fmSmooth > 0.001f
                ? CurrentWave.GetSampleMorphed(time + App.CurrentModule.WaveBank[(WaveIndex + 1) % 100].GetSampleAtPosition(time) * (_fmSmooth / 20f * (_fmSmooth / 20f)) / 2f, App.CurrentModule.WaveBank[(WaveIndex + 1) % 100], WaveMorphPosition, _waveStretchSmooth / 100f)
                : CurrentWave.GetSampleMorphed(time, App.CurrentModule.WaveBank[(WaveIndex + 1) % 100], WaveMorphPosition, _waveStretchSmooth / 100f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ContinuousTick(float deltaTime) {
            if (tremoloIntensity > 0) {
                tremoloTime += deltaTime * tremoloSpeed * 6f;
                tremoloMultiplier = 1f + (float)(Math.Sin(tremoloTime * 1) / 2 - 0.5f) * tremoloIntensity / 16f;
            }
            else {
                tremoloMultiplier = 1;
            }
            if (vibratoIntensity > 0) {
                vibratoTime += deltaTime * vibratoSpeed * 3f;
                vibratoOffset = (float)Math.Sin(vibratoTime * 2) * vibratoIntensity / 5f;
            }
            else {
                vibratoOffset = 0;
            }

            if (pitchFallSpeed != 0) {
                pitchFallOffset += pitchFallSpeed * deltaTime * 2;
            }

            if (portaTime < 2) {
                portaTime += deltaTime * (portaSpeed == 0 ? AudioEngine.SampleRate : portaSpeed);
            }

            if (channelNotePorta > channelNote) {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta < channelNote) {
                    channelNotePorta = channelNote;
                }
            }
            else if (channelNotePorta < channelNote) {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta > channelNote) {
                    channelNotePorta = channelNote;
                }
            }

            if (bendOffset > targetBendAmt) {
                bendOffset -= deltaTime * bendSpeed;
                if (bendOffset < targetBendAmt) {
                    bendOffset = targetBendAmt;
                }
            }
            else if (bendOffset < targetBendAmt) {
                bendOffset += deltaTime * bendSpeed;
                if (bendOffset > targetBendAmt) {
                    bendOffset = targetBendAmt;
                }
            }
            if (envelopePlayers[Envelope.EnvelopeType.Pitch].HasActiveEnvelopeData && !envelopePlayers[Envelope.EnvelopeType.Pitch].EnvelopeEnded) {
                pitchFallOffset += envelopePlayers[Envelope.EnvelopeType.Pitch].Value * deltaTime * 4;
            }

            if (lastPitch != TotalPitch) {
                lastPitch = TotalPitch;
                CurrentFrequency = Helpers.NoteToFrequency(lastPitch);
            }

        }

        public void NextTick() {

            tickNum++;
            // step envelopes
            foreach (Envelope envelope in CurrentInstrument.envelopes) {
                EnvelopePlayer player = envelopePlayers[envelope.Type];
                if (player.HasActiveEnvelopeData) {
                    player.Step();
                    if (!player.EnvelopeEnded && _state != VoiceState.Off && player.Value >= 0) {
                        switch (player.Type) {
                            case Envelope.EnvelopeType.Wave:
                                waveIndex.envelopeValue = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveBlend:
                                waveBlendAmt.envelopeValue = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveStretch:
                                waveStretchAmt.envelopeValue = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveSync:
                                waveSyncAmt.envelopeValue = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveFM:
                                waveFmAmt.envelopeValue = player.Value;
                                break;
                        }
                    }
                }
                else {
                    switch (player.Type) {
                        case Envelope.EnvelopeType.Wave:
                            waveIndex.envelopeValue = -1;
                            break;
                        case Envelope.EnvelopeType.WaveBlend:
                            waveBlendAmt.envelopeValue = -1;
                            break;
                        case Envelope.EnvelopeType.WaveStretch:
                            waveStretchAmt.envelopeValue = -1;
                            break;
                        case Envelope.EnvelopeType.WaveSync:
                            waveSyncAmt.envelopeValue = -1;
                            break;
                        case Envelope.EnvelopeType.WaveFM:
                            waveFmAmt.envelopeValue = -1;
                            break;
                    }
                }
            }

            channelVolume += volumeSlideSpeed;
            if (channelVolume > 99) {
                channelVolume = 99;
            }

            if (channelVolume < 0) {
                channelVolume = 0;
            }

            for (int i = 0; i < tickEvents.Count; i++) {
                TickEvent tickEvent = tickEvents[i];
                if (tickEvent != null) {
                    if (tickEvent.countdown <= 0) {
                        DoEvent(tickEvent);
                        tickEvents.Remove(tickEvent);
                        i--;
                    }
                    else {
                        tickEvent.Update();
                    }
                }
            }
            if (arpeggionote2 == arpeggionote3 && arpeggionote2 == 0) {
                arpeggioOffset = 0;
            }
            else {
                switch (arpCounter) {
                    case 0:
                        arpeggioOffset = 0;
                        break;
                    case 1:
                        arpeggioOffset = arpeggionote2;
                        break;
                    case 2:
                        arpeggioOffset = arpeggionote3;
                        break;
                }
                arpCounter++;
                if (arpCounter > 2) {
                    arpCounter = 0;
                }
            }

            if (envelopePlayers[Envelope.EnvelopeType.Volume].HasActiveEnvelopeData) {
                try {
                    if (envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeEnded &&
                        envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeToPlay.values[envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeToPlay.Length - 1] == 0) {
                        Cut();
                    }
                } catch {
                }
            }

            bassBoost = 0.6f * MathF.Pow(1.025f, -TotalPitch) + 0.975f;

        }

        private void DoEvent(TickEvent t) {
            if (t.eventType == TickEventType.Instrument) {
                SetMacro(t.value);
            }
            if (t.eventType == TickEventType.Note) {
                if (t.value == WTPattern.EVENT_NOTE_CUT) {
                    Cut();
                }
                else if (t.value == WTPattern.EVENT_NOTE_RELEASE) {
                    Release();
                }
                else if (t.value != WTPattern.EVENT_EMPTY) {
                    TriggerNote(t.value);
                }
            }
            if (t.eventType == TickEventType.Volume) {
                SetVolume(t.value);
            }
            if (t.eventType == TickEventType.Effect) {
                ApplyEffect((char)t.value, t.value2);
            }
        }

        public void ProcessSingleSample(out float left, out float right, float continuousDelta) {
            ContinuousTick(continuousDelta);
            left = right = 0;
            float freqCut = 1;
            float delta = 1f / AudioEngine.TrueSampleRate * CurrentFrequency;
            if (noteOn) {
                float sampleL;
                float sampleR;
                if (_state == VoiceState.Off) {
                    _fadeMultiplier /= 1.002f;
                    if (_fadeMultiplier < 0.001f) {
                        noteOn = false;
                    }
                }
                else {
                    _fadeMultiplier = 1;
                    _time += delta;
                }
                sampleL = 0;
                sampleR = 0;
                if (CurrentInstrument != null) {
                    if (CurrentInstrument is WaveInstrument) {
                        if (_time > 1) {
                            _time -= 1;
                        }

                        if (stereoPhaseOffset != 0) {
                            sampleL = EvaluateWave((float)_time - stereoPhaseOffset);
                            sampleR = EvaluateWave((float)_time + stereoPhaseOffset);
                        }
                        else {
                            sampleR = sampleL = EvaluateWave((float)_time);
                        }
                    }
                    else {
                        if (instrumentAsSample != CurrentInstrument) {
                            instrumentAsSample = CurrentInstrument as SampleInstrument;
                        }
                        instrumentAsSample.sample.SampleTick((float)_time, (int)(stereoPhaseOffset * 100), SampleStartOffset / 100f, out sampleL, out sampleR);
                        sampleL *= 1.25f;
                        sampleR *= 1.25f;
                        SamplePlaybackPosition = instrumentAsSample.sample.currentPlaybackPosition;
                        if (Math.Abs(sampleL) > _sampleVolume) {
                            _sampleVolume = Math.Abs(sampleL);
                        }
                        if (Math.Abs(sampleR) > _sampleVolume) {
                            _sampleVolume = Math.Abs(sampleR);
                        }
                    }
                }

                downsampleCounter++;
                if (downsampleCounter > downsampleFactor) {
                    lastSampleL = sampleL;
                    lastSampleR = sampleR;
                    downsampleCounter = 0;
                }
                else {
                    sampleL = lastSampleL;
                    sampleR = lastSampleR;
                }

                _volumeSmooth += (TotalAmplitude - _volumeSmooth) * 0.02f;
                _fmSmooth += (waveFmAmt.AdditiveValue - _fmSmooth) * 0.02f;
                _waveStretchSmooth += (waveStretchAmt.AdditiveValue - _waveStretchSmooth) * 0.02f;

                float l = sampleL * _volumeSmooth * _leftAmp * _fadeMultiplier * freqCut;
                float r = sampleR * _volumeSmooth * _rightAmp * _fadeMultiplier * freqCut;
                stereoBiQuadFilter.Transform(l, r, out l, out r);
                left = l * 0.225f * bassBoost;
                right = r * 0.225f * bassBoost;
                if (id >= 0 && IsMuted) {
                    left = 0;
                    right = 0;
                }
            }
        }
    }
}
