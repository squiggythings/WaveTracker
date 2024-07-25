using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;

namespace WaveTracker {
    public class Input {

        /// <summary>
        /// The amount of time since the last frame in milliseconds
        /// </summary>
        public static int deltaTime;

        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;

        static MouseState currentMouseState;
        static MouseState previousMouseState;

        public static float timeSinceLastClick;
        public static float timeSinceLastClickUp;

        static float lastTimeSinceLastClick;
        static float lastTimeSinceLastClickUp;
        static float timeSinceDoubleClick;

        const int DOUBLE_CLICK_TIME = 350;
        const int KEY_REPEAT_DELAY = 500;
        const int KEY_REPEAT_TIME = 33;
        const float MOUSE_DRAG_DISTANCE = 5;
        public static int dialogOpenCooldown;

        static Dictionary<Keys, int> keyTimePairs;

        public static KeyboardState Keyboard { get { return currentKeyState; } }
        public static MouseState Mouse { get { return currentMouseState; } }

        public static Point lastClickLocation;
        public static Point lastRightClickLocation;
        public static Point lastClickReleaseLocation;
        public static Point lastRightClickReleaseLocation;

        static bool cancelClick;

        public static KeyModifier CurrentModifier { get; set; }
        static bool doubleClick;
        static bool dragging;
        static bool wasDragging;
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
            keyTimePairs = new Dictionary<Keys, int>();
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                keyTimePairs.Add(k, 0);
            }
            timeSinceLastClick = 1000;
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
            lastTimeSinceLastClick = timeSinceLastClick;
            lastTimeSinceLastClickUp = timeSinceLastClickUp;
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            currentPressedKeys = currentKeyState.GetPressedKeys();
            CurrentModifier = GetCurrentModifier();
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                if (currentKeyState.IsKeyDown(k)) {
                    keyTimePairs[k] += gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                    keyTimePairs[k] = 0;

            }
            CurrentPressedKey = Keys.None;
            foreach (Keys k in currentPressedKeys) {
                if (k != Keys.LeftShift &&
                    k != Keys.RightShift &&
                    k != Keys.LeftControl &&
                    k != Keys.RightControl &&
                    k != Keys.LeftAlt &&
                    k != Keys.RightAlt) {
                    CurrentPressedKey = k;
                }
            }
            CurrentPressedShortcut = new KeyboardShortcut(CurrentPressedKey, CurrentModifier);


            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            timeSinceLastClick += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceLastClickUp += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceDoubleClick += gameTime.ElapsedGameTime.Milliseconds;

            if (GetRightClickDown(KeyModifier._Any)) {
                lastRightClickLocation = new Point(MousePositionX, MousePositionY);
            }

            if (GetClickDown(KeyModifier._Any)) {
                lastClickLocation = new Point(MousePositionX, MousePositionY);
                lastClickFocus = focus;
                if (!doubleClick && timeSinceLastClick < DOUBLE_CLICK_TIME && Vector2.Distance(lastClickReleaseLocation.ToVector2(), MousePos) < MOUSE_DRAG_DISTANCE) {
                    //this is a double click
                    doubleClick = true;
                }
                else {
                    //this is a normal click
                    doubleClick = false;
                }
                timeSinceLastClick = 0;
            }

            if (GetClick(KeyModifier._Any)) {
                if (Vector2.Distance(lastClickLocation.ToVector2(), MousePos) > MOUSE_DRAG_DISTANCE) {
                    dragging = true;
                }
            }
            else {
                if (dragging) {
                    wasDragging = true;
                }
                else {
                    wasDragging = false;
                }
                dragging = false;
            }
            if (GetClickUp(KeyModifier._Any)) {
                timeSinceLastClickUp = 0;
                lastClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }

            if (GetRightClickUp(KeyModifier._Any)) {
                lastRightClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }
        }

        static Vector2 MousePos { get { return new Vector2(MousePositionX, MousePositionY); } }

        public static void CancelClick() {
            cancelClick = true;
        }

        public static bool GetKey(Keys key, KeyModifier modifier) {
            if (modifierMatches(modifier))
                return currentKeyState.IsKeyDown(key);
            else
                return false;
        }

        public static bool GetKeyDown(Keys key, KeyModifier modifier) {

            if (modifierMatches(modifier))
                return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
            else
                return false;
        }


        public static bool GetKeyRepeat(Keys key, KeyModifier modifier) {

            if (modifierMatches(modifier)) {
                if (currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key))
                    return true;
                if (keyTimePairs[key] > KEY_REPEAT_DELAY) {
                    keyTimePairs[key] -= KEY_REPEAT_TIME;
                    return true;
                }
                return false;
            }
            else
                return false;
        }

        public static bool GetKeyUp(Keys key, KeyModifier modifier) {

            if (modifierMatches(modifier))
                return !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key);
            else
                return false;
        }

        public static bool GetClickUp(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
            else
                return false;
        }

        public static bool GetSingleClickUp(KeyModifier modifier) {

            return GetClickUp(modifier) && !doubleClick;
        }
        public static bool GetClickDown(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            else
                return false;
        }
        public static bool GetClick(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.LeftButton == ButtonState.Pressed;
            else
                return false;
        }

        public static bool GetRightClick(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.RightButton == ButtonState.Pressed;
            else
                return false;
        }

        public static bool GetRightClickUp(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
            else
                return false;
        }
        public static bool GetRightClickDown(KeyModifier modifier) {

            if (modifierMatches(modifier) && !cancelClick)
                return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
            else
                return false;
        }

        public static bool GetDoubleClick(KeyModifier modifier) {
            return GetClickDown(modifier) && doubleClick;
        }

        public static int MousePositionX { get { return (int)(currentMouseState.X / App.Settings.General.ScreenScale); } }
        public static int MousePositionY { get { return (int)(currentMouseState.Y / App.Settings.General.ScreenScale); } }

        public static int MouseDeltaX { get { return (int)(currentMouseState.X / App.Settings.General.ScreenScale) - (int)(previousMouseState.X / App.Settings.General.ScreenScale); } }

        public static int MouseDeltaY { get { return (int)(currentMouseState.Y / App.Settings.General.ScreenScale) - (int)(previousMouseState.Y / App.Settings.General.ScreenScale); } }

        public static int MouseScrollWheel(KeyModifier modifier) {
            if (modifierMatches(modifier)) {
                if (currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue) return -1;
                if (currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue) return 1;
                return 0;
            }
            else
                return 0;
        }

        static bool modifierMatches(KeyModifier mod) {
            if (mod == KeyModifier._Any)
                return true;
            return CurrentModifier == mod;
        }

        static KeyModifier GetCurrentModifier() {

            bool ctrl = (currentKeyState.IsKeyDown(Keys.LeftControl) || currentKeyState.IsKeyDown(Keys.RightControl));
            bool alt = (currentKeyState.IsKeyDown(Keys.LeftAlt) || currentKeyState.IsKeyDown(Keys.RightAlt));
            bool shift = (currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift));

            if (ctrl && alt && shift)
                return KeyModifier.CtrlShiftAlt;
            if (ctrl && alt)
                return KeyModifier.CtrlAlt;
            if (ctrl && shift)
                return KeyModifier.CtrlShift;
            if (alt && shift)
                return KeyModifier.ShiftAlt;
            if (alt)
                return KeyModifier.Alt;
            if (ctrl)
                return KeyModifier.Ctrl;
            if (shift)
                return KeyModifier.Shift;
            return KeyModifier.None;
        }

        public static bool MouseIsDragging {
            get { return dragging; }
        }
        /// <summary>
        /// True if the user just released a mouse drag this frame
        /// </summary>
        public static bool MouseJustEndedDragging {
            get { return wasDragging; }
        }

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
