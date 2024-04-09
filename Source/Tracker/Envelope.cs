using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;


namespace WaveTracker.Tracker {
    [ProtoContract]
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
            //this.defaultValue = defaultValue;
            IsActive = true;
            values = new sbyte[0];
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
            switch (Type) {
                case EnvelopeType.Volume:
                    return "Volume";
                case EnvelopeType.Arpeggio:
                    return "Arpeggio";
                case EnvelopeType.Pitch:
                    return "Pitch";
                case EnvelopeType.Wave:
                    return "Wave";
                case EnvelopeType.WaveBlend:
                    return "Wave Blend";
                case EnvelopeType.WaveStretch:
                    return "Wave Stretch";
                case EnvelopeType.WaveSync:
                    return "Wave Sync";
                case EnvelopeType.WaveFM:
                    return "Wave FM";
            }
            return "--";
        }

        public static string GetName(EnvelopeType type) {
            switch (type) {
                case EnvelopeType.Volume:
                    return "Volume";
                case EnvelopeType.Arpeggio:
                    return "Arpeggio";
                case EnvelopeType.Pitch:
                    return "Pitch";
                case EnvelopeType.Wave:
                    return "Wave";
                case EnvelopeType.WaveBlend:
                    return "Wave Blend";
                case EnvelopeType.WaveStretch:
                    return "Wave Stretch";
                case EnvelopeType.WaveSync:
                    return "Wave Sync";
                case EnvelopeType.WaveFM:
                    return "Wave FM";
            }
            return "--";
        }

        public void Resize(int length) {
            sbyte[] newArr = new sbyte[length];
            for (int i = 0; i < newArr.Length; i++) {
                if (i < values.Length) {
                    newArr[i] = values[i];
                }
                else {
                    newArr[i] = 0;
                }
            }
            values = newArr;
            if (LoopIndex > values.Length - 1) {
                LoopIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
            if (ReleaseIndex > values.Length - 2) {
                ReleaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            }
        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialization() {

        }
        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {

        }

        //public string ToEncodedString() {
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append((char)Type);
        //    sb.Append((char)ReleaseIndex);
        //    sb.Append((char)LoopIndex);
        //    sb.Append(IsActive ? '1' : '0');
        //    foreach (short value in values) {
        //        sb.Append((char)value);
        //    }
        //    return sb.ToString();
        //}

        //public void FromEncodedString(string encodedEnvelope) {
        //    int i = 0;
        //    Type = (EnvelopeType)encodedEnvelope[i++];
        //    ReleaseIndex = (byte)encodedEnvelope[i++];
        //    LoopIndex = (byte)encodedEnvelope[i++];
        //    IsActive = encodedEnvelope[i++] == '1';
        //    List<short> newValues = new List<short>();
        //    while (i < encodedEnvelope.Length) {
        //        newValues.Add((short)encodedEnvelope[i++]);
        //    }
        //    values = newValues.ToArray();
        //}

        /*public bool IsEqualTo(Envelope other) {
            if (other.isActive != isActive)
                return false;

            if (other.loopIndex != loopIndex)
                return false;
            if (other.releaseIndex != releaseIndex)
                return false;
            if (values.Length != other.values.Length)
                return false;
            for (int i = 0; i < values.Length; ++i) {
                if (values[i] != other.values[i]) {
                    return false;
                }
            }
            return true;
        }*/

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
            List<sbyte> vals = new List<sbyte>();
            foreach (string part in parts) {
                if (part == "/")
                    ReleaseIndex = (byte)--i;
                if (part == "|")
                    LoopIndex = (byte)i--;
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
