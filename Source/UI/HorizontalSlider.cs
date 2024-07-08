using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class HorizontalSlider : Clickable {

        public int Value { get; set; }
        public int MinimumValue { get; private set; }
        public int MaximumValue { get; private set; }
        public float ValueAsPercentage { get { return Helpers.Map(Value, MinimumValue, MaximumValue, 0f, 1f); } }

        public int FineAdjustAmount { get; set; }
        public int CoarseAdjustAmount { get; set; }

        public int QuantizeValue { get; set; }

        public float TickSpacing { get; set; }

        Clickable handle;
        bool isDraggingHandle;

        public HorizontalSlider(int x, int y, int width, int numTicks, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = 11;
            handle = new MouseRegion(0, 0, 5, height, this);
            QuantizeValue = 1;
            FineAdjustAmount = 1;
            CoarseAdjustAmount = 1;
            TickSpacing = width / (float)numTicks;
            this.parent = parent;
        }

        public void Update() {
            int mouseValue = (int)Math.Round(Helpers.MapClamped(MouseX, 0, width, MinimumValue, MaximumValue));
            handle.x = (int)(ValueAsPercentage * width) - handle.width / 2;
            handle.y = (height - handle.height) / 2;
            if (handle.ClickedDown) {
                isDraggingHandle = true;
            }
            if (isDraggingHandle) {
                if (Input.GetClickUp(KeyModifier._Any)) {
                    isDraggingHandle = false;
                }
                Value = mouseValue;
                handle.x = (int)(ValueAsPercentage * width) - handle.width / 2;
            }
            else {
                if (ClickedDown) {
                    if (MouseX < handle.x + handle.width / 2) {
                        Value -= CoarseAdjustAmount;
                        if (Value < mouseValue)
                            Value = mouseValue;
                    }
                    else {
                        Value += CoarseAdjustAmount;
                        if (Value > mouseValue)
                            Value = mouseValue;
                    }
                }
                if (IsHovered) {
                    Value -= Input.MouseScrollWheel(KeyModifier.None) * FineAdjustAmount;
                }
            }

            Value = Math.Clamp(Value, MinimumValue, MaximumValue);
        }

        public void SetValueLimits(int min, int max) {
            MinimumValue = min;
            MaximumValue = max;
            Value = Math.Clamp(Value, min, max);
        }

        void DrawTick(int x, int tickHeight) {
            DrawRect(x, 0, 1, tickHeight, UIColors.labelLight);
            DrawRect(x, height - tickHeight, 1, tickHeight, UIColors.labelLight);
        }


        public void Draw() {
            DrawRect(-2, height / 2 - 1, width + 1 + 4, 3, UIColors.labelLight);
            int tickHeight = (height - 7) / 2;
            DrawTick(0, tickHeight);
            for (float i = TickSpacing; i <= width; i += TickSpacing) {
                DrawTick((int)i, tickHeight);
            }
            DrawTick(width, tickHeight);
            if (isDraggingHandle) {
                DrawRect(handle.x, handle.y, handle.width, handle.height, ButtonColors.Default.backgroundColor);
            }
            else if (handle.IsHovered) {
                DrawRect(handle.x, handle.y, handle.width, handle.height, UIColors.selection.Lerp(Color.White, 0.2f));
            }
            else {
                DrawRect(handle.x, handle.y, handle.width, handle.height, UIColors.selection);
            }
            //Write(Value + "", handle.x, handle.y - 10, Color.Red);
        }
    }
}
