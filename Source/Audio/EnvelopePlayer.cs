using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.Audio {
    public class EnvelopePlayer {
        public int step;
        public bool released;
        public int Value { get; private set; }
        public Envelope EnvelopeToPlay { get; private set; }
        public bool EnvelopeEnded { get; private set; }
        public Envelope.EnvelopeType Type { get; private set; }

        public EnvelopePlayer(Envelope.EnvelopeType type) {
            step = 0;
            Type = type;
            released = false;
            EnvelopeToPlay = new Envelope(0);
            EnvelopeEnded = true;
        }

        public void SetEnvelope(Envelope envelope) {
            if (envelope == null) {
                EnvelopeToPlay = null;
                Evaluate();
                return;
            }
            if (envelope.Type != Type) {
                throw new ArgumentException("Envelope type and player type do not match!");
            }
            else {
                EnvelopeToPlay = envelope;
                Evaluate();
            }
        }

        public bool HasActiveEnvelopeData {
            get {
                if (EnvelopeToPlay == null)
                    return false;
                else
                    return EnvelopeToPlay.Length > 0 && EnvelopeToPlay.IsActive;
            }
        }

        public void Start() {
            step = 0;
            EnvelopeEnded = false;
            released = false;
            Evaluate();
        }

        public void Release() {
            released = true;
            if (EnvelopeToPlay.HasRelease) {
                step = EnvelopeToPlay.ReleaseIndex + 1;
                Evaluate();
            }
        }

        //public int Evaluate() {
        //    if (!envelopeToPlay.IsActive)
        //        return GetDefaultValue();
        //    if (envelopeToPlay.Length > 0) {
        //        if (step > envelopeToPlay.Length)
        //            step = envelopeToPlay.Length - 1;
        //        if (step < 0)
        //            step = 0;
        //    }
        //    if (envelopeToPlay.Length <= 0 || step < 0)
        //        return GetDefaultValue();
        //    try {
        //        return envelopeToPlay.values[step];
        //    } catch {
        //        return GetDefaultValue();
        //    }
        //}

        int GetDefaultValue() {
            return Type switch {
                Envelope.EnvelopeType.Volume => 99,
                _ => 0,
            };
        }

        public void Step() {
            if (EnvelopeToPlay.IsActive && EnvelopeToPlay.Length > 0) {
                step++;
                if (EnvelopeToPlay.HasRelease) {
                    if (step > EnvelopeToPlay.ReleaseIndex && !released) {
                        if (EnvelopeToPlay.ReleaseIndex <= EnvelopeToPlay.LoopIndex || !EnvelopeToPlay.HasLoop)
                            step = EnvelopeToPlay.ReleaseIndex;

                    }
                    if (EnvelopeToPlay.HasLoop) {
                        if (EnvelopeToPlay.ReleaseIndex >= EnvelopeToPlay.LoopIndex) {
                            if (step > EnvelopeToPlay.ReleaseIndex && !released)
                                step = EnvelopeToPlay.LoopIndex;
                        }
                        else {
                            if (step >= EnvelopeToPlay.Length) {
                                step = EnvelopeToPlay.LoopIndex;
                            }
                        }
                    }

                }
                else // no release
                  {
                    if (EnvelopeToPlay.HasLoop) {
                        if (step >= EnvelopeToPlay.Length) {
                            step = EnvelopeToPlay.LoopIndex;
                        }
                    }
                }
                if (step >= EnvelopeToPlay.Length) {
                    EnvelopeEnded = true;
                    step = EnvelopeToPlay.Length - 1;
                }
                else {
                    EnvelopeEnded = false;
                }
            }
            Evaluate();
        }


        void Evaluate() {
            if (EnvelopeToPlay == null) {
                Value = GetDefaultValue();
            }
            else if (!EnvelopeToPlay.IsActive) {
                Value = GetDefaultValue();
            }
            else if (step >= 0 && step < EnvelopeToPlay.Length) {
                Value = EnvelopeToPlay.values[step];
            }
            else {
                Value = GetDefaultValue();
            }
        }

        public string GetState() {
            return Value + " " + step + " ";
        }
    }
}
