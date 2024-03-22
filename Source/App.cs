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

namespace WaveTracker {
    public class App : Game {
        public const string VERSION = "0.2.1";

        private GraphicsDeviceManager graphics;
        private SpriteBatch targetBatch;
        //public static Texture2D channelHeaderSprite;

        public int ScreenWidth = 1920;
        public int ScreenHeight = 1080;
        public static int ScreenScale = 2;
        /// <summary>
        /// The height of the app in scaled pixels
        /// </summary>
        public static int WindowHeight { get; private set; }
        /// <summary>
        /// The width of the app in scaled pixels
        /// </summary>
        public static int WindowWidth { get; private set; }
        RenderTarget2D target;

        WaveBank waveBank;
        SongSettings songSettings;
        EditSettings editSettings;
        FramesPanel frameView;
        public Toolbar toolbar;
        AudioEngine audioEngine;
        int lastPianoKey;
        public static Song newSong;
        public static int pianoInput;
        public static int mouseCursorArrow;
        public static bool VisualizerMode;
        public static Visualization visualization;
        string filename;
        public static PatternEditor PatternEditor { get; private set; }
        public static InstrumentBank InstrumentBank { get; private set; }
        public static WTModule CurrentModule { get; set; }
        public static int CurrentSongIndex { get; set; }
        public static WTSong CurrentSong { get { return CurrentModule.Songs[CurrentSongIndex]; } }



        public App(string[] args) {
            if (args.Length > 0)
                filename = args[0];
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferWidth = 1920;  // set this value to the desired width of your window
            //graphics.PreferredBackBufferHeight = 1080 - 72;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            Window.Position = new Point(-8, 0);
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Preferences.profile = PreferenceProfile.DefaultProfile;
            Preferences.ReadFromFile();
            //frameRenderer = new FrameRenderer();
            frameView = new FramesPanel(2, 106, 504, 42);
            songSettings = new SongSettings();
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            PatternEditor = new PatternEditor(0, 184);

        }

        protected override void Initialize() {

            Input.Intialize();
            //frameRenderer.x = 0;
            //frameRenderer.y = 151;
            //Song.currentSong = new Song();
            //newSong = Song.currentSong.Clone();

            CurrentModule = new WTModule();
            //PatternEditor.OnSwitchSong();
            
            waveBank = new WaveBank();
            InstrumentBank = new InstrumentBank();
            InstrumentBank.Initialize();
            InstrumentBank.editor = new InstrumentEditor();
            InstrumentBank.editor.browser = new SampleBrowser();
            Dialogs.Initialize();
            editSettings = new EditSettings();
            toolbar = new Toolbar();
            waveBank.editor = new WaveEditor();
            songSettings = new SongSettings();
            frameView.Initialize();
            audioEngine = new AudioEngine();
            audioEngine.Initialize();
            ChannelManager.Initialize(WTModule.MAX_CHANNEL_COUNT, waveBank);
            //frameRenderer.Initialize();
            //FrameEditor.channelScrollbar = new UI.ScrollbarHorizontal(22, 323, 768, 7, null);
            //FrameEditor.channelScrollbar.SetSize(Tracker.Song.CHANNEL_COUNT, 12);
            visualization = new Visualization();
            IsFixedTimeStep = false;
            base.Initialize();

        }

        protected override void LoadContent() {
            
            //Checkbox.textureSheet = Content.Load<Texture2D>("instrumentwindow");
            //NumberBox.buttons = Content.Load<Texture2D>("window_edit");
            
            Graphics.font = Content.Load<SpriteFont>("custom_font");
            Graphics.img = Content.Load<Texture2D>("img");
            
            Graphics.pixel = new Texture2D(GraphicsDevice, 1, 1);
            Graphics.pixel.SetData(new[] { Color.White });
            // TODO: use this.Content to load your game content here
            targetBatch = new SpriteBatch(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
            SaveLoad.NewFile();
            SaveLoad.LoadFrom(filename);
        }

        protected override void Update(GameTime gameTime) {

            Window.Title = SaveLoad.FileName + (SaveLoad.IsSaved ? "" : "*") + " - WaveTracker v" + VERSION;
            WindowHeight = Window.ClientBounds.Height / ScreenScale;
            WindowWidth = Window.ClientBounds.Width / ScreenScale;


            if (IsActive) {
                Input.GetState(gameTime);
            }
            else {
                Input.windowFocusTimer = 5;
                Input.dialogOpenCooldown = 3;
            }
            if (Input.dialogOpenCooldown == 0) {
                int mouseX = Mouse.GetState().X;
                int mouseY = Mouse.GetState().Y;
                int width = Window.ClientBounds.Width - 2;
                int height = Window.ClientBounds.Height - 2;
                if (new Rectangle(1, 1, width, height).Contains(mouseX, mouseY))
                    if (mouseCursorArrow == 0) {
                        Mouse.SetCursor(MouseCursor.Arrow);
                    }
                    else {
                        Mouse.SetCursor(MouseCursor.SizeNS);
                        mouseCursorArrow--;
                    }
            }

            Tooltip.Update(gameTime);
            if (Input.GetKeyDown(Keys.F12, KeyModifier.None)) {
                ChannelManager.Reset();
                audioEngine.Reset();
            }
            PatternEditor.Update();
            if (!VisualizerMode) {
                waveBank.Update();
                InstrumentBank.Update();
            }
            if (Input.focus == null || Input.focus == waveBank.editor || Input.focus == InstrumentBank.editor)
                pianoInput = Helpers.GetPianoInput(PatternEditor.CurrentOctave);
            else {
                pianoInput = -1;
            }
            waveBank.editor.Update();

            if (waveBank.editor.GetPianoMouseInput() > -1)
                pianoInput = waveBank.editor.GetPianoMouseInput();
            if (InstrumentBank.editor.GetPianoMouseInput() > -1)
                pianoInput = InstrumentBank.editor.GetPianoMouseInput();
            if (PatternEditor.cursorPosition.Column == CursorColumnType.Note || WaveEditor.enabled || InstrumentEditor.enabled) {
                if (pianoInput != -1 && lastPianoKey != pianoInput) {
                    if (!Playback.isPlaying)
                        AudioEngine.ResetTicks();
                    ChannelManager.previewChannel.SetMacro(InstrumentBank.CurrentInstrumentIndex);
                    ChannelManager.previewChannel.TriggerNote(pianoInput);
                }
            }
            if (pianoInput == -1 && lastPianoKey != -1) {
                if (!Playback.isPlaying)
                    AudioEngine.ResetTicks();
                ChannelManager.previewChannel.PreviewCut();
            }

            if (!ChannelManager.previewChannel.waveEnv.toPlay.isActive)
                ChannelManager.previewChannel.SetWave(WaveBank.lastSelectedWave);

            Playback.Update(gameTime);


            if (!VisualizerMode) {
                songSettings.Update();
                frameView.Update();
                //frameRenderer.Update(gameTime);
                //FrameEditor.Update();
                editSettings.Update();
            }
            else {
                //sframeRenderer.UpdateChannelHeaders();
            }
            toolbar.Update();
            Dialogs.Update();
            base.Update(gameTime);
            lastPianoKey = pianoInput;
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

            Rendering.Graphics.batch = targetBatch;

            if (!VisualizerMode) {
                // draw frame editor
                //frameRenderer.DrawFrame(Song.currentSong.frames[FrameEditor.currentFrame], FrameEditor.cursorRow, FrameEditor.cursorColumn);

                PatternEditor.Draw();

                // draw instrument bank
                InstrumentBank.Draw();

                // draw wave bank
                waveBank.Draw();

                // draw edit settings
                editSettings.Draw();

                // draw frame view
                frameView.Draw();

                // draw song settings
                songSettings.Draw();

                //FrameEditor.channelScrollbar.Draw();
                //Rendering.Graphics.DrawRect(0, FrameEditor.channelScrollbar.y, FrameEditor.channelScrollbar.x, FrameEditor.channelScrollbar.height, new Color(223, 224, 232));


                // draw click position
                //Rendering.Graphics.DrawRect(Input.lastClickLocation.X, Input.lastClickLocation.Y, 1, 1, Color.Red);
                //Rendering.Graphics.DrawRect(Input.lastClickReleaseLocation.X, Input.lastClickReleaseLocation.Y, 1, 1, Color.DarkRed);
            }
            else {
                visualization.Draw();
            }
            toolbar.Draw();
            Dialogs.Draw();
         
            if (!VisualizerMode) {
                waveBank.editor.Draw();
                InstrumentBank.editor.Draw();

            }
            Tooltip.Draw();
            //int y = 10;
            //foreach (MMDevice k in audioEngine.devices)
            //{
            //    Rendering.Graphics.Write(k.DeviceFriendlyName, 2, y, Color.Red);
            //    y += 10;

            //}
            //Rendering.Graphics.Write("AudioStatus: " + audioEngine.wasapiOut.PlaybackState.ToString(), 2, 2, Color.Red);
            //Rendering.Graphics.Write("filename: " + filename, 2, 12, Color.Red);
            //Rendering.Graphics.Write("FPS: " + 1 / gameTime.ElapsedGameTime.TotalSeconds, 2, 2, Color.Red);
            targetBatch.End();



            //set rendering back to the back buffer
            GraphicsDevice.SetRenderTarget(null);
            //render target to back buffer
            targetBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
            //targetBatch.Draw(pixel, new Rectangle(0, 0, 1920, scrOffsetY), Color.White);
            //targetBatch.Draw(pixel, new Rectangle(0, 1080 + scrOffsetY, 1920, 90), Color.White);
            targetBatch.Draw(target, new Rectangle(0, 0, ScreenWidth * ScreenScale, ScreenHeight * ScreenScale), Color.White);
            if (VisualizerMode && Input.focus == null) {
                try {
                    visualization.DrawPiano(visualization.states);
                } catch {
                    //visualization.DrawPiano(visualization.statesPrev);
                }
                visualization.DrawOscilloscopes();
            }
            targetBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args) {
            // Do stuff here
            //SaveLoad.DoUnsavedCheck();
            Debug.WriteLine("Closing WaveTracker...");
            AudioEngine.instance.Stop();
            base.OnExiting(sender, args);
        }
    }
}