using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.Tracker;

namespace WaveTracker.Audio
{
    public class Channel
    {
        private ChannelManager _manager;
        int id;
        int macroStep;
        Wave currentWave;
        public List<TickEvent> tickEvents;
        float totalAmplitude => tremoloMultiplier * channelVolume / 99f * volumeEnv.Evaluate() / 99f; // * currentMacro.GetState().volumeMultiplier;
        float totalPitch => channelNotePorta + bendOffset + vibratoOffset + pitchFallOffset + arpeggioOffset + detuneOffset + arpEnv.Evaluate();
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
        float vibratoSpeed;
        float vibratoIntensity;
        float tremoloMultiplier; // 7xx command
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

        int channelVolume; // volume column
        int channelNote; // notes column
        float lastNote;
        public Macro currentMacro; // instrument column
        public EnvelopePlayer volumeEnv;
        public EnvelopePlayer arpEnv;
        public EnvelopePlayer pitchEnv;
        public EnvelopePlayer waveEnv;


        private float _frequency;
        private decimal _time;
        private float _leftAmp;
        private float _rightAmp;
        float _fadeMultiplier = 1;
        float _volumeSmooth;
        public int _tickTime;
        public int tickNum;
        int macroID;
        private enum VoiceState { On, Off, Release }
        private VoiceState _state;

        public Channel(int id, ChannelManager manager)
        {
            this.id = id;
            tickEvents = new List<TickEvent>();
            _manager = manager;
            currentMacro = new Macro(MacroType.Wave);
            macroID = 0;
            volumeEnv = new EnvelopePlayer();
            arpEnv = new EnvelopePlayer();
            pitchEnv = new EnvelopePlayer();
            waveEnv = new EnvelopePlayer();
            Reset();
        }

        public void QueueEvent(TickEventType eventType, int val1, int val2, int delay)
        {
            tickEvents.Add(new TickEvent(eventType, val1, val2, delay + 0));
        }

        public void EffectCommand(int command, int parameter)
        {
            if (command == 4) // 4XY
            {
                vibratoSpeed = (parameter + 1) / 16;
                vibratoIntensity = parameter % 16;
            }
            if (command == 7) // 7XY
            {
                tremoloSpeed = (parameter + 1) / 16;
                tremoloIntensity = parameter % 16;
            }
            if (command == 8) // 8XX
            {
                _Pan(parameter / 100f);
            }
            if (command == 1) // 1XX
            {
                pitchFallSpeed = parameter / 1f;
            }
            if (command == 2) // 2XX
            {
                pitchFallSpeed = -parameter / 1f;
            }
            if (command == 3) // 3XX
            {
                portaSpeed = parameter * 1.15f;
            }
            if (command == 10) // QXY
            {
                bendSpeed = (parameter / 16) * 9 + 1;
                targetBendAmt += parameter % 16;
            }
            if (command == 11) // RXY
            {
                bendSpeed = (parameter / 16) * 9 + 1;
                targetBendAmt -= parameter % 16;
            }
            if (command == 0) //0XY
            {
                arpCounter = 0;
                arpeggionote2 = parameter / 16;
                arpeggionote3 = parameter % 16;
            }
            if (command == 9) //0XY
            {
                stereoPhaseOffset = parameter / 200f;
            }
            if (command == 14) // PXX
            {
                detuneOffset = (parameter - 50) / 37.5f;
            }
            if (command == 16) // VXX
            {
                SetWave(parameter);
            }

        }

        public void SetVolume(int vol)
        {
            channelVolume = vol;
        }

        public void SetMacro(int id)
        {
            Macro m = Game1.currentSong.instruments[id];
            currentMacro = m;
            volumeEnv.toPlay = m.volumeEnvelope;
            arpEnv.toPlay = m.arpEnvelope;
            pitchEnv.toPlay = m.pitchEnvelope;
            waveEnv.toPlay = m.waveEnvelope;
            if (macroID != id)
            {
                macroID = id;
                if (m.macroType == MacroType.Sample)
                    _time = 0;
            }

        }

        public void Release()
        {
            if (_state == VoiceState.On)
            {
                volumeEnv.Release();
                pitchEnv.Release();
                arpEnv.Release();
                waveEnv.Release();
                _state = VoiceState.Release;
            }
        }

        public void PreviewCut()
        {
            if (volumeEnv.toPlay.isActive && volumeEnv.toPlay.HasRelease)
                Release();
            else
                Cut();
        }

        public void Cut()
        {
            _state = VoiceState.Off;
        }

        public void OnNoteEnd()
        {
            vibratoTime = 0;
            arpCounter = 0;
            noteOn = false;
        }

        public void Reset()
        {
            tickEvents.Clear();
            noteOn = false;
            _state = VoiceState.Off;
            channelVolume = 99;
            detuneOffset = 0;
            pitchFallSpeed = 0;
            pitchFallOffset = 0;
            vibratoOffset = 0;
            tremoloMultiplier = 1;
            _time = 0.0M;
            arpCounter = 0;
            arpeggionote2 = 0;
            arpeggionote3 = 0;
            vibratoIntensity = 0;
            vibratoOffset = 0;
            vibratoSpeed = 0;
            vibratoTime = 0;
            tremoloIntensity = 0;
            tremoloMultiplier = 0;
            tremoloSpeed = 0;
            tremoloTime = 0;
            stereoPhaseOffset = 0;
            currentWave = _manager.waveBank.GetWave(0);
            lastNote = channelNote;
            channelNotePorta = channelNote;
            portaSpeed = 0;
            portaTime = 0;

            bendSpeed = 0;
            targetBendAmt = 0;
            bendOffset = 0;
            _Pan(0.5f);
        }

        public void SetWave(int w)
        {

            currentWave = _manager.waveBank.GetWave(w);
        }

        public void ResetTick(int num)
        {
            _tickTime = num;
        }

        public void TriggerNote(int num)
        {

            pitchFallOffset = 0;
            if (!noteOn)
            {
                _fadeMultiplier = 1f;
            }
            if (_state == VoiceState.Off)
            {
                _time = 0.0M;
            }
            if (_state == VoiceState.Release && volumeEnv.Evaluate() == 0)
                _time = 0.0M;
            targetBendAmt = 0;
            bendOffset = 0;
            if (channelNote != num || _state != VoiceState.On)
            {
                lastNote = channelNotePorta;
                channelNote = num;
                if (portaSpeed == 0)
                    channelNotePorta = channelNote;
                portaTime = 0;
                noteOn = true;
                _state = VoiceState.On;
            }

            volumeEnv.Start();
            pitchEnv.Start();
            arpEnv.Start();
            waveEnv.Start();
            if (currentMacro.macroType == MacroType.Wave)
            {
                if (waveEnv.toPlay.isActive)
                    if (!waveEnv.envelopeEnded)
                        SetWave(waveEnv.Evaluate());
            }

            _tickTime = 0;
            if (currentMacro.macroType == MacroType.Sample)
                _time = 0;
            _frequency = Helpers.NoteToFrequency(num);

        }
        private void _Pan(float val)
        {

            if (val > 0.98f)
                val = 1.0f;
            panValue = val;
            _rightAmp = (float)Math.Sqrt(val);
            _leftAmp = (float)Math.Sqrt(1.0f - val);
        }
        public float CurrentAmplitude
        { get { return totalAmplitude * (noteOn ? 1 : 0); } }

        public float CurrentPan
        { get { return panValue; } }

        public float CurrentPitch
        { get { return totalPitch; } }
        public float CurrentFrequency
        { get { return _frequency; } }

        float EvaluateWave(float time)
        {
            return currentWave.getSampleAtPosition(time);
        }



        void ContinuousTick(float deltaTime)
        {
            tremoloTime += deltaTime * tremoloSpeed * 6f;
            tremoloMultiplier = 1f + (float)(Math.Sin(tremoloTime * 1) / 2 - 0.5f) * tremoloIntensity / 16f;

            vibratoTime += deltaTime * vibratoSpeed * 3f;
            vibratoOffset = (float)Math.Sin(vibratoTime * 2) * vibratoIntensity / 10f;

            pitchFallOffset += pitchFallSpeed * deltaTime * 2;
            if (portaTime < 2)
                portaTime += deltaTime * (portaSpeed == 0 ? AudioEngine.sampleRate : portaSpeed);

            if (channelNotePorta > channelNote)
            {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta < channelNote)
                    channelNotePorta = channelNote;
            }
            else if (channelNotePorta < channelNote)
            {
                channelNotePorta += (channelNote - lastNote) * deltaTime * portaSpeed;
                if (channelNotePorta > channelNote)
                    channelNotePorta = channelNote;
            }

            if (bendOffset > targetBendAmt)
            {
                bendOffset -= deltaTime * bendSpeed;
                if (bendOffset < targetBendAmt)
                    bendOffset = targetBendAmt;
            }
            else if (bendOffset < targetBendAmt)
            {
                bendOffset += deltaTime * bendSpeed;
                if (bendOffset > targetBendAmt)
                    bendOffset = targetBendAmt;
            }
            pitchFallOffset += pitchEnv.Evaluate();
            _frequency = Helpers.NoteToFrequency(totalPitch);

        }

        float Lerp(float firstFloat, float secondFloat, float by)
        {
            by = Math.Clamp(by, 0f, 1f);
            return firstFloat * (1 - by) + secondFloat * by;
        }

        void NextTick()
        {
            tickNum++;

            switch (arpCounter)
            {
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

            for (int i = 0; i < tickEvents.Count; i++)
            {
                tickEvents[i].Update();
                if (tickEvents[i].countdown <= 0)
                {
                    DoEvent(tickEvents[i]);
                    tickEvents.RemoveAt(i);
                    i--;
                }

            }
            arpCounter++;
            if (arpCounter > 2)
                arpCounter = 0;
            if (currentMacro.macroType == MacroType.Wave)
            {
                if (waveEnv.toPlay.isActive)
                    if (!waveEnv.envelopeEnded)
                        SetWave(waveEnv.Evaluate());
                waveEnv.Step();
            }
            volumeEnv.Step();
            arpEnv.Step();
            pitchEnv.Step();

            _frequency = Helpers.NoteToFrequency(totalPitch);
        }

        void DoEvent(TickEvent t)
        {
            if (t.eventType == TickEventType.Instrument)
            {
                SetMacro(t.value);
            }
            if (t.eventType == TickEventType.Note)
            {
                if (t.value == -2)
                    Cut();
                else if (t.value == -3)
                    Release();
                else if (t.value >= 0)
                    TriggerNote(t.value);
            }
            if (t.eventType == TickEventType.Volume)
            {
                SetVolume(t.value);
            }
            if (t.eventType == TickEventType.Effect)
            {
                EffectCommand(t.value, t.value2);
            }
        }


        public void Process(float[,] buffer)
        {
            int samplesPerBuffer = buffer.GetLength(1);

            for (int i = 0; i < samplesPerBuffer; i++)
            {
                float freqCut;
                if (_frequency > 6500)
                {
                    freqCut = 1 / (1 + (_frequency - 6500) / 1500);
                }
                else
                {
                    freqCut = 1;
                }
                if (_frequency > 30000)
                {
                    freqCut = 0;
                    _frequency = 30000;
                    _state = VoiceState.Off;
                }
                decimal delta = 1.0M / AudioEngine.sampleRate * (Decimal)_frequency;
                if (_tickTime > AudioEngine.sampleRate / AudioEngine.tickSpeed)
                {
                    _tickTime -= AudioEngine.sampleRate / AudioEngine.tickSpeed;
                    NextTick();
                    if (id == 0)
                    {
                        Playback.Step(true);
                    }
                }
                ContinuousTick((float)(1.0M / AudioEngine.sampleRate * AudioEngine.tickSpeed / 60));
                if (noteOn)
                {

                    if (_state == VoiceState.Off)
                    {
                        _fadeMultiplier /= 1.002f;
                        if (_fadeMultiplier < 0.001f)
                        {
                            noteOn = false;
                        }
                    }
                    else
                    {
                        _fadeMultiplier = 1;
                        _time += delta;
                    }


                    float sampleL = 0;
                    float sampleR = 0;
                    if (currentMacro != null)
                        if (currentMacro.macroType == MacroType.Wave)
                        {
                            sampleL = EvaluateWave((float)_time - stereoPhaseOffset);
                            sampleR = EvaluateWave((float)_time + stereoPhaseOffset);
                        }
                        else
                        {
                            currentMacro.sample.SampleTick(_time, 0);
                            sampleL = currentMacro.sample.SAMPLE_OUTPUT_L;
                            sampleR = currentMacro.sample.SAMPLE_OUTPUT_R;
                        }


                    // Important: Use += instead of = !
                    // float targetAmp = (float)Math.Pow(totalAmplitude, 1.25);
                    _volumeSmooth += (totalAmplitude - _volumeSmooth) * 0.02f;
                    if (FrameEditor.channelToggles[id])
                    {
                        float l = sampleL * _volumeSmooth * _leftAmp * _fadeMultiplier * freqCut;
                        float r = sampleR * _volumeSmooth * _rightAmp * _fadeMultiplier * freqCut;

                        if (AudioEngine.quantizeAmplitude)
                        {
                            if (_volumeSmooth < 0.005f)
                            {
                                l = 0;
                                r = 0;
                            }
                            int quantamt = 100;
                            l = (float)(Math.Ceiling(l * quantamt)) / (float)quantamt;
                            r = (float)(Math.Ceiling(r * quantamt)) / (float)quantamt;
                        }
                        buffer[0, i] += l * 0.21f;
                        buffer[1, i] += r * 0.21f;
                    }
                }
                _tickTime++;
            }

        }

    }

    public class EnvelopePlayer
    {
        public int step;
        public bool released;
        public Envelope toPlay;
        public bool envelopeEnded { get; private set; }

        public EnvelopePlayer()
        {
            step = 0;
            released = false;
            toPlay = new Envelope(0);
        }

        public void Start()
        {
            step = 0;
            envelopeEnded = false;
            released = false;
        }

        public void Release()
        {
            released = true;
            if (toPlay.HasRelease)
                step = toPlay.releaseIndex;
        }

        public int Evaluate()
        {
            if (!toPlay.isActive)
                return toPlay.defaultValue;
            if (toPlay.values.Count == 0 || step < 0)
                return toPlay.defaultValue;
            return toPlay.values[step];
        }

        public void Step()
        {
            step++;
            if (toPlay.HasRelease)
            {
                if (step > toPlay.releaseIndex && !released)
                    step = toPlay.releaseIndex;
                if (toPlay.HasLoop)
                {
                    if (toPlay.releaseIndex > toPlay.loopIndex)
                    {
                        if (step > toPlay.releaseIndex)
                            step = toPlay.loopIndex;
                    }
                    else
                    {
                        if (step >= toPlay.values.Count)
                        {
                            step = toPlay.loopIndex;
                        }
                    }
                }

            }
            else
            {
                if (toPlay.HasLoop)
                {
                    if (step >= toPlay.values.Count)
                    {
                        step = toPlay.loopIndex;
                    }
                }
                else
                {

                }
            }
            if (step >= toPlay.values.Count)
            {
                envelopeEnded = true;
                step = toPlay.values.Count - 1;
            }
            else
            {
                envelopeEnded = false;
            }
        }

        public string GetState()
        {
            return Evaluate() + " " + step + " ";
        }
    }
}
