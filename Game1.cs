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

namespace WaveTracker
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch targetBatch;
        public static Texture2D pixel;
        public static Texture2D channelHeaderSprite;

        public int ScreenWidth = 960;
        public int ScreenHeight = 540 - 24;
        public static int ScreenScale = 2;
        public static SpriteFont font;
        RenderTarget2D target;
        Rendering.FrameRenderer frameRenderer;
        Rendering.InstrumentBank instrumentBank;
        Rendering.WaveBank waveBank;
        Rendering.SongSettings songSettings;
        Rendering.EditSettings editSettings;
        Rendering.FrameView frameView;
        public static Tracker.Song currentSong;
        public Rendering.Toolbar toolbar;
        //public OuzoTracker.Forms.CreateInstrumentDialog control1;
        Audio.AudioEngine audioEngine;
        Audio.ChannelManager channelManager;
        int lastPianoKey;
        public static int previewChannel;
        public static Tracker.Song newSong;
        public static int pianoInput;
        public static int mouseCursorArrow;
        public static bool VisualizerMode;
        public static Rendering.Visualization visualization;
        string filename;



        public Game1(string[] args)
        {
            if (args.Length > 0)
                filename = args[0];
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferWidth = 1920;  // set this value to the desired width of your window
            //graphics.PreferredBackBufferHeight = 1080 - 72;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            Window.Position = new Point(-8, 0);
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Preferences.profile = PreferenceProfile.defaultProfile;
            Preferences.ReadFromFile();
            frameRenderer = new Rendering.FrameRenderer();
            frameView = new Rendering.FrameView();
            songSettings = new Rendering.SongSettings();
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;

        }

        protected override void Initialize()
        {

            Input.Intialize();
            frameRenderer.x = 0;
            frameRenderer.y = 151;
            currentSong = new Tracker.Song();
            newSong = currentSong.Clone();
            waveBank = new Rendering.WaveBank();
            instrumentBank = new Rendering.InstrumentBank();

            channelManager = new Audio.ChannelManager(Tracker.Song.CHANNEL_COUNT, waveBank);
            frameRenderer.Initialize(channelManager);
            FrameEditor.UnmuteAllChannels();
            FrameEditor.channelScrollbar = new UI.ScrollbarHorizontal(22, 323, 768, 7, null);
            FrameEditor.channelScrollbar.SetSize(Tracker.Song.CHANNEL_COUNT, 12);
            editSettings = new Rendering.EditSettings();
            visualization = new Rendering.Visualization(frameRenderer);
            this.IsFixedTimeStep = false;
            base.Initialize();

        }

        public static int bottomOfScreen;

        protected override void LoadContent()
        {
            Checkbox.textureSheet = Content.Load<Texture2D>("instrumentwindow");
            UI.NumberBox.buttons = Content.Load<Texture2D>("window_edit");
            Preferences.dialog = new Rendering.PreferencesDialog();
            editSettings.Initialize();
            font = Content.Load<SpriteFont>("custom_font");
            channelHeaderSprite = Content.Load<Texture2D>("trackerchannelheader");
            toolbar = new Rendering.Toolbar(Content.Load<Texture2D>("toolbar"));
            waveBank.editor = new Rendering.WaveEditor(Content.Load<Texture2D>("wave_window"));

            instrumentBank.Initialize(Content.Load<Texture2D>("toolbar"));
            instrumentBank.editor = new Rendering.InstrumentEditor(Content.Load<Texture2D>("instrumentwindow"));
            instrumentBank.editor.browser = new Rendering.SampleBrowser(Content.Load<Texture2D>("window_edit"));
            songSettings.Initialize(Content.Load<Texture2D>("window_edit"));
            frameView.Initialize(Content.Load<Texture2D>("toolbar"), GraphicsDevice);
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            // TODO: use this.Content to load your game content here
            targetBatch = new SpriteBatch(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, ScreenWidth, 600);
            audioEngine = new Audio.AudioEngine();
            audioEngine.Initialize(channelManager);
            SaveLoad.NewFile();
            SaveLoad.LoadFrom(filename);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.dialogOpenCooldown == 0)
            {
                int mouseX = Mouse.GetState().X;
                int mouseY = Mouse.GetState().Y;
                int width = Window.ClientBounds.Width - 2;
                int height = Window.ClientBounds.Height - 2;
                if (new Rectangle(1, 1, width, height).Contains(mouseX, mouseY))
                    if (mouseCursorArrow == 0)
                    {
                        Mouse.SetCursor(MouseCursor.Arrow);
                    }
                    else
                    {
                        Mouse.SetCursor(MouseCursor.SizeNS);
                        mouseCursorArrow--;
                    }
            }
            Window.Title = SaveLoad.fileName + (SaveLoad.isSaved ? "" : "*") + " - WaveTracker";
            ScreenScale = 2;
            bottomOfScreen = Window.ClientBounds.Height / 2;
            FrameEditor.channelScrollbar.y = bottomOfScreen - 14;
            if (Input.GetKeyDown(Keys.F12, KeyModifier.None))
            {
                channelManager.Reset();
                channelManager.ResetTicks(0);
                audioEngine.Reset();
            }

            Tooltip.Update(gameTime);
            if (IsActive)
            {
                Input.GetState(gameTime);
            }
            else
            {
                Input.focusTimer = 5;
                Input.dialogOpenCooldown = 3;
            }
            if (!VisualizerMode)
            {
                waveBank.Update();
                instrumentBank.Update();
            }
            if (Input.focus == null || Input.focus == waveBank.editor || Input.focus == instrumentBank.editor)
                pianoInput = Helpers.GetPianoInput(FrameEditor.currentOctave);
            else
            {
                pianoInput = -1;
            }
            waveBank.editor.Update();

            if (waveBank.editor.pianoInput() > -1)
                pianoInput = waveBank.editor.pianoInput();
            if (instrumentBank.editor.pianoInput() > -1)
                pianoInput = instrumentBank.editor.pianoInput();
            if (FrameEditor.currentColumn % 5 == 0 || Rendering.WaveEditor.enabled || Rendering.InstrumentEditor.enabled)
            {
                if (pianoInput != -1 && lastPianoKey != pianoInput)
                {
                    previewChannel = FrameEditor.currentColumn / 5;
                    channelManager.channels[previewChannel].SetMacro(Rendering.InstrumentBank.CurrentInstrumentIndex);
                    channelManager.channels[previewChannel].TriggerNote(pianoInput);
                    Audio.AudioEngine._tickCounter = 0;
                }
            }
            if (pianoInput == -1 && lastPianoKey != -1)
            {
                channelManager.channels[previewChannel].PreviewCut();
            }
            if (Rendering.WaveEditor.enabled)
            {
                if (!Audio.ChannelManager.instance.channels[previewChannel].waveEnv.toPlay.isActive)
                    Audio.ChannelManager.instance.channels[previewChannel].SetWave(Rendering.WaveBank.currentWave);
            }
            Tracker.Playback.Update(gameTime);

            #region octave change
            if (Input.GetKeyRepeat(Keys.OemOpenBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Divide, KeyModifier.None))
                FrameEditor.currentOctave--;
            if (Input.GetKeyRepeat(Keys.OemCloseBrackets, KeyModifier.None) || Input.GetKeyRepeat(Keys.Multiply, KeyModifier.None))
                FrameEditor.currentOctave++;

            if (FrameEditor.currentOctave < 0)
                FrameEditor.currentOctave = 0;
            if (FrameEditor.currentOctave > 9)
                FrameEditor.currentOctave = 9;

            #endregion

            if (!VisualizerMode)
            {
                songSettings.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                frameView.Update();
                frameRenderer.Update(gameTime);
                FrameEditor.Update();
                editSettings.Update();
            }
            else
            {
                frameRenderer.UpdateChannelHeaders();
            }
            audioEngine.exportingDialog.Update();
            Preferences.dialog.Update();
            toolbar.Update();
            base.Update(gameTime);
            lastPianoKey = pianoInput;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(target);


            GraphicsDevice.Clear(UIColors.black);

            // TODO: Add your drawing code here
            targetBatch.Begin(SpriteSortMode.Deferred, new BlendState
            {
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
            }, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            Rendering.Graphics.batch = targetBatch;

            if (!VisualizerMode)
            {
                // draw frame editor
                frameRenderer.DrawFrame(currentSong.frames[FrameEditor.currentFrame], FrameEditor.cursorRow, FrameEditor.cursorColumn);

                // draw instrument bank
                instrumentBank.Draw();

                // draw wave bank
                waveBank.Draw();

                // draw song settings
                songSettings.Draw();

                // draw edit settings
                editSettings.Draw();

                // draw frame view
                frameView.Draw();


                FrameEditor.channelScrollbar.Draw();
                Rendering.Graphics.DrawRect(0, FrameEditor.channelScrollbar.y, FrameEditor.channelScrollbar.x, FrameEditor.channelScrollbar.height, new Color(223, 224, 232));

                // draw click position
                //Rendering.Graphics.DrawRect(Input.lastClickLocation.X, Input.lastClickLocation.Y, 1, 1, Color.Red);
                //Rendering.Graphics.DrawRect(Input.lastClickReleaseLocation.X, Input.lastClickReleaseLocation.Y, 1, 1, Color.DarkRed);
            }
            else
            {
                visualization.Draw();
            }
            toolbar.Draw();
            Preferences.dialog.Draw();
            if (!VisualizerMode)
            {
                waveBank.editor.Draw();
                instrumentBank.editor.Draw();
                audioEngine.exportingDialog.Draw();

            }
            toolbar.exportDialog.Draw();
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
            targetBatch.Draw(target, new Rectangle(0, 0, ScreenWidth * ScreenScale, 1200), Color.White);
            if (VisualizerMode && Input.focus == null)
            {
                try
                {
                    visualization.DrawPiano(visualization.states);
                }
                catch
                {
                    //visualization.DrawPiano(visualization.statesPrev);
                }
                visualization.DrawOscilloscopes();
            }
            targetBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            // Do stuff here
            //SaveLoad.DoUnsavedCheck();
            Debug.WriteLine("Closing WaveTracker...");
            Audio.AudioEngine.instance.Stop();
            base.OnExiting(sender, args);
        }
    }
}