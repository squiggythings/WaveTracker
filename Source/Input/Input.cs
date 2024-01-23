using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;

namespace WaveTracker {
    public class Input {

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
        const float MOUSE_DRAG_DISTANCE = 5;
        public static int dialogOpenCooldown;

        static Dictionary<Keys, int> keyTimePairs;

        public static KeyboardState Keyboard { get { return currentKeyState; } }
        public static MouseState Mouse { get { return currentMouseState; } }

        public static Point lastClickLocation;
        public static Point lastClickReleaseLocation;
        static bool cancelClick;

        static KeyModifier currentModifier;
        static bool singleClick;
        static bool doubleClick;
        static bool dragging;
        static bool wasDragging;
        public static bool internalDialogIsOpen;
        public static Element focus = null;
        public static int focusTimer;
        static double clickTimer;
        public static void Intialize() {
            keyTimePairs = new Dictionary<Keys, int>();
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                keyTimePairs.Add(k, 0);
            }
            timeSinceLastClick = 1000;
        }

        public static void DialogStarted() {
            dialogOpenCooldown = 5;
        }

        public static void GetState(GameTime gameTime) {
            cancelClick = false;
            if (focusTimer > 0) {
                focusTimer--;
            }
            if (dialogOpenCooldown > 0) {
                dialogOpenCooldown--;
                if (dialogOpenCooldown > 2)
                    return;
            }
            lastTimeSinceLastClick = timeSinceLastClick;
            lastTimeSinceLastClickUp = timeSinceLastClickUp;
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            currentModifier = getCurrentModifier();
            foreach (Keys k in Enum.GetValues(typeof(Keys))) {
                if (currentKeyState.IsKeyDown(k)) {
                    keyTimePairs[k] += 1;
                }
                else
                    keyTimePairs[k] = 0;

            }


            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            timeSinceLastClick += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceLastClickUp += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceDoubleClick += gameTime.ElapsedGameTime.Milliseconds;



            if (GetClickDown(KeyModifier._Any)) {
                lastClickLocation = new Point(MousePositionX, MousePositionY);
                if (timeSinceLastClick < DOUBLE_CLICK_TIME && Vector2.Distance(lastClickReleaseLocation.ToVector2(), MousePos) < MOUSE_DRAG_DISTANCE) {

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
            singleClick = false;
            if (GetClickUp(KeyModifier._Any)) {
                timeSinceLastClickUp = 0;
                //if (doubleClick == false)
                //    singleClick = true;
                //doubleClick = false;
                lastClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }
        }

        public static Vector2 MousePos { get { return new Vector2(MousePositionX, MousePositionY); } }

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


        // TODO: FIX THIS METHOD
        public static bool GetKeyRepeat(Keys key, KeyModifier modifier) {

            if (modifierMatches(modifier)) {
                if (keyTimePairs[key] == 1)
                    return true;
                if (keyTimePairs[key] > 30) {
                    return keyTimePairs[key] % 2 == 0;
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

        public static bool GetDoubleClick(KeyModifier modifier) {

            return GetClickDown(modifier) && doubleClick;
        }

        public static int MousePositionX { get { return (currentMouseState.X) / Game1.ScreenScale; } }
        public static int MousePositionY { get { return (currentMouseState.Y) / Game1.ScreenScale; } }

        public static int MouseDeltaX { get { return (currentMouseState.X / Game1.ScreenScale) - (previousMouseState.X / Game1.ScreenScale); } }

        public static int MouseDeltaY { get { return (currentMouseState.Y / Game1.ScreenScale) - (previousMouseState.Y / Game1.ScreenScale); } }

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
            return currentModifier == mod;
        }

        static KeyModifier getCurrentModifier() {

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
