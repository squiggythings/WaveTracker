using Microsoft.Xna.Framework;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeListItem : Clickable {
        /// <summary>
        /// The envelope this list item represents
        /// </summary>
        public Envelope Envelope { get; set; }

        /// <summary>
        /// If the item was clicked on, not including the bypass toggle or delete button
        /// </summary>
        public bool WasClickedOnToSelect { get; private set; }
        public bool WasClickedOnToDelete { get; private set; }

        private Clickable deleteButton;
        private SwitchToggle bypassToggle;

        public EnvelopeListItem(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            width = 92;
            height = 15;
            bypassToggle = new SwitchToggle(2, 3, this);
            bypassToggle.HasContrastOutline = true;
            bypassToggle.SetTooltip("Toggle envelope", "Enable or disable the envelope");
            deleteButton = new MouseRegion(77, 0, 15, 15, this);
            deleteButton.SetTooltip("Remove", "Remove the envelope from the instrument");
            SetParent(parent);
        }

        public void Update() {
            if (Envelope == null) {
                return;
            }

            WasClickedOnToSelect = false;
            WasClickedOnToDelete = false;
            if (ClickedDown && !bypassToggle.IsHovered) {
                if (!deleteButton.IsHovered) {
                    WasClickedOnToSelect = true;
                }
            }
            if (deleteButton.Clicked) {
                WasClickedOnToDelete = true;
            }
            bypassToggle.Value = Envelope.IsActive;
            bypassToggle.Update();
            if (bypassToggle.ValueWasChangedInternally) {
                Envelope.IsActive = bypassToggle.Value;
                Audio.ChannelManager.PreviewChannel.Reset();
            }
        }

        public void Draw(bool isSelected) {
            if (Envelope != null) {
                if (isSelected) {
                    DrawRoundedRect(0, 0, width, height, UIColors.selection);
                    bypassToggle.Draw();
                    Write(Envelope.GetName(), 18, 4, Helpers.Alpha(Color.White, Envelope.IsActive ? 255 : 64));
                    if (deleteButton.IsHovered) {
                        DrawRoundedRect(78, 1, 13, 13, new Color(122, 167, 255));
                    }
                    DrawSprite(82, 5, new Rectangle(400, 272, 5, 5), Color.White);
                }
                else {
                    DrawRoundedRect(0, 0, width, height, Color.White);
                    bypassToggle.Draw();
                    Write(Envelope.GetName(), 18, 4, Helpers.Alpha(UIColors.label, Envelope.IsActive ? 255 : 64));
                    if (deleteButton.IsHovered) {
                        DrawRoundedRect(78, 1, 13, 13, UIColors.panel);
                    }
                    if (IsHovered) {
                        DrawSprite(82, 5, new Rectangle(400, 272, 5, 5), UIColors.label);
                    }
                }
            }
        }
    }
}
