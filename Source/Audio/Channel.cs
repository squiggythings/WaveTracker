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
        public int waveIndex { get; private set; }
        public List<TickEvent> tickEvents;
        float totalAmplitude {
            get { return tremoloMultiplier * (channelVolume / 99f) * (envelopePlayers[0].Evaluate() / 99f); }
        } // * currentMacro.GetState().volumeMultiplier;
        float totalPitch => Math.Clamp(channelNotePorta + bendOffset + vibratoOffset + pitchFallOffset + arpeggioOffset + detuneOffset + arpEnvelopeResult, -12, 131);

        public float waveMorphPosition => waveBlendAmt / 99f;
        float bassBoost;

        int arpEnvelopeResult;
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
        int syncAmt; // Kxx command
        public int SampleStartOffset { get; private set; } // Yxx command
        int channelVolume; // volume column
        int channelNote; // notes column
        float lastNote;
        public Instrument currentInstrument; // instrument column
        SampleInstrument instrumentAsSample;
        //public EnvelopePlayer volumeEnv;
        //public EnvelopePlayer arpEnv;
        //public EnvelopePlayer pitchEnv;
        //public EnvelopePlayer waveEnv;
        //public EnvelopePlayer waveModEnv;
        public EnvelopePlayer[] envelopePlayers;
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
        int macroID;
        private enum VoiceState { On, Off, Release }
        private VoiceState _state;
        public bool IsPlaying => _state == VoiceState.On;

        public Channel(int id) {
            this.id = id;
            tickEvents = new List<TickEvent>();
            envelopePlayers = new EnvelopePlayer[8];
            for (int i = 0; i < envelopePlayers.Length; i++) {
                envelopePlayers[i] = new EnvelopePlayer();
            }
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
                    syncAmt = parameter;
                    break;
            }
        }

        public void SetVolume(int vol) {
            channelVolume = vol;
        }

        public void SetMacro(int id) {

            if (id < App.CurrentModule.Instruments.Count) {
                Instrument instrument = App.CurrentModule.Instruments[id];
                currentInstrument = instrument;

                for (int i = 0; i < instrument.envelopes.Count; i++) {
                    envelopePlayers[i].envelopeToPlay = instrument.envelopes[i];
                }
                //volumeEnv.toPlay = instrument.volumeEnvelope;
                //arpEnv.toPlay = instrument.arpEnvelope;
                //pitchEnv.toPlay = instrument.pitchEnvelope;
                //waveEnv.toPlay = instrument.waveEnvelope;
                //waveModEnv.toPlay = instrument.waveModEnvelope;
                if (macroID != id) {
                    macroID = id;
                    if (this.id < 0)
                        ResetModulations();
                    if (instrument is SampleInstrument)
                        _time = 0;
                }
                for (int i = 0; i < instrument.envelopes.Count; i++) {
                    envelopePlayers[i].Start();
                }
                if (currentInstrument is SampleInstrument) {
                    instrumentAsSample = currentInstrument as SampleInstrument;
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
                for (int i = 0; i < currentInstrument.envelopes.Count; i++) {
                    envelopePlayers[i].Release();
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
            if (currentInstrument.envelopes[0].IsActive && currentInstrument.envelopes[0].HasRelease)
                Release();
            else
                Cut();
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
            macroID = 0;
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
            arpEnvelopeResult = 0;
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
            syncAmt = 0;
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
        }

        public void SetWave(int waveIndex) {
            this.waveIndex = waveIndex;
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
            if (_state == VoiceState.Release && envelopePlayers[0].Evaluate() == 0)
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
            envelopePlayers[1].Start();
            arpEnvelopeResult = envelopePlayers[1].Evaluate();
            if (currentInstrument is SampleInstrument)
                _time = 0;
            _frequency = Helpers.NoteToFrequency(totalPitch);


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
                    return totalAmplitude * (_state == VoiceState.Off ? 0 : 1);
                else
                    return totalAmplitude * (_sampleVolume / 1.5f + 0.01f) * (_state == VoiceState.Off ? 0 : 1);
            }
        }

        public float CurrentAmplitudeAsWave {
            get {
                return totalAmplitude * (_state == VoiceState.Off ? 0 : 1);
            }
        }

        /// <summary>
        /// Pan value from 0.0-1.0, 0.5f is center
        /// </summary>
        public float CurrentPan { get { return panValue; } }

        /// <summary>
        /// This channel's current MIDI pitch
        /// </summary>
        public float CurrentPitch { get { return totalPitch; } }
        /// <summary>
        /// This channel's current frequency in hertz
        /// </summary>
        public float CurrentFrequency { get { return _frequency; } }

        public float EvaluateWave(float time) {
            // float t = time;
            float s = (syncAmt / 100f * 8) + 1;
            time = (s * (time % 1)) % 1;
            if (_fmSmooth > 0.001f)
                return currentWave.GetSampleMorphed(time + App.CurrentModule.WaveBank[(waveIndex + 1) % 100].GetSampleAtPosition(time) * (_fmSmooth * _fmSmooth) / 2f, App.CurrentModule.WaveBank[(waveIndex + 1) % 100], waveBlendAmt / 99f, waveStretchAmt / 100f);
            else
                return currentWave.GetSampleMorphed(time, App.CurrentModule.WaveBank[(waveIndex + 1) % 100], waveBlendAmt / 99f, waveStretchAmt / 100f);
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
            if (envelopePlayers[2].envelopeToPlay.IsActive && !envelopePlayers[2].EnvelopeEnded)
                pitchFallOffset += envelopePlayers[2].Evaluate() * deltaTime * 4;
            if (lastPitch != totalPitch) {
                lastPitch = totalPitch;
                _frequency = Helpers.NoteToFrequency(lastPitch);
            }

        }

        public void NextTick() {

            tickNum++;
            foreach (EnvelopePlayer envelopePlayer in envelopePlayers) {
                if (envelopePlayer.envelopeToPlay.IsActive) {
                    envelopePlayer.Step();
                    if (!envelopePlayer.EnvelopeEnded && _state != VoiceState.Off) {
                        if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.Wave) {
                            SetWave(envelopePlayer.Evaluate());
                        }
                        else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveBlend) {
                            waveBlendAmt = envelopePlayer.Evaluate();
                        }
                        else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveStretch) {
                            waveStretchAmt = envelopePlayer.Evaluate();
                        }
                        else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveSync) {
                            syncAmt = envelopePlayer.Evaluate();
                        }
                        else if (envelopePlayer.envelopeToPlay.Type == Envelope.EnvelopeType.WaveFM) {
                            fmAmt = envelopePlayer.Evaluate() / 20f;
                        }
                    }
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


            if (envelopePlayers[0].envelopeToPlay.Length > 0 && envelopePlayers[0].envelopeToPlay.IsActive)
                try {
                    if (envelopePlayers[0].EnvelopeEnded && envelopePlayers[0].envelopeToPlay.values[envelopePlayers[0].envelopeToPlay.Length - 1] == 0)
                        Cut();
                } catch {
                    // Cut();
                }
            arpEnvelopeResult = envelopePlayers[1].Evaluate();
            bassBoost = 0.6f * MathF.Pow(1.025f, -totalPitch) + 0.975f;
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
                _volumeSmooth += (totalAmplitude - _volumeSmooth) * 0.02f;
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
