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
        public Envelope envelopeToPlay;
        public bool EnvelopeEnded { get; private set; }

        public EnvelopePlayer() {
            step = 0;
            released = false;
            envelopeToPlay = new Envelope(0);
            EnvelopeEnded = true;
        }

        public void Start() {
            step = 0;
            EnvelopeEnded = false;
            released = false;
        }

        public void Release() {
            released = true;
            if (envelopeToPlay.HasRelease)
                step = envelopeToPlay.ReleaseIndex + 1;
        }

        public int Evaluate() {
            if (!envelopeToPlay.IsActive)
                return GetDefaultValue();
            if (envelopeToPlay.Length > 0) {
                if (step > envelopeToPlay.Length) step = envelopeToPlay.Length - 1;
                if (step < 0)
                    step = 0;
            }
            if (envelopeToPlay.Length <= 0 || step < 0)
                return GetDefaultValue();
            try {
                return envelopeToPlay.values[step];
            } catch {
                return GetDefaultValue();
            }
        }

        int GetDefaultValue() {
            return envelopeToPlay.Type switch {
                Envelope.EnvelopeType.Volume => 99,
                _ => 0,
            };
        }

        public void Step() {
            if (envelopeToPlay.IsActive) {
                step++;
                if (envelopeToPlay.HasRelease) {
                    if (step > envelopeToPlay.ReleaseIndex && !released) {
                        if (envelopeToPlay.ReleaseIndex <= envelopeToPlay.LoopIndex || !envelopeToPlay.HasLoop)
                            step = envelopeToPlay.ReleaseIndex;

                    }
                    if (envelopeToPlay.HasLoop) {
                        if (envelopeToPlay.ReleaseIndex >= envelopeToPlay.LoopIndex) {
                            if (step > envelopeToPlay.ReleaseIndex && !released)
                                step = envelopeToPlay.LoopIndex;
                        }
                        else {
                            if (step >= envelopeToPlay.Length) {
                                step = envelopeToPlay.LoopIndex;
                            }
                        }
                    }

                }
                else // no release
                  {
                    if (envelopeToPlay.HasLoop) {
                        if (step >= envelopeToPlay.Length) {
                            step = envelopeToPlay.LoopIndex;
                        }
                    }
                }
                if (step >= envelopeToPlay.Length) {
                    EnvelopeEnded = true;
                    step = envelopeToPlay.Length - 1;
                }
                else {
                    EnvelopeEnded = false;
                }
            }
        }

        public string GetState() {
            return Evaluate() + " " + step + " ";
        }
    }
}
