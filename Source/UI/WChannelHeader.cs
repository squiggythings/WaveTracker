using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Audio;
using WaveTracker.UI;

namespace WaveTracker.UI {
    public class WChannelHeader : Clickable {

        /// <summary>
        /// The number of pixels to decay the volume meter each frame
        /// </summary>
        const int METER_DECAY_RATE = 4;

        /// <summary>
        /// The width of the pan meter
        /// </summary>
        const int PAN_METER_WIDTH = 23;
        /// <summary>
        /// The width of the volume meter
        /// </summary>
        const int VOLUME_METER_WIDTH = 53;

        /// <summary>
        /// The number of effect columns on this channel
        /// </summary>
        public int NumEffectColumns { get; set; }

        /// <summary>
        /// Returns false if the mouse is past the patternEditors bounds
        /// </summary>
        bool MouseIsValid => MouseX + x < parentEditor.width;

        /// <summary>
        /// The pattern editor this channel header belongs to
        /// </summary>
        PatternEditor parentEditor;

        /// <summary>
        /// The bounds of where the user clicks to add an effect column
        /// </summary>
        MouseRegion expandEffectButton;

        /// <summary>
        /// The bounds of where the user clicks to remove an effect column
        /// </summary>
        MouseRegion collapseEffectButton;

        /// <summary>
        /// The index of the channel this channel header is rendering
        /// </summary>
        int channelNum;

        /// <summary>
        /// The amplitude meter value (in pixels)
        /// </summary>
        int amplitude;

        public WChannelHeader(int x, int y, int width, int height, int channelNum, PatternEditor parentEditor) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.channelNum = channelNum;
            this.parentEditor = parentEditor;
            expandEffectButton = new MouseRegion(56, 7, 6, 13, this);
            collapseEffectButton = new MouseRegion(50, 7, 6, 13, this);
            SetParent(parentEditor);
        }

        public void Update() {

            if (enabled) {
                if (MouseIsValid) {
                    if (!collapseEffectButton.IsHovered && !expandEffectButton.IsHovered) {
                        if (SingleClickedM(KeyModifier.None)) {
                            if (ChannelManager.IsChannelMuted(channelNum))
                                ChannelManager.UnmuteChannel(channelNum);
                            else
                                ChannelManager.MuteChannel(channelNum);
                        }
                        if (DoubleClickedM(KeyModifier.None)) {
                            if (ChannelManager.IsChannelSoloed(channelNum)) {

                            }
                            else {
                                ChannelManager.SoloChannel(channelNum);
                            }
                            ChannelManager.MuteAllChannels();
                            ChannelManager.UnmuteChannel(channelNum);
                        }
                    }

                    if (expandEffectButton.Clicked) NumEffectColumns++;
                    if (collapseEffectButton.Clicked) NumEffectColumns--;
                }
                expandEffectButton.enabled = NumEffectColumns < 4;
                collapseEffectButton.enabled = NumEffectColumns > 1;
            }
        }

        void UpdateAmplitude(Channel channel) {
            int currentAmp = Math.Clamp((int)(channel.CurrentAmplitude * VOLUME_METER_WIDTH + 0.5f), 0, VOLUME_METER_WIDTH);

            amplitude -= METER_DECAY_RATE;
            if (currentAmp >= amplitude)
                amplitude = currentAmp;

            if (amplitude < 0)
                amplitude = 0;
            channel._sampleVolume *= 0.6f;
        }

        public void Draw() {
            if (enabled) {
                int arrowOffset = 11;

                // draw background + text
                if (ChannelManager.IsChannelMuted(channelNum)) {
                    DrawRoundedRect(0, 0, width, height, new Color(208, 209, 221));
                    DrawRect(0, 20, width, height - 20, new Color(191, 193, 209));
                    if (channelNum >= 0)
                        Write("Channel " + (channelNum + 1), 4, 11, new Color(230, 69, 57));
                    arrowOffset = 12;
                }
                else if (IsPressed) {
                    DrawRoundedRect(0, 0, width, height, new Color(223, 224, 232));
                    DrawRect(0, 20, width, height - 20, new Color(208, 209, 221));
                    if (channelNum >= 0)
                        Write("Channel " + (channelNum + 1), 4, 10, new Color(104, 111, 153));
                }
                else {
                    DrawRoundedRect(0, 0, width, height, Color.White);
                    DrawRect(0, 20, width, height - 20, new Color(223, 224, 232));
                    if (channelNum >= 0)
                        Write("Channel " + (channelNum + 1), 4, 10, new Color(104, 111, 153));
                }


                // draw expansion arrows
                if (collapseEffectButton.enabled)
                    DrawExpansionArrow(52 - (collapseEffectButton.IsPressed ? 1 : 0), arrowOffset, true, collapseEffectButton.IsHovered && MouseIsValid);
                if (expandEffectButton.enabled)
                    DrawExpansionArrow(57 + (expandEffectButton.IsPressed ? 1 : 0), arrowOffset, false, expandEffectButton.IsHovered && MouseIsValid);

                // draw panning meter bg
                int panMeterStartPos = 5 + VOLUME_METER_WIDTH / 2 - PAN_METER_WIDTH / 2;
                DrawRect(panMeterStartPos, 22, PAN_METER_WIDTH, 1, new Color(104, 111, 153));
                DrawRect(panMeterStartPos, 23, PAN_METER_WIDTH, 1, new Color(163, 167, 194));

                // draw volume meter bg
                DrawRect(5, 25, VOLUME_METER_WIDTH, 1, new Color(104, 111, 153));
                DrawRect(5, 26, VOLUME_METER_WIDTH, 2, new Color(163, 167, 194));

                // if the user is editing this header's channel, render the preview channel instead
                Channel channelToDisplay;
                if (parentEditor.GetCursorPos.Channel == channelNum && !ChannelManager.channels[channelNum].isPlaying)
                    channelToDisplay = ChannelManager.previewChannel;
                else
                    channelToDisplay = ChannelManager.channels[channelNum];

                UpdateAmplitude(channelToDisplay);

                // write "fx" under each extra effect column
                for (int i = 2; i <= NumEffectColumns; ++i) {
                    Write("fx" + (i), 65 + (i - 2) * 18, 22, UIColors.labelLight);
                }

                // draw pan/volume meter
                if (ChannelManager.IsChannelMuted(channelNum)) {
                    DrawRect(panMeterStartPos + (int)(channelToDisplay.CurrentPan * (PAN_METER_WIDTH - 2)), 22, 3, 2, new Color(104, 111, 153));
                    DrawRect(5, 25, amplitude, 3, new Color(104, 111, 153));
                }
                else {
                    DrawRect(panMeterStartPos + (int)(channelToDisplay.CurrentPan * (PAN_METER_WIDTH - 2)), 22, 3, 2, Color.White);
                    DrawRect(5, 25, amplitude, 3, new Color(0, 219, 39));
                }
            }
        }


        void DrawExpansionArrow(int x, int y, bool facingLeft, bool hovered) {
            Color c = hovered ? UIColors.labelDark : Helpers.Alpha(UIColors.labelLight, 200);
            if (facingLeft) {
                DrawRect(x + 2, y, 1, 5, c);
                DrawRect(x + 1, y + 1, 1, 3, c);
                DrawRect(x, y + 2, 1, 1, c);
            }
            else {
                DrawRect(x, y, 1, 5, c);
                DrawRect(x + 1, y + 1, 1, 3, c);
                DrawRect(x + 2, y + 2, 1, 1, c);
            }
        }
    }
}
