using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Tracker;

namespace WaveTracker.Audio {
    public class Channel {

        const int OVERSAMPLE = 2;
        int id;
        public bool IsMuted { get; set; }
        public Wave currentWave;
        public int WaveIndex { get; private set; }
        public List<TickEvent> tickEvents;
        float TotalAmplitude {
            get { return tremoloMultiplier * (channelVolume / 99f) * (envelopePlayers[Envelope.EnvelopeType.Volume].Value / 99f); }
        } // * currentMacro.GetState().volumeMultiplier;
        float TotalPitch => Math.Clamp(channelNotePorta + bendOffset + vibratoOffset + pitchFallOffset + arpeggioOffset + detuneOffset + envelopePlayers[Envelope.EnvelopeType.Arpeggio].Value, -12, 131);

        public float WaveMorphPosition => waveBlendAmt / 99f;
        float bassBoost;

        /// <summary>
        /// value from 0.0-1.0, 0.5f is center
        /// </summary>
        float panValue = 0.5f;
        bool noteOn;

        int arpeggioOffset;
        int arpeggionote2;
        int arpeggionote3;
        int arpCounter;

        public float stereoPhaseOffset;

        float vibratoOffset; // 4xx command
        double vibratoTime;
        int volumeSlideSpeed;
        float vibratoSpeed;
        float vibratoIntensity;
        float tremoloMultiplier = 1f; // 7xx command
        float tremoloTime;
        float tremoloSpeed;
        float tremoloIntensity;
        float pitchFallOffset; // 1xx and 2xx commands
        float pitchFallSpeed; // 1xx and 2xx commands
        float portaSpeed; // 3xx command (in samples)
        float portaTime;
        float bendSpeed; // 3xx command (in samples)
        float bendOffset;
        int targetBendAmt;
        float channelNotePorta;
        float detuneOffset; // Pxx command
        int waveBlendAmt; // Ixx command
        float fmAmt; // Mxx command
        int waveStretchAmt; // Jxx command
        int waveSyncAmt; // Kxx command
        public int SampleStartOffset { get; private set; } // Yxx command
        int channelVolume; // volume column
        int channelNote; // notes column
        float lastNote;
        public Instrument currentInstrument; // instrument column
        SampleInstrument instrumentAsSample;
        public Dictionary<Envelope.EnvelopeType, EnvelopePlayer> envelopePlayers;
        public float _sampleVolume;
        float lastPitch;

        private float _frequency;
        private double _time;
        private float _leftAmp;
        private float _rightAmp;
        float _fadeMultiplier = 1;
        float _volumeSmooth;
        float _fmSmooth;
        public float coarseSampleVolume;
        public int tickNum;
        int currentInstrumentID;
        private enum VoiceState { On, Off, Release }
        private VoiceState _state;
        public bool IsPlaying => _state == VoiceState.On;

        public Channel(int id) {
            this.id = id;
            tickEvents = new List<TickEvent>();
            envelopePlayers = new Dictionary<Envelope.EnvelopeType, EnvelopePlayer>();
            foreach (Envelope.EnvelopeType envelopeType in Enum.GetValues(typeof(Envelope.EnvelopeType))) {
                envelopePlayers.Add(envelopeType, new EnvelopePlayer(envelopeType));
            }

            //envelopePlayers = new EnvelopePlayer[Enum.GetValues(typeof(Envelope.EnvelopeType)).Length];
            //for (int i = 0; i < envelopePlayers.Length; i++) {
            //    envelopePlayers[i] = new EnvelopePlayer();
            //}
            Reset();
        }

        public float SampleTime => (float)_time;
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
            switch (command) // 4XY
            {
                case '4':
                    if (parameter == 0)
                        vibratoTime = 0;
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
                    waveBlendAmt = parameter;
                    break;
                case 'M':
                    fmAmt = parameter / 20f;
                    break;
                case 'J':
                    waveStretchAmt = parameter;
                    break;
                case 'Y':
                    SampleStartOffset = parameter;
                    break;
                case 'K':
                    waveSyncAmt = parameter;
                    break;
            }
        }

        public void SetVolume(int vol) {
            channelVolume = vol;
        }

        void ResetEnvelopePlayers() {
            foreach (EnvelopePlayer player in envelopePlayers.Values) {
                player.SetEnvelope(null);
            }
        }

        public void SetMacro(int id) {

            if (id < App.CurrentModule.Instruments.Count) {
                Instrument instrument = App.CurrentModule.Instruments[id];
                currentInstrument = instrument;
                ResetEnvelopePlayers();
                foreach (Envelope envelope in instrument.envelopes) {
                    envelopePlayers[envelope.Type].SetEnvelope(envelope);
                }
                //volumeEnv.toPlay = instrument.volumeEnvelope;
                //arpEnv.toPlay = instrument.arpEnvelope;
                //pitchEnv.toPlay = instrument.pitchEnvelope;
                //waveEnv.toPlay = instrument.waveEnvelope;
                //waveModEnv.toPlay = instrument.waveModEnvelope;
                if (currentInstrumentID != id) {
                    currentInstrumentID = id;
                    if (this.id < 0)
                        ResetModulations();
                    if (instrument is SampleInstrument) {
                        _time = 0;
                        instrumentAsSample = currentInstrument as SampleInstrument;
                    }
                }
                foreach (Envelope envelope in instrument.envelopes) {
                    envelopePlayers[envelope.Type].Start();
                }
                if (currentInstrument is WaveInstrument) {
                    if (envelopePlayers[Envelope.EnvelopeType.Wave].HasActiveEnvelopeData) {
                        SetWave(envelopePlayers[Envelope.EnvelopeType.Wave].Value);
                    }
                    if (envelopePlayers[Envelope.EnvelopeType.WaveBlend].HasActiveEnvelopeData) {
                        waveBlendAmt = envelopePlayers[Envelope.EnvelopeType.WaveBlend].Value;
                    }
                    if (envelopePlayers[Envelope.EnvelopeType.WaveStretch].HasActiveEnvelopeData) {
                        waveStretchAmt = envelopePlayers[Envelope.EnvelopeType.WaveFM].Value;
                    }
                    if (envelopePlayers[Envelope.EnvelopeType.WaveSync].HasActiveEnvelopeData) {
                        waveSyncAmt = envelopePlayers[Envelope.EnvelopeType.WaveSync].Value;
                    }
                    if (envelopePlayers[Envelope.EnvelopeType.WaveFM].HasActiveEnvelopeData) {
                        fmAmt = envelopePlayers[Envelope.EnvelopeType.WaveFM].Value / 20f;
                        _fmSmooth = fmAmt;
                    }
                }
                //if (currentInstrument is WaveInstrument) {
                //    if (waveEnv.toPlay.IsActive)
                //        if (!waveEnv.EnvelopeEnded)
                //            SetWave(waveEnv.Evaluate());
                //    if (waveModEnv.toPlay.IsActive)
                //        if (!waveModEnv.EnvelopeEnded) {
                //            if (currentInstrument.waveModType == 0) {
                //                waveBlendAmt = waveModEnv.Evaluate();
                //            }
                //            else if (currentInstrument.waveModType == 1) {
                //                waveStretchAmt = waveModEnv.Evaluate();
                //            }
                //            else if (currentInstrument.waveModType == 2) {
                //                fmAmt = waveModEnv.Evaluate() / 20f;
                //                _fmSmooth = fmAmt;
                //            }
                //        }
                //}
            }

        }

        public void Release() {
            if (_state == VoiceState.On) {
                foreach (Envelope envelope in currentInstrument.envelopes) {
                    envelopePlayers[envelope.Type].Release();
                }
                //volumeEnv.Release();
                //pitchEnv.Release();
                //arpEnv.Release();
                //waveEnv.Release();
                //waveModEnv.Release();
                _state = VoiceState.Release;
            }
        }

        public void PreviewCut() {
            if (envelopePlayers[Envelope.EnvelopeType.Volume].HasActiveEnvelopeData && currentInstrument.envelopes[0].HasRelease) {
                Release();
            }
            else {
                Cut();
                foreach (Envelope envelope in currentInstrument.envelopes) {
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
            currentInstrument = new WaveInstrument();
            currentInstrumentID = 0;
            //volumeEnv = new EnvelopePlayer();
            //arpEnv = new EnvelopePlayer();
            //pitchEnv = new EnvelopePlayer();
            //waveEnv = new EnvelopePlayer();
            //waveModEnv = new EnvelopePlayer();
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
            waveBlendAmt = 0;
            fmAmt = 0;
            _fmSmooth = 0;
            waveStretchAmt = 0;
            SetWave(0);
            lastNote = channelNote;
            channelNotePorta = channelNote;
            portaSpeed = 0;
            portaTime = 0;
            SampleStartOffset = 0;
            waveSyncAmt = 0;
            bendSpeed = 0;
            targetBendAmt = 0;
            bendOffset = 0;
            SetPanning(0.5f);
        }

        public void ResetModulations() {
            waveBlendAmt = 0;
            _fmSmooth = 0;
            fmAmt = 0;
            waveStretchAmt = 0;
            waveSyncAmt = 0;
        }

        public void SetWave(int waveIndex) {
            WaveIndex = waveIndex;
            currentWave = ChannelManager.waveBank.GetWave(waveIndex);
        }


        public void TriggerNote(int midiNum) {
            pitchFallOffset = 0;
            if (!noteOn) {
                _fadeMultiplier = 1f;
            }
            if (_state == VoiceState.Off) {
                _time = 0.0;
            }
            if (_state == VoiceState.Release && envelopePlayers[Envelope.EnvelopeType.Volume].Value == 0)
                _time = 0.0;
            targetBendAmt = 0;
            bendOffset = 0;
            if (channelNote != midiNum || _state != VoiceState.On) {
                lastNote = channelNotePorta;
                channelNote = midiNum;
                if (portaSpeed == 0)
                    channelNotePorta = channelNote;
                portaTime = 0;
                noteOn = true;
                _state = VoiceState.On;
            }
            //if (envelopePlayersDict[Envelope.EnvelopeType.Arpeggio].HasActiveEnvelopeData)
            //    envelopePlayersDict[Envelope.EnvelopeType.Arpeggio].Start();
            if (currentInstrument is SampleInstrument)
                _time = 0;
            _frequency = Helpers.NoteToFrequency(TotalPitch);


        }
        private void SetPanning(float val) {
            if (val > 0.98f)
                val = 1.0f;
            panValue = val;
            _rightAmp = (float)Math.Sqrt(val);
            _leftAmp = (float)Math.Sqrt(1.0f - val);
        }

        /// <summary>
        /// The amplitude of this channel from 0.0 - 1.0
        /// </summary>
        public float CurrentAmplitude {
            get {
                if (currentInstrument is WaveInstrument)
                    return TotalAmplitude * (_state == VoiceState.Off ? 0 : 1);
                else
                    return TotalAmplitude * (_sampleVolume / 1.5f + 0.01f) * (_state == VoiceState.Off ? 0 : 1);
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
        public float CurrentPan { get { return panValue; } }

        /// <summary>
        /// This channel's current MIDI pitch
        /// </summary>
        public float CurrentPitch { get { return TotalPitch; } }
        /// <summary>
        /// This channel's current frequency in hertz
        /// </summary>
        public float CurrentFrequency { get { return _frequency; } }

        public float EvaluateWave(float time) {
            // float t = time;
            float s = (waveSyncAmt / 99f * 8) + 1;
            time = (s * (time % 1)) % 1;
            if (_fmSmooth > 0.001f)
                return currentWave.GetSampleMorphed(time + App.CurrentModule.WaveBank[(WaveIndex + 1) % 100].GetSampleAtPosition(time) * (_fmSmooth * _fmSmooth) / 2f, App.CurrentModule.WaveBank[(WaveIndex + 1) % 100], waveBlendAmt / 99f, waveStretchAmt / 100f);
            else
                return currentWave.GetSampleMorphed(time, App.CurrentModule.WaveBank[(WaveIndex + 1) % 100], waveBlendAmt / 99f, waveStretchAmt / 100f);
        }

        void ContinuousTick(float deltaTime) {
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
            else
                vibratoOffset = 0;
            if (pitchFallSpeed != 0)
                pitchFallOffset += pitchFallSpeed * deltaTime * 2;
            if (portaTime < 2)
                portaTime += deltaTime * (portaSpeed == 0 ? AudioEngine.SAMPLE_RATE : portaSpeed);

            if (channelNotePorta > channelNote) {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta < channelNote)
                    channelNotePorta = channelNote;
            }
            else if (channelNotePorta < channelNote) {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta > channelNote)
                    channelNotePorta = channelNote;
            }

            if (bendOffset > targetBendAmt) {
                bendOffset -= deltaTime * bendSpeed;
                if (bendOffset < targetBendAmt)
                    bendOffset = targetBendAmt;
            }
            else if (bendOffset < targetBendAmt) {
                bendOffset += deltaTime * bendSpeed;
                if (bendOffset > targetBendAmt)
                    bendOffset = targetBendAmt;
            }
            if (envelopePlayers[Envelope.EnvelopeType.Pitch].HasActiveEnvelopeData && !envelopePlayers[Envelope.EnvelopeType.Pitch].EnvelopeEnded)
                pitchFallOffset += envelopePlayers[Envelope.EnvelopeType.Pitch].Value * deltaTime * 4;
            if (lastPitch != TotalPitch) {
                lastPitch = TotalPitch;
                _frequency = Helpers.NoteToFrequency(lastPitch);
            }

        }

        public void NextTick() {

            tickNum++;
            // step envelopes
            foreach (Envelope envelope in currentInstrument.envelopes) {
                EnvelopePlayer player = envelopePlayers[envelope.Type];
                if (player.HasActiveEnvelopeData) {
                    player.Step();
                    if (!player.EnvelopeEnded && _state != VoiceState.Off && player.Value >= 0) {
                        switch (player.Type) {
                            case Envelope.EnvelopeType.Wave:
                                SetWave(player.Value);
                                break;
                            case Envelope.EnvelopeType.WaveBlend:
                                waveBlendAmt = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveStretch:
                                waveStretchAmt = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveSync:
                                waveSyncAmt = player.Value;
                                break;
                            case Envelope.EnvelopeType.WaveFM:
                                fmAmt = player.Value / 20f;
                                break;
                        }
                    }
                    //if (envelopePlayer.envelopeToPlay != null) {
                    //    if (envelopePlayer.envelopeToPlay.IsActive) {
                    //        envelopePlayer.Step();
                    //        if (!envelopePlayer.EnvelopeEnded && _state != VoiceState.Off) {
                    //            if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.Wave) {
                    //                SetWave(envelopePlayer.Evaluate());
                    //            }
                    //            else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveBlend) {
                    //                waveBlendAmt = envelopePlayer.Evaluate();
                    //            }
                    //            else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveStretch) {
                    //                waveStretchAmt = envelopePlayer.Evaluate();
                    //            }
                    //            else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveSync) {
                    //                syncAmt = envelopePlayer.Evaluate();
                    //            }
                    //            else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveFM) {
                    //                fmAmt = envelopePlayer.Evaluate() / 20f;
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            //if (currentInstrument is WaveInstrument) {
            //    waveEnv.Step();
            //    if (waveEnv.toPlay.IsActive)
            //        if (!waveEnv.EnvelopeEnded && _state != VoiceState.Off)
            //            SetWave(waveEnv.Evaluate());

            //}
            //if (currentInstrument is WaveInstrument) {
            //    waveModEnv.Step();
            //    if (waveModEnv.toPlay.IsActive)
            //        if (!waveModEnv.EnvelopeEnded && _state != VoiceState.Off) {
            //            if (currentInstrument.waveModType == 0) {
            //                waveMorphAmt = waveModEnv.Evaluate();
            //            }
            //            else if (currentInstrument.waveModType == 1) {
            //                waveBendAmt = waveModEnv.Evaluate();
            //            }
            //            else if (currentInstrument.waveModType == 2) {
            //                fmAmt = waveModEnv.Evaluate() / 20f;
            //            }
            //        }

            //}
            //if (volumeEnv.toPlay.IsActive)
            //    volumeEnv.Step();
            //if (arpEnv.toPlay.IsActive)
            //    arpEnv.Step();
            //if (pitchEnv.toPlay.IsActive)
            //    pitchEnv.Step();

            channelVolume += volumeSlideSpeed;
            if (channelVolume > 99)
                channelVolume = 99;
            if (channelVolume < 0)
                channelVolume = 0;
            for (int i = 0; i < tickEvents.Count; i++) {
                TickEvent t = tickEvents[i];
                if (t != null) {
                    if (t.countdown <= 0) {
                        DoEvent(t);
                        tickEvents.Remove(t);
                        i--;
                    }
                    else {
                        t.Update();
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
                if (arpCounter > 2)
                    arpCounter = 0;
            }


            if (envelopePlayers[Envelope.EnvelopeType.Volume].HasActiveEnvelopeData)
                try {
                    if (envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeEnded &&
                        envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeToPlay.values[envelopePlayers[Envelope.EnvelopeType.Volume].EnvelopeToPlay.Length - 1] == 0)
                        Cut();
                } catch {
                    // Cut();
                }
            //arpEnvelopeResult = envelopePlayers[1].Evaluate();
            bassBoost = 0.6f * MathF.Pow(1.025f, -TotalPitch) + 0.975f;
            //bassBoost = 1;

        }

        void DoEvent(TickEvent t) {
            if (t.eventType == TickEventType.Instrument) {
                SetMacro(t.value);
            }
            if (t.eventType == TickEventType.Note) {
                if (t.value == WTPattern.EVENT_NOTE_CUT)
                    Cut();
                else if (t.value == WTPattern.EVENT_NOTE_RELEASE)
                    Release();
                else if (t.value != WTPattern.EVENT_EMPTY)
                    TriggerNote(t.value);
            }
            if (t.eventType == TickEventType.Volume) {
                SetVolume(t.value);
            }
            if (t.eventType == TickEventType.Effect) {
                ApplyEffect((char)t.value, t.value2);
            }
        }

        public void ProcessSingleSample(out float left, out float right, bool continuousTick, float continuousDelta) {
            left = right = 0;

            float freqCut = 1;
            //if (_frequency > 15804) {
            //    //freqCut = 0;
            //    _frequency = 15804;
            //    //_state = VoiceState.Off;
            //}
            float delta = 1f / (OVERSAMPLE * AudioEngine.SAMPLE_RATE) * _frequency;
            if (continuousTick)
                ContinuousTick(continuousDelta);
            if (noteOn) {
                float sampleSumL = 0;
                float sampleSumR = 0;
                float sampleL;
                float sampleR;
                for (int i = 0; i < 2; ++i) {
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
                    if (currentInstrument != null) {
                        if (currentInstrument is WaveInstrument) {
                            if (_time > 1)
                                _time -= 1;
                            if (stereoPhaseOffset != 0) {
                                sampleL = EvaluateWave((float)_time - stereoPhaseOffset);
                                sampleR = EvaluateWave((float)_time + stereoPhaseOffset);
                            }
                            else {
                                sampleR = sampleL = EvaluateWave((float)_time);
                            }
                        }
                        else {
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
                    sampleSumL += sampleL;
                    sampleSumR += sampleR;
                }
                sampleL = sampleSumL / OVERSAMPLE;
                sampleR = sampleSumR / OVERSAMPLE;
                _volumeSmooth += (TotalAmplitude - _volumeSmooth) * 0.02f;
                _fmSmooth += (fmAmt - _fmSmooth) * 0.05f;

                // float f = (float)Math.Pow(_volumeSmooth, 1.25);
                float f = _volumeSmooth;
                float l = sampleL * f * _leftAmp * _fadeMultiplier * freqCut;
                float r = sampleR * f * _rightAmp * _fadeMultiplier * freqCut;
                //if (AudioEngine.quantizeAmplitude) {
                //    if (_volumeSmooth < 0.005f) {
                //        l = 0;
                //        r = 0;
                //    }
                //    int quantamt = 8;
                //    l = (float)(Math.Ceiling(l * quantamt)) / (float)quantamt;
                //    r = (float)(Math.Ceiling(r * quantamt)) / (float)quantamt;
                //}
                left = l * 0.2f * bassBoost;
                right = r * 0.2f * bassBoost;
                if (id >= 0 && IsMuted) {
                    left = 0;
                    right = 0;
                }

            }

        }
    }
}
