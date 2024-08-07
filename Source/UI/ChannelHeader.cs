using Microsoft.Xna.Framework;
using System;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class ChannelHeader : Clickable {

        /// <summary>
        /// The number of pixels to decay the volume meter each frame
        /// </summary>
        private const int METER_DECAY_RATE = 4;

        /// <summary>
        /// The width of the pan meter
        /// </summary>
        private const int PAN_METER_WIDTH = 23;

        /// <summary>
        /// The width of the volume meter
        /// </summary>
        private const int VOLUME_METER_WIDTH = 53;

        /// <summary>
        /// The number of effect columns on this channel
        /// </summary>
        public int NumEffectColumns { get; set; }

        /// <summary>
        /// Returns false if the mouse is past the patternEditors bounds
        /// </summary>
        private bool MouseIsValid {
            get {
                return MouseX + x < parentEditor.width;
            }
        }

        /// <summary>
        /// The pattern editor this channel header belongs to
        /// </summary>
        private PatternEditor parentEditor;

        /// <summary>
        /// The bounds of where the user clicks to add an effect column
        /// </summary>
        private MouseRegion expandEffectButton;

        /// <summary>
        /// The bounds of where the user clicks to remove an effect column
        /// </summary>
        private MouseRegion collapseEffectButton;

        /// <summary>
        /// The index of the channel this channel header is rendering
        /// </summary>
        private int channelNum;

        /// <summary>
        /// The amplitude meter value (in pixels)
        /// </summary>
        private int amplitude;

        public float Amplitude { get { return Math.Clamp(amplitude / 50f, 0f, 1f); } }

        private Channel channelToDisplay;
        private Menu contextMenu;
        public ChannelHeader(int x, int y, int width, int channelNum, PatternEditor parentEditor) {
            this.x = x;
            this.y = y;
            this.width = width;
            height = 31;
            this.channelNum = channelNum;
            this.parentEditor = parentEditor;
            expandEffectButton = new MouseRegion(56, 7, 6, 13, this);
            collapseEffectButton = new MouseRegion(50, 7, 6, 13, this);
            contextMenu = new Menu([
                new MenuOption("Toggle channel",ToggleChannel),
                new MenuOption("Solo channel",SoloChannel),
                new MenuOption("Unmute all channels",ChannelManager.UnmuteAllChannels)
            ]);
            SetParent(parentEditor);
            channelToDisplay = ChannelManager.Channels[channelNum];
        }

        private void ToggleChannel() {
            ChannelManager.ToggleChannel(channelNum);
        }

        private void SoloChannel() {
            ChannelManager.SoloChannel(channelNum);
        }

        public void Update() {
            // if the user is editing this header's channel, render the preview channel instead
            channelToDisplay = parentEditor.cursorPosition.Channel == channelNum && !ChannelManager.Channels[channelNum].IsPlaying && !Playback.IsPlaying
                ? ChannelManager.PreviewChannel
                : ChannelManager.Channels[channelNum];
            UpdateAmplitude(channelToDisplay);
            if (enabled && !App.VisualizerMode) {
                if (Input.focusTimer > 1) {
                    if (MouseIsValid) {
                        if (RightClicked) {
                            ContextMenu.Open(contextMenu);
                        }
                        if (!collapseEffectButton.IsHovered && !expandEffectButton.IsHovered) {
                            if (SingleClickedM(KeyModifier.None)) {
                                ToggleChannel();
                            }
                            if (DoubleClickedM(KeyModifier.None) || ClickedM(KeyModifier.Ctrl)) {
                                if (ChannelManager.IsEveryChannelMuted() || ChannelManager.IsChannelSoloed(channelNum)) {
                                    ChannelManager.UnmuteAllChannels();
                                }
                                else {
                                    SoloChannel();
                                }
                            }
                        }

                        if (expandEffectButton.Clicked) {
                            NumEffectColumns++;
                        }

                        if (collapseEffectButton.Clicked) {
                            NumEffectColumns--;
                        }
                    }
                    expandEffectButton.enabled = NumEffectColumns < 4;
                    collapseEffectButton.enabled = NumEffectColumns > 1;
                }
            }
        }

        private void UpdateAmplitude(Channel channel) {
            int currentAmp = Math.Clamp((int)(channel.CurrentAmplitude * VOLUME_METER_WIDTH + 0.5f), 0, VOLUME_METER_WIDTH);

            amplitude -= METER_DECAY_RATE;
            if (currentAmp >= amplitude) {
                amplitude = currentAmp;
            }

            if (amplitude < 0) {
                amplitude = 0;
            }

            channel._sampleVolume *= 0.6f;
        }

        public void Draw() {
            if (enabled) {
                int arrowOffset = 11;

                // draw background + text
                if (ChannelManager.IsChannelMuted(channelNum)) {
                    DrawRoundedRect(0, 0, width, height, new Color(208, 209, 221));
                    DrawRect(0, 20, width, height - 20, new Color(191, 193, 209));
                    if (channelNum >= 0) {
                        Write("Channel " + (channelNum + 1), 4, 11, new Color(230, 69, 57));
                    }

                    arrowOffset = 12;
                }
                else if (MouseIsValid && IsPressed && !(collapseEffectButton.IsHovered || expandEffectButton.IsHovered)) {
                    DrawRoundedRect(0, 0, width, height, new Color(223, 224, 232));
                    DrawRect(0, 20, width, height - 20, new Color(208, 209, 221));
                    if (channelNum >= 0) {
                        Write("Channel " + (channelNum + 1), 4, 10, new Color(104, 111, 153));
                    }
                }
                else {
                    DrawRoundedRect(0, 0, width, height, Color.White);
                    DrawRect(0, 20, width, height - 20, new Color(223, 224, 232));
                    if (channelNum >= 0) {
                        Write("Channel " + (channelNum + 1), 4, 10, new Color(104, 111, 153));
                    }
                }

                // draw expansion arrows
                if (collapseEffectButton.enabled) {
                    DrawExpansionArrow(52 - (collapseEffectButton.IsPressed ? 1 : 0), arrowOffset, true, collapseEffectButton.IsHovered && MouseIsValid);
                }

                if (expandEffectButton.enabled) {
                    DrawExpansionArrow(57 + (expandEffectButton.IsPressed ? 1 : 0), arrowOffset, false, expandEffectButton.IsHovered && MouseIsValid);
                }

                // draw panning meter bg
                int panMeterStartPos = 5 + VOLUME_METER_WIDTH / 2 - PAN_METER_WIDTH / 2;
                DrawRect(panMeterStartPos, 22, PAN_METER_WIDTH, 1, new Color(104, 111, 153));
                DrawRect(panMeterStartPos, 23, PAN_METER_WIDTH, 1, new Color(163, 167, 194));

                // draw volume meter bg
                DrawRect(5, 25, VOLUME_METER_WIDTH, 1, new Color(104, 111, 153));
                DrawRect(5, 26, VOLUME_METER_WIDTH, 2, new Color(163, 167, 194));

                // write "fx" under each extra effect column
                for (int i = 2; i <= NumEffectColumns; ++i) {
                    Write("fx" + i, 65 + (i - 2) * 18, 22, UIColors.labelLight);
                }

                // draw pan/volume meter
                if (!AudioEngine.IsRendering) {
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
        }

        private void DrawExpansionArrow(int x, int y, bool facingLeft, bool hovered) {
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
