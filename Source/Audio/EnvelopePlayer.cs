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
        public Envelope toPlay;
        public bool EnvelopeEnded { get; private set; }

        public EnvelopePlayer() {
            step = 0;
            released = false;
            toPlay = new Envelope(0);
            EnvelopeEnded = true;
        }

        public void Start() {
            step = 0;
            EnvelopeEnded = false;
            released = false;
        }

        public void Release() {
            released = true;
            if (toPlay.HasRelease)
                step = toPlay.releaseIndex + 1;
        }

        public int Evaluate() {
            if (!toPlay.isActive)
                return toPlay.defaultValue;
            if (toPlay.values.Count > 0) {
                if (step > toPlay.values.Count) step = toPlay.values.Count - 1;
                if (step < 0)
                    step = 0;
            }
            if (toPlay.values.Count <= 0 || step < 0)
                return toPlay.defaultValue;
            try {
                return toPlay.values[step];
            } catch {
                return toPlay.defaultValue;
            }
        }

        public void Step() {
            if (toPlay.isActive) {
                step++;
                if (toPlay.HasRelease) {
                    if (step > toPlay.releaseIndex && !released) {
                        if (toPlay.releaseIndex <= toPlay.loopIndex || !toPlay.HasLoop)
                            step = toPlay.releaseIndex;

                    }
                    if (toPlay.HasLoop) {
                        if (toPlay.releaseIndex >= toPlay.loopIndex) {
                            if (step > toPlay.releaseIndex && !released)
                                step = toPlay.loopIndex;
                        }
                        else {
                            if (step >= toPlay.values.Count) {
                                step = toPlay.loopIndex;
                            }
                        }
                    }

                }
                else // no release
                  {
                    if (toPlay.HasLoop) {
                        if (step >= toPlay.values.Count) {
                            step = toPlay.loopIndex;
                        }
                    }
                }
                if (step >= toPlay.values.Count) {
                    EnvelopeEnded = true;
                    step = toPlay.values.Count - 1;
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
