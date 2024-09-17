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

        public const string VERSION = "1.0.4";
        private static App instance;

        private GraphicsDeviceManager graphics;

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

        public static GraphicsDevice GameGraphicsDevice {
            get {
                return instance.GraphicsDevice;
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


        public static GameTime GameTime { get; private set; }

        public App(string[] args) {
            instance = this;
            if (args.Length > 0) {
                if (Path.Exists(inputFilepath)) {
                    inputFilepath = args[0];
                }
            }

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
            Graphics.Initialize(Content, GraphicsDevice);

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

        private void OpenHelp() {
            try {
                Process.Start("explorer", "https://wavetracker.org/documentation");
            } catch {
                Dialogs.OpenMessageDialog("Could not open help!", MessageDialog.Icon.Error, "OK");
            }
        }

        private void OpenEffectList() {
            try {
                Process.Start("explorer", "https://wavetracker.org/documentation/effect-list");
            } catch {
                Dialogs.OpenMessageDialog("Could not open help!", MessageDialog.Icon.Error, "OK");
            }
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
            ]));
            MenuStrip.AddButton("Help", new Menu([
                new MenuOption("Open manual...", OpenHelp),
                new MenuOption("Effect list...", OpenEffectList),
                null,
                new MenuOption("Reset audio", ResetAudio),
            ]));

            base.Initialize();

        }

        protected override void LoadContent() {
            SaveLoad.NewFile();
            SaveLoad.LoadFile(inputFilepath);
        }

        private int dialogDelay = 0;
        protected override void Update(GameTime gameTime) {
            GameTime = gameTime;
            Window.Title = SaveLoad.FileNameWithoutExtension + (SaveLoad.IsSaved ? "" : "*") + " [#" + (CurrentSongIndex + 1) + " " + CurrentSong.ToString() + "] - WaveTracker " + VERSION;
            WindowHeight = Window.ClientBounds.Height / Settings.General.ScreenScale;
            WindowWidth = Window.ClientBounds.Width / Settings.General.ScreenScale;
            if (dialogDelay < 2) {
                dialogDelay++;
                if (dialogDelay == 2) {
                    SaveLoad.CheckCrashPath();
                    SaveLoad.SetCrashFlag(true);
                    PianoInput.Initialize();
                }
            }

            if (IsActive) {
                Input.GetState();
                PianoInput.Update();
            }
            else {
                Input.windowFocusTimer = 5;
                Input.dialogOpenCooldown = 3;
            }
            SaveLoad.AutosaveTick();
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

            Tooltip.Update();
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
            GameTime = gameTime;
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(UIColors.black);

            Graphics.spriteBatch.Begin(SpriteSortMode.Deferred,
                Graphics.BlendState,
                Graphics.SamplerState,
                Graphics.DepthStencilState,
                RasterizerState.CullNone
            );
            Graphics.Scale = Settings.General.ScreenScale;

            Graphics.SetFont();


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
                else {
                    Visualizer.DrawTracker();
                    Graphics.Scale = 1;
                    Visualizer.DrawPianoAndScopes();
                    Graphics.Scale = Settings.General.ScreenScale;
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

            Graphics.Write("focus: " + Input.focus, 2, 200, Color.Red);

            Graphics.spriteBatch.End();

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
            ContextMenu.CloseCurrent();

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
            SaveLoad.SetCrashFlag(false);
            AudioEngine.Stop();
            PianoInput.StopMIDI();
            SettingsProfile.WriteToDisk(Settings);
            base.OnExiting(sender, args);
        }
    }
}