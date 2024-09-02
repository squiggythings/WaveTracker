using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WaveTracker.Audio;
using WaveTracker.Rendering;
using WaveTracker.Tracker;
using WaveTracker.UI;

namespace WaveTracker {
    public class App : Game {

        public const string VERSION = "1.0.1";
        private static App instance;

        private GraphicsDeviceManager graphics;
        private SpriteBatch targetBatch;
        private RenderTarget2D target;

        /// <summary>
        /// The height of the app in scaled pixels
        /// </summary>
        public static int WindowHeight { get; private set; }
        /// <summary>
        /// The width of the app in scaled pixels
        /// </summary>
        public static int WindowWidth { get; private set; }

        /// <summary>
        /// The module panel
        /// </summary>
        public static ModulePanel ModulePanel { get; private set; }
        /// <summary>
        /// The edit settings panel
        /// </summary>
        public static EditSettings EditSettings { get; private set; }
        /// <summary>
        /// The frames panel
        /// </summary>
        public static FramesPanel FramesPanel { get; private set; }
        /// <summary>
        /// The toolbar at the top of the screen
        /// </summary>
        public static Toolbar Toolbar { get; private set; }

        /// <summary>
        /// The visualizer
        /// </summary>
        public static Visualizer Visualizer { get; private set; }
        /// <summary>
        /// Is the visualizer active or not
        /// </summary>
        public static bool VisualizerMode { get; set; }
        /// <summary>
        /// The pattern editor
        /// </summary>
        public static PatternEditor PatternEditor { get; private set; }

        /// <summary>
        /// The instrument bank panel
        /// </summary>
        public static InstrumentBank InstrumentBank { get; private set; }

        /// <summary>
        /// The Instrument Editor window
        /// </summary>
        public static InstrumentEditor InstrumentEditor { get; private set; }

        /// <summary>
        /// The Wave Bank panel
        /// </summary>
        public static WaveBank WaveBank { get; private set; }

        /// <summary>
        /// The Wave Editor window
        /// </summary>
        public static WaveEditor WaveEditor { get; private set; }

        /// <summary>
        /// The module currently being edited
        /// </summary>
        public static WTModule CurrentModule { get; set; }
        /// <summary>
        /// The index of the song currently being edited
        /// </summary>
        public static int CurrentSongIndex { get; set; }
        /// <summary>
        /// The song currently being edited
        /// </summary>
        public static WTSong CurrentSong { get { return CurrentModule.Songs[CurrentSongIndex]; } }

        /// <summary>
        /// The App's currently loaded settings
        /// </summary>
        public static SettingsProfile Settings { get; private set; }

        /// <summary>
        /// A reference to the settings' keyboard shortcuts
        /// </summary>
        public static Dictionary<string, KeyboardShortcut> Shortcuts {
            get {
                return Settings.Keyboard.Shortcuts;
            }
        }

        /// <summary>
        /// A reference to the window
        /// </summary>
        public static GameWindow ClientWindow {
            get {
                return instance.Window;
            }
        }

        /// <summary>
        /// Height of the menustrip
        /// </summary>
        public const int MENUSTRIP_HEIGHT = 10;

        /// <summary>
        /// The timer to set the mouse cursor to an directional arrow instead of the default
        /// </summary>
        public static int MouseCursorArrow { get; set; }

        /// <summary>
        /// The menustrip at the top of the screen
        /// </summary>
        public MenuStrip MenuStrip { get; private set; }

        /// <summary>
        /// If the application is being opened to edit a file, this is that filepath.
        /// </summary>
        private string inputFilepath;

        public App(string[] args) {
            instance = this;
            if (args.Length > 0) {
                inputFilepath = args[0];
            }

            graphics = new GraphicsDeviceManager(this);
            graphics.ApplyChanges();
            Window.Position = new Point(-8, 0);
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            // form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            // form.FormClosing += ClosingForm;
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

        private Menu CreateModuleMenu() {
            return new Menu([
                new MenuOption("Module Settings", Dialogs.moduleSettings.Open),
                null,
                new SubMenu("Cleanup", [
                        new MenuOption("Remove unused instruments", CurrentModule.RemoveUnusedInstruments),
                        new MenuOption("Remove unused waves", CurrentModule.RemoveUnusedWaves),
                    ])
                ]);
        }
        private Menu CreateSongMenu() {
            return new Menu([
                new MenuOption("Insert frame", PatternEditor.InsertNewFrame, !VisualizerMode),
                new MenuOption("Remove frame", PatternEditor.RemoveFrame, !VisualizerMode),
                new MenuOption("Duplicate frame", PatternEditor.DuplicateFrame, !VisualizerMode),
                null,
                new MenuOption("Move frame left", PatternEditor.MoveFrameLeft, !VisualizerMode),
                new MenuOption("Move frame right", PatternEditor.MoveFrameRight, !VisualizerMode),
            ]);
        }

        protected override void Initialize() {

            CurrentModule = new WTModule();
            WaveBank = new WaveBank(510, 18 + MENUSTRIP_HEIGHT);
            ChannelManager.Initialize(WTModule.MAX_CHANNEL_COUNT);
            PatternEditor = new PatternEditor(0, 184 + MENUSTRIP_HEIGHT);
            InstrumentBank = new InstrumentBank(790, 152 + MENUSTRIP_HEIGHT);
            InstrumentEditor = new InstrumentEditor();
            Visualizer = new Visualizer();
            Dialogs.Initialize();
            EditSettings = new EditSettings(312, 18 + MENUSTRIP_HEIGHT);
            Toolbar = new Toolbar(2, 0 + MENUSTRIP_HEIGHT);
            WaveEditor = new WaveEditor();
            FramesPanel = new FramesPanel(2, 106 + MENUSTRIP_HEIGHT, 504, 42);
            ModulePanel = new ModulePanel(2, 18 + MENUSTRIP_HEIGHT);
            AudioEngine.Initialize();

            IsFixedTimeStep = false;

            MenuStrip = new MenuStrip(0, 0, 960, null);
            MenuStrip.AddButton("File", SaveLoad.CreateFileMenu);
            MenuStrip.AddButton("Edit", PatternEditor.CreateEditMenu);
            MenuStrip.AddButton("Song", CreateSongMenu);
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
            SaveLoad.LoadFile(inputFilepath);
        }

        private int midiDelay = 0;
        protected override void Update(GameTime gameTime) {
            Window.Title = SaveLoad.FileNameWithoutExtension + (SaveLoad.IsSaved ? "" : "*") + " [#" + (CurrentSongIndex + 1) + " " + CurrentSong.ToString() + "] - WaveTracker " + VERSION;
            WindowHeight = Window.ClientBounds.Height / Settings.General.ScreenScale;
            WindowWidth = Window.ClientBounds.Width / Settings.General.ScreenScale;
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

            int pianoInput = -1;
            if (WaveEditor.GetPianoMouseInput() > -1) {
                pianoInput = WaveEditor.GetPianoMouseInput();
            }

            if (InstrumentEditor.GetPianoMouseInput() > -1) {
                pianoInput = InstrumentEditor.GetPianoMouseInput();
            }

            PianoInput.ReceivePreviewPianoInput(pianoInput);

            Playback.Update();

            if (!VisualizerMode) {
                ModulePanel.Update();
                FramesPanel.Update();
                EditSettings.Update();
            }

            Toolbar.Update();
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

            targetBatch.Begin(SpriteSortMode.Deferred,
                new BlendState {
                    ColorSourceBlend = Blend.SourceAlpha,
                    ColorDestinationBlend = Blend.InverseSourceAlpha,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.InverseSourceAlpha,
                },
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone
            );

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
                FramesPanel.Draw();

                // draw song settings
                ModulePanel.Draw();

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

            Toolbar.Draw();
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

            targetBatch.End();

            //set rendering back to the back buffer
            GraphicsDevice.SetRenderTarget(null);

            //render target to back buffer
            targetBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Settings.General.ScreenScale % 1 == 0 ? SamplerState.PointClamp : SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
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
            // System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(instance.Window.Handle);

            // form.Close();
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
        /// Called after the unsaved changes dialog is finished
        /// </summary>
        /// <param name="result"></param>
        private void UnsavedChangesCallback(string result) {
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