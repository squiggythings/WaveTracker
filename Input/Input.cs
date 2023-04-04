using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace WaveTracker
{
    public class Input
    {
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;

        static MouseState currentMouseState;
        static MouseState previousMouseState;

        public static float timeSinceLastClick;
        public static float timeSinceLastClickUp;

        static float lastTimeSinceLastClick;
        static float lastTimeSinceLastClickUp;
        static float timeSinceDoubleClick;

        public const int DOUBLE_CLICK_TIME = 350;
        public static int dialogOpenCooldown;

        static Dictionary<Keys, int> keyTimePairs;

        public static KeyboardState Keyboard { get { return currentKeyState; } }
        public static MouseState Mouse { get { return currentMouseState; } }

        public static Point lastClickLocation;
        public static Point lastClickReleaseLocation;

        static KeyModifier currentModifier;
        public static bool singleClick;
        public static bool doubleClick;
        public static bool internalDialogIsOpen;

        public static void Intialize()
        {
            keyTimePairs = new Dictionary<Keys, int>();
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                keyTimePairs.Add(k, 0);
            }
            timeSinceLastClick = 1000;
        }

        public static void DialogStarted()
        {
            dialogOpenCooldown = 5;
        }

        public static void GetState(GameTime gameTime)
        {
            if (dialogOpenCooldown > 0)
            {
                dialogOpenCooldown--;
                if (dialogOpenCooldown > 2)
                    return;
            }
            lastTimeSinceLastClick = timeSinceLastClick;
            lastTimeSinceLastClickUp = timeSinceLastClickUp;
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            currentModifier = getCurrentModifier();
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                if (currentKeyState.IsKeyDown(k))
                {
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



            if (GetClickDown(KeyModifier._Any))
            {
                doubleClick = false;
            }
            if (GetClickDown(KeyModifier._Any) && timeSinceLastClick < DOUBLE_CLICK_TIME && Vector2.Distance(lastClickLocation.ToVector2(), lastClickReleaseLocation.ToVector2()) < 5)
            {
                timeSinceDoubleClick = 0;
                doubleClick = true;
            }
            if (GetClickDown(KeyModifier._Any))
            {
                lastClickLocation = new Point(MousePositionX, MousePositionY);
                timeSinceLastClick = 0;
            }
            singleClick = false;
            if (GetClickUp(KeyModifier._Any))
            {
                timeSinceLastClickUp = 0;
                if (doubleClick == false)
                    singleClick = true;
                doubleClick = false;
                lastClickReleaseLocation = new Point(MousePositionX, MousePositionY);
            }
        }

        public static Vector2 MousePos { get { return new Vector2(MousePositionX, MousePositionY); } }

        public static bool GetKey(Keys key, KeyModifier modifier)
        {
            if (modifierMatches(modifier))
                return currentKeyState.IsKeyDown(key);
            else
                return false;
        }

        public static bool GetKeyDown(Keys key, KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
            else
                return false;
        }


        // TODO: FIX THIS METHOD
        public static bool GetKeyRepeat(Keys key, KeyModifier modifier)
        {

            if (modifierMatches(modifier))
            {
                if (keyTimePairs[key] == 1)
                    return true;
                if (keyTimePairs[key] > 30)
                {
                    return keyTimePairs[key] % 2 == 0;
                }
                return false;
            }
            else
                return false;
        }
        public static bool GetKeyUp(Keys key, KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key);
            else
                return false;
        }

        public static bool GetClickUp(KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
            else
                return false;
        }

        public static bool GetSingleClickUp(KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return GetClickUp(modifier) && singleClick;
            else
                return false;
        }
        public static bool GetClickDown(KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            else
                return false;
        }
        public static bool GetClick(KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return currentMouseState.LeftButton == ButtonState.Pressed;
            else
                return false;
        }

        public static bool GetDoubleClick(KeyModifier modifier)
        {

            if (modifierMatches(modifier))
                return GetClickDown(modifier) && doubleClick;
            else
                return false;
        }

        public static int MousePositionX { get { return (currentMouseState.X) / Game1.ScreenScale; } }
        public static int MousePositionY { get { return (currentMouseState.Y) / Game1.ScreenScale; } }

        public static int MouseDeltaX { get { return (currentMouseState.X / Game1.ScreenScale) - (previousMouseState.X / Game1.ScreenScale); } }

        public static int MouseDeltaY { get { return (currentMouseState.Y / Game1.ScreenScale) - (previousMouseState.Y / Game1.ScreenScale); } }

        public static int MouseScrollWheel(KeyModifier modifier)
        {
            if (modifierMatches(modifier))
            {
                if (currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue) return -1;
                if (currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue) return 1;
                return 0;
            }
            else
                return 0;
        }

        static bool modifierMatches(KeyModifier mod)
        {
            if (mod == KeyModifier._Any)
                return true;
            return currentModifier == mod;
        }

        static KeyModifier getCurrentModifier()
        {

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
    }


    public enum KeyModifier
    {
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
