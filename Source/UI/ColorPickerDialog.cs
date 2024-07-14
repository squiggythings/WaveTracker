using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;

namespace WaveTracker.UI {
    public class ColorPickerDialog : Dialog {
        public ColorButton parentButton;
        Button cancel, ok;
        Color color;
        HSLColor hslColor;


        MouseRegion colorSpectrumRegion;
        MouseRegion satSliderRegion;
        MouseRegion alphaSliderRegion;
        const int spectrumX = 9;
        const int spectrumY = 18;
        const int spectrumWidth = 128;
        const int spectrumHeight = 100;
        const int sliderHeight = 7;
        bool canStart = false;
        Textbox hexCode;
        NumberBox redNum, greenNum, blueNum, alphaNum;

        public ColorPickerDialog() : base("Pick Color...", 221, 160) {
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            colorSpectrumRegion = new MouseRegion(spectrumX, spectrumY, spectrumWidth, spectrumHeight, this);
            colorSpectrumRegion.SetTooltip("", "Set hue and lightness of color");
            satSliderRegion = new MouseRegion(spectrumX, spectrumY + spectrumHeight + 2, spectrumWidth, sliderHeight, this);
            satSliderRegion.SetTooltip("", "Set saturation of color");
            alphaSliderRegion = new MouseRegion(spectrumX, spectrumY + spectrumHeight + 2 + sliderHeight + 2, spectrumWidth, sliderHeight, this);
            alphaSliderRegion.SetTooltip("", "Set alpha transparency of color");

            hexCode = new Textbox("", 147, 60, 66, 66, this);
            hexCode.SetTooltip("", "Edit the color's hex code");
            hexCode.SetPrefix("#");
            redNum = new NumberBox("Red", 147, 81, 66, 36, this);
            redNum.SetTooltip("", "Set the red component of the color");
            redNum.SetValueLimits(0, 255);
            greenNum = new NumberBox("Green", 147, 95, 66, 36, this);
            greenNum.SetTooltip("", "Set the green component of the color");
            greenNum.SetValueLimits(0, 255);
            blueNum = new NumberBox("Blue", 147, 109, 66, 36, this);
            blueNum.SetTooltip("", "Set the blue component of the color");
            blueNum.SetValueLimits(0, 255);
            alphaNum = new NumberBox("Alpha", 147, 123, 66, 36, this);
            alphaNum.SetTooltip("", "Set the alpha component of the color");
            alphaNum.SetValueLimits(0, 255);

        }

        public void Open(ColorButton button) {
            base.Open(Input.focus);
            this.parentButton = button;
            color = parentButton.Color;
            SetAllValuesFromColor();
            canStart = false;
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();
                if (canStart) {
                    if (cancel.Clicked || ExitButton.Clicked)
                        Close();
                    if (ok.Clicked) {
                        parentButton.Color = color;
                        Close();
                    }


                    if (colorSpectrumRegion.DidClickInRegion) {
                        hslColor.H = colorSpectrumRegion.MouseXClamped * 360f;
                        hslColor.L = 1 - colorSpectrumRegion.MouseYClamped;
                        UpdateColorFromHSL();
                    }
                    if (satSliderRegion.DidClickInRegion) {
                        hslColor.S = satSliderRegion.MouseXClamped;
                        UpdateColorFromHSL();
                    }
                    if (alphaSliderRegion.DidClickInRegion) {
                        hslColor.A = satSliderRegion.MouseXClamped;
                        UpdateColorFromHSL();
                    }

                    redNum.Update();
                    if (redNum.ValueWasChangedInternally)
                        UpdateColorFromRGB();

                    greenNum.Update();
                    if (greenNum.ValueWasChangedInternally)
                        UpdateColorFromRGB();

                    blueNum.Update();
                    if (blueNum.ValueWasChangedInternally)
                        UpdateColorFromRGB();

                    alphaNum.Update();
                    if (alphaNum.ValueWasChangedInternally)
                        UpdateColorFromRGB();

                    hexCode.Update();
                    if (hexCode.ValueWasChangedInternally)
                        UpdateColorFromHex();
                }
                else if (!Input.GetClick(KeyModifier._Any))
                    canStart = true;
            }
        }

        void SetAllValuesFromColor() {
            redNum.Value = color.R;
            greenNum.Value = color.G;
            blueNum.Value = color.B;
            alphaNum.Value = color.A;

            hslColor = color.ToHSL();

            hexCode.Text = color.GetHexCode();
        }

        void UpdateColorFromHex() {
            color.SetFromHex(hexCode.Text);
            hexCode.Text = color.GetHexCode();
            hslColor = color.ToHSL();
            redNum.Value = color.R;
            greenNum.Value = color.G;
            blueNum.Value = color.B;
            alphaNum.Value = color.A;
        }

        void UpdateColorFromRGB() {
            color = new Color(redNum.Value, greenNum.Value, blueNum.Value, alphaNum.Value);
            hslColor = color.ToHSL();
            hexCode.Text = color.GetHexCode();
        }
        void UpdateColorFromHSL() {
            color = hslColor.ToRGB();
            hexCode.Text = color.GetHexCode();
            redNum.Value = color.R;
            greenNum.Value = color.G;
            blueNum.Value = color.B;
            alphaNum.Value = color.A;
        }

        void DrawSelectorPoint(int x, int y) {
            Color pointColor;
            if ((color.R * 30 + color.G * 59 + color.B * 11) / 100 < 128)
                pointColor = Color.White;
            else
                pointColor = Color.Black;

            //DrawRect(x + 1, y, 2, 1, ptCol);
            //DrawRect(x, y + 1, 1, 2, ptCol);
            //DrawRect(x - 2, y, 2, 1, ptCol);
            //DrawRect(x, y - 2, 1, 2, ptCol);
            DrawRect(x + 1, y, 1, 1, pointColor);
            DrawRect(x, y + 1, 1, 1, pointColor);
            DrawRect(x - 1, y, 1, 1, pointColor);
            DrawRect(x, y - 1, 1, 1, pointColor);
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();


                int specX = colorSpectrumRegion.x;
                int specY = colorSpectrumRegion.y;
                DrawRect(specX - 1, specY - 1, spectrumWidth + 1, spectrumHeight + 1, UIColors.labelLight);
                DrawRect(specX, specY, spectrumWidth + 1, spectrumHeight + 1, Color.White);
                // draw color spectrum
                for (int x = 0; x < spectrumWidth; x++) {
                    for (int y = 0; y < spectrumHeight; y++) {
                        Color col = Helpers.HSLtoRGB((int)(x * (360f / spectrumWidth)), hslColor.S, 1.0f - y / (float)spectrumHeight);
                        DrawRect(x + specX, y + specY, 1, 1, col);
                    }
                }
                // draw sat slider
                specY += spectrumHeight + 2;
                DrawRect(specX - 1, specY - 1, spectrumWidth + 1, sliderHeight + 1, UIColors.labelLight);
                DrawRect(specX, specY, spectrumWidth + 1, sliderHeight + 1, Color.White);
                for (int x = 0; x < spectrumWidth; x++) {
                    Color col = Helpers.HSLtoRGB((int)hslColor.H, x / (float)spectrumWidth, hslColor.L);
                    DrawRect(x + specX, specY, 1, sliderHeight, col);
                }

                // draw alpha slider
                specY += sliderHeight + 2;
                DrawRect(specX - 1, specY - 1, spectrumWidth + 1, sliderHeight + 1, UIColors.labelLight);
                DrawRect(specX, specY, spectrumWidth + 1, sliderHeight + 1, Color.White);

                bool check = true;
                for (int x = 0; x < spectrumWidth; x += 8) {
                    check = !check;
                    DrawRect(x + specX, specY, 8, sliderHeight / 2, check ? Color.Gray : Color.LightGray);
                    DrawRect(x + specX, specY + sliderHeight / 2, 8, (int)(sliderHeight / 2f + 0.5f), !check ? Color.Gray : Color.LightGray);
                }

                for (int x = 0; x < spectrumWidth; x++) {
                    float alpha = x / (float)spectrumWidth;
                    alpha *= 256;
                    Color col = Helpers.Alpha(hslColor.ToRGB(), (int)alpha);
                    DrawRect(x + specX, specY, 1, sliderHeight, col);
                }

                DrawSelectorPoint(spectrumX + (int)(hslColor.H / 360f * (spectrumWidth - 1)), spectrumY + (int)((1 - hslColor.L) * (spectrumHeight - 1)));

                DrawSelectorPoint(spectrumX + (int)(hslColor.S * (spectrumWidth - 1)), spectrumY + spectrumHeight + 2 + sliderHeight / 2);
                DrawSelectorPoint(spectrumX + (int)(hslColor.A * (spectrumWidth - 1)), spectrumY + spectrumHeight + 4 + sliderHeight + sliderHeight / 2);


                // draw color preview
                int prevX = 148;
                int prevY = 18;
                int prevW = 64;
                int prevH = 40;
                DrawRect(prevX - 1, prevY - 1, prevW + 1, prevH + 1, UIColors.labelLight);
                DrawRect(prevX, prevY, prevW + 1, prevH + 1, Color.White);
                for (int i = 0; i < 8; ++i) {
                    for (int j = 0; j < 5; ++j) {
                        if ((i + j) % 2 == 0)
                            DrawRect(prevX + i * 8, prevY + j * 8, 8, 8, Color.Gray);
                        else
                            DrawRect(prevX + i * 8, prevY + j * 8, 8, 8, Color.LightGray);
                    }
                }
                DrawRect(prevX, prevY, prevW, prevH, color);

                hexCode.Draw();
                redNum.Draw();
                greenNum.Draw();
                blueNum.Draw();
                alphaNum.Draw();
            }
        }
    }
}