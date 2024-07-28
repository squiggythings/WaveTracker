using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.Security.Principal;
using WaveTracker.UI;
using WaveTracker.Rendering;
using WaveTracker.Tracker;
using WaveTracker.Audio;
using System.IO;

namespace WaveTracker {
    public class App : Game {
        static App instance;
        public const string VERSION = "0.3.1";

        private GraphicsDeviceManager graphics;
        private SpriteBatch targetBatch;
        //public static Texture2D channelHeaderSprite;

        // public static float ScreenScale { get; } = 2;
        /// <summary>
        /// The height of the app in scaled pixels
        /// </summary>
        public static int WindowHeight { get; private set; }
        /// <summary>
        /// The width of the app in scaled pixels
        /// </summary>
        public static int WindowWidth { get; private set; }
        RenderTarget2D target;


        public static SongSettings SongSettings { get; private set; }
        public static EditSettings EditSettings { get; private set; }
        FramesPanel frameView;
        public Toolbar toolbar;
        static int pianoInput;
        public static int MouseCursorArrow { get; set; }
        public static bool VisualizerMode { get; set; }
        public static Visualizer Visualizer { get; set; }
        public static PatternEditor PatternEditor { get; private set; }
        public static InstrumentBank InstrumentBank { get; private set; }

        public static InstrumentEditor InstrumentEditor { get; private set; }
        public static WaveBank WaveBank { get; set; }
        public static WaveEditor WaveEditor { get; private set; }


        public static WTModule CurrentModule { get; set; }
        public static int CurrentSongIndex { get; set; }
        public static WTSong CurrentSong { get { return CurrentModule.Songs[CurrentSongIndex]; } }

        public static SettingsProfile Settings { get; private set; }

        public static Dictionary<string, KeyboardShortcut> Shortcuts => Settings.Keyboard.Shortcuts;

        public const int MENUSTRIP_HEIGHT = 10;

        public static GameWindow ClientWindow => instance.Window;

        public MenuStrip MenuStrip { get; set; }

        string inputFilepath;


        public App(string[] args) {
            instance = this;
            if (args.Length > 0)
                inputFilepath = args[0];

            graphics = new GraphicsDeviceManager(this);
            graphics.ApplyChanges();
            Window.Position = new Point(-8, 0);
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            form.FormClosing += ClosingForm;
            if (!Directory.Exists(SaveLoad.ThemeFolderPath)) {
                Directory.CreateDirectory(SaveLoad.ThemeFolderPath);
                File.WriteAllText(Path.Combine(SaveLoad.ThemeFolderPath, "Default.wttheme"), ColorTheme.CreateString(ColorTheme.Default));
                File.WriteAllText(Path.Combine(SaveLoad.ThemeFolderPath, "Famitracker.wttheme"), ColorTheme.CreateString(ColorTheme.Famitracker));
                File.WriteAllText(Path.Combine(SaveLoad.ThemeFolderPath, "Fruity.wttheme"), ColorTheme.CreateString(ColorTheme.Fruity));
                File.WriteAllText(Path.Combine(SaveLoad.ThemeFolderPath, "OpenMPT.wttheme"), ColorTheme.CreateString(ColorTheme.OpenMPT));
                File.WriteAllText(Path.Combine(SaveLoad.ThemeFolderPath, "Neon.wttheme"), ColorTheme.CreateString(ColorTheme.Neon));
            }
            Input.Intialize();

            Settings = SettingsProfile.ReadFromDisk();
            SaveLoad.ReadRecentFiles();
        }

        /// <summary>
        /// Forces a call of update then draw
        /// </summary>
        public static void ForceUpdate() {
            instance.Tick();
        }

        Menu CreateModuleMenu() {
            return new Menu([
                new MenuOption("Module Settings", Dialogs.moduleSettings.Open),
                null,
                new SubMenu("Cleanup", [
                        new MenuOption("Remove unused instruments", CurrentModule.RemoveUnusedInstruments),
                        new MenuOption("Remove unused waves", CurrentModule.RemoveUnusedWaves),
                    ])
                ]);
        }

        protected override void Initialize() {


            CurrentModule = new WTModule();
            WaveBank = new WaveBank(510, 18 + MENUSTRIP_HEIGHT);
            ChannelManager.Initialize(WTModule.MAX_CHANNEL_COUNT, WaveBank);
            PatternEditor = new PatternEditor(0, 184 + MENUSTRIP_HEIGHT);
            InstrumentBank = new InstrumentBank(790, 152 + MENUSTRIP_HEIGHT);
            InstrumentEditor = new InstrumentEditor();
            Visualizer = new Visualizer();
            Dialogs.Initialize();
            EditSettings = new EditSettings(312, 18 + MENUSTRIP_HEIGHT);
            toolbar = new Toolbar(2, 0 + MENUSTRIP_HEIGHT);
            WaveEditor = new WaveEditor();
            frameView = new FramesPanel(2, 106 + MENUSTRIP_HEIGHT, 504, 42);
            SongSettings = new SongSettings(2, 18 + MENUSTRIP_HEIGHT);
            AudioEngine.Initialize();

            IsFixedTimeStep = false;

            MenuStrip = new MenuStrip(0, 0, 960, null);
            MenuStrip.AddButton("File", SaveLoad.CreateFileMenu);
            MenuStrip.AddButton("Edit", PatternEditor.CreateEditMenu);
            MenuStrip.AddButton("Song", new Menu([
                new MenuOption("Insert frame", PatternEditor.InsertNewFrame),
                new MenuOption("Remove frame", PatternEditor.RemoveFrame),
                new MenuOption("Duplicate frame", PatternEditor.DuplicateFrame),
                null,
                new MenuOption("Move frame left", PatternEditor.MoveFrameLeft),
                new MenuOption("Move frame right", PatternEditor.MoveFrameRight),
            ]));
            MenuStrip.AddButton("Module", CreateModuleMenu);
            MenuStrip.AddButton("Instrument", InstrumentBank.CreateInstrumentMenu);
            MenuStrip.AddButton("Tracker", new Menu([
                new MenuOption("Play", Playback.Play),
                new MenuOption("Play from beginning", Playback.PlayFromBeginning),
                new MenuOption("Play from cursor", Playback.PlayFromCursor),
                new MenuOption("Stop", Playback.Stop),
                null,
                new MenuOption("Toggle edit mode", PatternEditor.ToggleEditMode),
                null,
                new MenuOption("Toggle channel", ChannelManager.ToggleCurrentChannel),
                new MenuOption("Solo channel", ChannelManager.SoloCurrentChannel),
                null,
                new MenuOption("Reset audio", ResetAudio),

            ]));

            base.Initialize();

        }

        protected override void LoadContent() {

            Graphics.font = Content.Load<SpriteFont>("custom_font");
            Graphics.img = Content.Load<Texture2D>("img");
            Graphics.pixel = new Texture2D(GraphicsDevice, 1, 1);
            Graphics.pixel.SetData(new[] { Color.White });
            targetBatch = new SpriteBatch(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            SaveLoad.NewFile();
            SaveLoad.ReadFrom(inputFilepath);
        }
        int midiDelay = 0;
        protected override void Update(GameTime gameTime) {
            Window.Title = SaveLoad.FileNameWithoutExtension + (SaveLoad.IsSaved ? "" : "*") + " [#" + (CurrentSongIndex + 1) + " " + CurrentSong.ToString() + "] - WaveTracker " + VERSION;
            WindowHeight = (Window.ClientBounds.Height / Settings.General.ScreenScale);
            WindowWidth = (Window.ClientBounds.Width / Settings.General.ScreenScale);
            if (midiDelay < 2) {
                midiDelay++;
                if (midiDelay == 2) {
                    PianoInput.Initialize();
                }
            }

            if (IsActive) {
                Input.GetState(gameTime);
                PianoInput.Update();
            }
            else {
                Input.windowFocusTimer = 5;
                Input.dialogOpenCooldown = 3;
            }

            if (Input.dialogOpenCooldown == 0) {
                if (Input.MousePositionX > 1 && Input.MousePositionX < WindowWidth - 1) {
                    if (Input.MousePositionY > 1 && Input.MousePositionY < WindowHeight - 1) {
                        if (MouseCursorArrow == 0) {
                            Mouse.SetCursor(MouseCursor.Arrow);
                        }
                        else {
                            Mouse.SetCursor(MouseCursor.SizeNS);
                            MouseCursorArrow--;
                        }
                    }
                }
            }

            Tooltip.Update(gameTime);
            if (Shortcuts["General\\Reset audio"].IsPressedDown) {
                ResetAudio();
            }
            PatternEditor.Update();
            if (!VisualizerMode) {
                WaveBank.Update();
                WaveEditor.Update();
                InstrumentBank.Update();
                InstrumentEditor.Update();
            }

            pianoInput = -1;
            if (WaveEditor.GetPianoMouseInput() > -1)
                pianoInput = WaveEditor.GetPianoMouseInput();
            if (InstrumentEditor.GetPianoMouseInput() > -1)
                pianoInput = InstrumentEditor.GetPianoMouseInput();
            PianoInput.ReceivePreviewPianoInput(pianoInput);

            Playback.Update();


            if (!VisualizerMode) {
                SongSettings.Update();
                frameView.Update();
                EditSettings.Update();
            }

            toolbar.Update();
            MenuStrip.Update();
            Dialogs.Update();

            if (VisualizerMode) {
                Visualizer.Update();
            }

            ContextMenu.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();

            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.Clear(UIColors.black);


            // TODO: Add your drawing code here
            targetBatch.Begin(SpriteSortMode.Deferred, new BlendState {
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
            }, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            Graphics.batch = targetBatch;

            if (!VisualizerMode) {
                // draw pattern editor
                PatternEditor.Draw();

                // draw instrument bank
                InstrumentBank.Draw();

                // draw wave bank
                WaveBank.Draw();

                // draw edit settings
                EditSettings.Draw();

                // draw frame view
                frameView.Draw();

                // draw song settings
                SongSettings.Draw();

                // draw click position
                //Rendering.Graphics.DrawRect(Input.lastClickLocation.X, Input.lastClickLocation.Y, 1, 1, Color.Red);
                //Rendering.Graphics.DrawRect(Input.lastClickReleaseLocation.X, Input.lastClickReleaseLocation.Y, 1, 1, Color.DarkRed);
            }
            else {
                if (!Settings.Visualizer.DrawInHighResolution) {
                    Visualizer.DrawPianoAndScopes();
                    Visualizer.DrawTracker();
                }
                else if (Input.focus == null) {
                    Visualizer.DrawTracker();
                }
            }


            toolbar.Draw();
            MenuStrip.Draw();

            if (!VisualizerMode) {
                WaveEditor.Draw();
                InstrumentEditor.Draw();
            }

            Dialogs.Draw();
            Dropdown.DrawCurrentMenu();
            DropdownButton.DrawCurrentMenu();
            ContextMenu.Draw();
            Tooltip.Draw();
            if (VisualizerMode && Input.focus == null && Settings.Visualizer.DrawInHighResolution) {
                Visualizer.DrawTracker();
            }


            //int y = 10;
            //foreach (MMDevice k in audioEngine.devices)
            //{
            //    Rendering.Graphics.Write(k.DeviceFriendlyName, 2, y, Color.Red);
            //    y += 10;

            //}
            //Rendering.Graphics.Write("AudioStatus: " + audioEngine.wasapiOut.PlaybackState.ToString(), 2, 2, Color.Red);
            //Rendering.Graphics.Write("filename: " + filename, 2, 12, Color.Red);
            //Rendering.Graphics.Write("FPS: " + 1 / gameTime.ElapsedGameTime.TotalSeconds, 2, 2, Color.Red);

            //Graphics.Write(MidiInput.GetMidiNote + ", " + pianoInput, 2, 250, Color.Red);
            //Graphics.Write("@" + Input.focus + "", 2, 260, Color.Red);

            targetBatch.End();



            //set rendering back to the back buffer
            GraphicsDevice.SetRenderTarget(null);
            //render target to back buffer
            targetBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, (Settings.General.ScreenScale % 1) == 0 ? SamplerState.PointClamp : SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
            targetBatch.Draw(target, new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * Settings.General.ScreenScale, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * Settings.General.ScreenScale), Color.White);
            if (VisualizerMode && Input.focus == null && Settings.Visualizer.DrawInHighResolution) {
                Visualizer.DrawPianoAndScopes();
            }
            targetBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Resets all channels and audio
        /// </summary>
        public static void ResetAudio() {
            ChannelManager.Reset();
            PianoInput.ReadMidiDevices();
            AudioEngine.Reset();
        }

        /// <summary>
        /// Closes WaveTracker
        /// </summary>
        public static void ExitApplication() {
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(instance.Window.Handle);

            form.Close();
        }

        /// <summary>
        /// Called before the app closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClosingForm(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!SaveLoad.IsSaved) {
                e.Cancel = true;
                SaveLoad.DoSaveChangesDialog(UnsavedChangesCallback);
            }

        }

        /// <summary>
        /// Called
        /// </summary>
        /// <param name="result"></param>
        void UnsavedChangesCallback(string result) {
            if (result == "Yes") {
                SaveLoad.SaveFile();
            }
            else if (result == "Cancel") {
                return;
            }
            Exit();
        }

        protected override void OnExiting(object sender, EventArgs args) {
            Debug.WriteLine("Closing WaveTracker...");
            AudioEngine.Stop();
            PianoInput.StopMIDI();
            SettingsProfile.WriteToDisk(Settings);
            base.OnExiting(sender, args);
        }
    }
}