using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using WaveTracker.UI;

namespace WaveTracker {
    public class Input {

        /// <summary>
        /// The amount of time since the last frame in milliseconds
        /// </summary>
        public static int deltaTime;
        private static KeyboardState currentKeyState;
        private static KeyboardState previousKeyState;
        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        /// <summary>
        /// In milliseconds
        /// </summary>
        public static float TimeSinceLastClick { get; private set; }
        /// <summary>
        /// In milliseconds
        /// </summary>
        public static float TimeSinceLastClickUp { get; private set; }

        /// <summary>
        /// Time the mouse button is held down in milliseconds
        /// </summary>
        public static float ClickTime { get; private set; }

        private static float lastTimeSinceLastClick;
        private static float lastTimeSinceLastClickUp;
        private static float timeSinceDoubleClick;
        private const int DOUBLE_CLICK_TIME = 350;
        private const int KEY_REPEAT_DELAY = 500;
        private const int KEY_REPEAT_TIME = 33;
        private const float MOUSE_DRAG_DISTANCE = 5;
        public static int dialogOpenCooldown;
        private static Dictionary<Keys, int> keyTimePairs;

        public static KeyboardState Keyboard { get { return currentKeyState; } }
        public static MouseState Mouse { get { return currentMouseState; } }

        public static Point LastClickLocation { get; private set; }
        public static Point LastRightClickLocation { get; private set; }
        public static Point LastClickReleaseLocation { get; private set; }
        public static Point LastRightClickReleaseLocation { get; private set; }

        private static bool cancelClick;

        public static KeyModifier CurrentModifier { get; set; }

        private static bool doubleClick;
        public static bool internalDialogIsOpen;
        public static Element focus = null;
        public static Element lastFocus = null;
        /// <summary>
        /// What element was in focus during the last click
        /// </summary>
        public static Element lastClickFocus;
        /// <summary>
        /// How many frames since we last switched focus
        /// </summary>
        public static int focusTimer;
        /// <summary>
        /// How many frames since the window has been out of focus
        /// </summary>
        public static int windowFocusTimer;

        public static Keys[] currentPressedKeys;
        public static Keys CurrentPressedKey { get; private set; }
        public static KeyboardShortcut CurrentPressedShortcut { get; private set; }
        public static void Intialize() {
            keyTimePairs = [];
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                keyTimePairs.Add(k, 0);
            }

            TimeSinceLastClick = 1000;
        }

        public static void DialogStarted() {
            dialogOpenCooldown = 3;
        }

        public static void GetState(GameTime gameTime) {
            deltaTime = gameTime.ElapsedGameTime.Milliseconds;
            cancelClick = false;
            if (windowFocusTimer > 0) {
                windowFocusTimer--;
            }

            if (focus != lastFocus) {
                lastFocus = focus;
                focusTimer = 0;
            }
            else {
                focusTimer++;
            }

            if (dialogOpenCooldown > 0) {
                dialogOpenCooldown--;
                if (dialogOpenCooldown > 0) {
                    cancelClick = true;
                    return;
                }
            }

            lastTimeSinceLastClick = TimeSinceLastClick;
            lastTimeSinceLastClickUp = TimeSinceLastClickUp;
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            currentPressedKeys = currentKeyState.GetPressedKeys();
            CurrentModifier = GetCurrentModifier();
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                if (currentKeyState.IsKeyDown(k)) {
                    if (keyTimePairs[k] > KEY_REPEAT_DELAY) {
                        keyTimePairs[k] -= KEY_REPEAT_TIME;
                    }

                    keyTimePairs[k] += gameTime.ElapsedGameTime.Milliseconds;
                }
                else {
                    keyTimePairs[k] = 0;
                }
            }

            CurrentPressedKey = Keys.None;
            foreach (Keys k in currentPressedKeys) {
                if (k is not Keys.LeftShift and
                    not Keys.RightShift and
                    not Keys.LeftControl and
                    not Keys.RightControl and
                    not Keys.LeftAlt and
                    not Keys.RightAlt) {
                    CurrentPressedKey = k;
                }
            }

            CurrentPressedShortcut = new KeyboardShortcut(CurrentPressedKey, CurrentModifier);

            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            TimeSinceLastClick += gameTime.ElapsedGameTime.Milliseconds;
            TimeSinceLastClickUp += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceDoubleClick += gameTime.ElapsedGameTime.Milliseconds;
            if (GetClick(KeyModifier._Any)) {
                ClickTime += gameTime.ElapsedGameTime.Milliseconds;
            }
            else {
                ClickTime = 0;
            }

            if (GetRightClickDown(KeyModifier._Any)) {
                LastRightClickLocation = new Point(MousePositionX, MousePositionY);
            }

            if (GetClickDown(KeyModifier._Any)) {
                LastClickLocation = new Point(MousePositionX, MousePositionY);
                lastClickFocus = focus;
                if (!doubleClick && TimeSinceLastClick < DOUBLE_CLICK_TIME && Vector2.Distance(LastClickReleaseLocation.ToVector2(), MousePos) < MOUSE_DRAG_DISTANCE) {
                    //this is a double click
                    doubleClick = true;
                }
                else {
                    //this is a normal click
                    doubleClick = false;
                }

                TimeSinceLastClick = 0;
            }

            if (GetClick(KeyModifier._Any)) {
                if (Vector2.Distance(LastClickLocation.ToVector2(), MousePos) > MOUSE_DRAG_DISTANCE) {
                    MouseIsDragging = true;
                }
            }
            else {
                MouseJustEndedDragging = MouseIsDragging;
                MouseIsDragging = false;
            }

            if (GetClickUp(KeyModifier._Any)) {
                TimeSinceLastClickUp = 0;
                LastClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }

            if (GetRightClickUp(KeyModifier._Any)) {
                LastRightClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }
        }

        private static Vector2 MousePos { get { return new Vector2(MousePositionX, MousePositionY); } }

        public static void CancelClick() {
            cancelClick = true;
        }

        public static bool GetKey(Keys key, KeyModifier modifier) {
            return ModifierMatches(modifier) && currentKeyState.IsKeyDown(key);
        }

        public static bool GetKeyDown(Keys key, KeyModifier modifier) {

            return ModifierMatches(modifier) && currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public static bool GetKeyRepeat(Keys key, KeyModifier modifier) {

            return ModifierMatches(modifier) && (currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key) || keyTimePairs[key] > KEY_REPEAT_DELAY);
        }

        public static bool GetKeyUp(Keys key, KeyModifier modifier) {

            return ModifierMatches(modifier) && !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key);
        }

        public static bool GetClickUp(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool GetSingleClickUp(KeyModifier modifier) {

            return GetClickUp(modifier) && !doubleClick;
        }
        public static bool GetClickDown(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        }
        public static bool GetClick(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool GetRightClick(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.RightButton == ButtonState.Pressed;
        }

        public static bool GetRightClickUp(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
        }
        public static bool GetRightClickDown(KeyModifier modifier) {

            return ModifierMatches(modifier) && !cancelClick && currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        }

        public static bool GetDoubleClick(KeyModifier modifier) {
            return GetClickDown(modifier) && doubleClick;
        }

        public static int MousePositionX { get { return currentMouseState.X / App.Settings.General.ScreenScale; } }
        public static int MousePositionY { get { return currentMouseState.Y / App.Settings.General.ScreenScale; } }

        public static int MouseDeltaX { get { return currentMouseState.X / App.Settings.General.ScreenScale - previousMouseState.X / App.Settings.General.ScreenScale; } }

        public static int MouseDeltaY { get { return currentMouseState.Y / App.Settings.General.ScreenScale - previousMouseState.Y / App.Settings.General.ScreenScale; } }

        public static int MouseScrollWheel(KeyModifier modifier) {
            return ModifierMatches(modifier)
                ? currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue
                    ? -1
                    : currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue ? 1 : 0
                : 0;
        }

        private static bool ModifierMatches(KeyModifier mod) {
            return mod == KeyModifier._Any || CurrentModifier == mod;
        }

        private static KeyModifier GetCurrentModifier() {

            bool ctrl = currentKeyState.IsKeyDown(Keys.LeftControl) || currentKeyState.IsKeyDown(Keys.RightControl);
            bool alt = currentKeyState.IsKeyDown(Keys.LeftAlt) || currentKeyState.IsKeyDown(Keys.RightAlt);
            bool shift = currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift);

            return ctrl && alt && shift
                ? KeyModifier.CtrlShiftAlt
                : ctrl && alt
                ? KeyModifier.CtrlAlt
                : ctrl && shift
                ? KeyModifier.CtrlShift
                : alt && shift
                ? KeyModifier.ShiftAlt
                : alt ? KeyModifier.Alt : ctrl ? KeyModifier.Ctrl : shift ? KeyModifier.Shift : KeyModifier.None;
        }

        public static bool MouseIsDragging { get; private set; }
        /// <summary>
        /// True if the user just released a mouse drag this frame
        /// </summary>
        public static bool MouseJustEndedDragging { get; private set; }
    }

    public enum KeyModifier {
        None,
        Ctrl,
        Shift,
        Alt,
        CtrlShift,
        CtrlAlt,
        ShiftAlt,
        CtrlShiftAlt,
        _Any,
    }
}
