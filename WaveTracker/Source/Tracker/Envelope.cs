using ProtoBuf;
using System;
using System.Collections.Generic;

namespace WaveTracker.Tracker {
    [ProtoContract(SkipConstructor = true)]
    public class Envelope {
        public const int MAX_ENVELOPE_LENGTH = 255;
        public enum EnvelopeType { Volume, Arpeggio, Pitch, Wave, WaveBlend, WaveStretch, WaveFM, WaveSync };
        [ProtoMember(1)]
        public EnvelopeType Type { get; private set; }
        [ProtoMember(2)]
        public bool IsActive { get; set; }
        [ProtoMember(3)]
        public sbyte[] values;
        [ProtoMember(4)]
        public byte ReleaseIndex { get; set; }
        [ProtoMember(5)]
        public byte LoopIndex { get; set; }

        public int Length { get { return values.Length; } }

        public const int EMPTY_LOOP_RELEASE_INDEX = byte.MaxValue;

        public Envelope(EnvelopeType type) {
            Type = type;
            IsActive = true;
            values = [];
            ReleaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            LoopIndex = EMPTY_LOOP_RELEASE_INDEX;
        }

        public Envelope Clone() {
            Envelope ret = new Envelope(Type);
            ret.IsActive = IsActive;
            ret.values = new sbyte[values.Length];
            for (int i = 0; i < values.Length; i++) {
                ret.values[i] = values[i];
            }
            ret.LoopIndex = LoopIndex;
            ret.ReleaseIndex = ReleaseIndex;
            return ret;
        }

        public string GetName() {
            return Type switch {
                EnvelopeType.Volume => "Volume",
                EnvelopeType.Arpeggio => "Arpeggio",
                EnvelopeType.Pitch => "Pitch",
                EnvelopeType.Wave => "Wave",
                EnvelopeType.WaveBlend => "Wave Blend",
                EnvelopeType.WaveStretch => "Wave Stretch",
                EnvelopeType.WaveSync => "Wave Sync",
                EnvelopeType.WaveFM => "Wave FM",
                _ => "--",
            };
        }

        public static string GetName(EnvelopeType type) {
            return type switch {
                EnvelopeType.Volume => "Volume",
                EnvelopeType.Arpeggio => "Arpeggio",
                EnvelopeType.Pitch => "Pitch",
                EnvelopeType.Wave => "Wave",
                EnvelopeType.WaveBlend => "Wave Blend",
                EnvelopeType.WaveStretch => "Wave Stretch",
                EnvelopeType.WaveSync => "Wave Sync",
                EnvelopeType.WaveFM => "Wave FM",
                _ => "--",
            };
        }

        public void Resize(int length) {
            sbyte[] newArr = new sbyte[length];
            for (int i = 0; i < newArr.Length; i++) {
                newArr[i] = i < values.Length ? values[i] : (sbyte)0;
            }
            values = newArr;
            if (LoopIndex > values.Length - 1) {
                LoopIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
            if (ReleaseIndex > values.Length - 2) {
                ReleaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
        }

        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {
            if (values != null) {
                for (int i = 0; i < values.Length; i++) {
                    switch (Type) {
                        case EnvelopeType.Volume:
                        case EnvelopeType.Wave:
                        case EnvelopeType.WaveBlend:
                        case EnvelopeType.WaveStretch:
                        case EnvelopeType.WaveSync:
                        case EnvelopeType.WaveFM:
                            values[i] = Math.Clamp(values[i], (sbyte)0, (sbyte)99);
                            break;
                        case EnvelopeType.Arpeggio:
                            values[i] = Math.Clamp(values[i], (sbyte)-118, (sbyte)120);
                            break;
                        case EnvelopeType.Pitch:
                            values[i] = Math.Clamp(values[i], (sbyte)-100, (sbyte)99);
                            break;

                    }
                }
            }
            else {
                values = [];
            }
        }

        public bool HasRelease { get { return ReleaseIndex != EMPTY_LOOP_RELEASE_INDEX; } }
        public bool HasLoop { get { return LoopIndex != EMPTY_LOOP_RELEASE_INDEX; } }

        /// <summary>
        /// Converts the contents of this envelope to a string with each value separated by a space
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            string s = "";
            if (values.Length > 0) {
                for (int i = 0; i < values.Length; i++) {
                    if (LoopIndex == i) {
                        s += "| ";
                    }
                    if (ReleaseIndex + 1 == i && ReleaseIndex != EMPTY_LOOP_RELEASE_INDEX) {
                        s += "/ ";
                    }
                    s += values[i] + (i < values.Length - 1 ? " " : "");
                }
            }
            return s;
        }

        /// <summary>
        /// Loads this envelope from a string with each value separated by a space
        /// </summary>
        /// <param name="input"></param>
        public void LoadFromString(string input) {
            string[] parts = input.Split(' ');
            int i = 0;
            ReleaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            LoopIndex = EMPTY_LOOP_RELEASE_INDEX;
            List<sbyte> vals = [];
            foreach (string part in parts) {
                if (part == "/") {
                    ReleaseIndex = (byte)--i;
                }

                if (part == "|") {
                    LoopIndex = (byte)i--;
                }

                if (sbyte.TryParse(part, out sbyte val)) {
                    switch (Type) {
                        case EnvelopeType.Volume:
                        case EnvelopeType.Wave:
                        case EnvelopeType.WaveBlend:
                        case EnvelopeType.WaveStretch:
                        case EnvelopeType.WaveSync:
                        case EnvelopeType.WaveFM:
                            val = Math.Clamp(val, (sbyte)0, (sbyte)99);
                            break;
                        case EnvelopeType.Arpeggio:
                            val = Math.Clamp(val, (sbyte)-118, (sbyte)120);
                            break;
                        case EnvelopeType.Pitch:
                            val = Math.Clamp(val, (sbyte)-100, (sbyte)99);
                            break;

                    }
                    vals.Add(val);
                }
                i++;
                if (i >= MAX_ENVELOPE_LENGTH) {
                    return;
                }
            }
            values = vals.ToArray();
            if (LoopIndex > values.Length - 1) {
                LoopIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
            if (ReleaseIndex > values.Length - 2) {
                ReleaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
        }
    }
}
