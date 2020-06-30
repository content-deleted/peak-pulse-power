using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI311.GameEngine;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Media;
using NAudio;
using NAudio.Wave;
using NAudio.Utils;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace Final {

    public class Gameplay : GameScreen {
        const int terrainSize = 300; // This controls the overall size of (displayed) terrain including culling

        PlayerIndex ControllingPlayer;
        SpriteBatch spriteBatch;
        public RenderTarget2D renderTarget;
        Effect postProcess;

        public static Camera camera = new Camera();
        Song staticNoise;

        Effect virtualTerrain;
        CustomTerrainRenderer terrainRenderer;
        GameObject3d terrainObject;

        ContentManager content;

        Mp3FileReader reader;
        WaveOut waveOut;
        List<Mp3Frame> mp3Frames = new List<Mp3Frame>();
        Texture2D test;
        Texture2D background;

        SpriteFont HUDfont;

        byte[] avgE;

        public Gameplay(SongSelect.songInfo s, PlayerIndex controllerIndex) {
            ControllingPlayer = controllerIndex;
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0);

            //SONG LOADING AFTER THIS
            reader = new Mp3FileReader(s.songPath); //reader = new AudioFileReader(s.songPath);

            waveOut = new WaveOut();

            var totalLength = reader.Length;
            Mp3Frame b = reader.ReadNextFrame();

            while (b != null) {
                mp3Frames.Add(b);
                b = reader.ReadNextFrame();
            }

            var MaxFrameLength = mp3Frames.Max(f => f.FrameLength);

            test = new Texture2D(GameScreenManager.GraphicsDevice, MaxFrameLength, mp3Frames.Count());

            int index = 0;

            uint[] totalData = new uint[MaxFrameLength * mp3Frames.Count()];

            avgE = new byte[mp3Frames.Count()];
            int i=0;
            byte lastInput = 0;
            foreach (Mp3Frame frame in mp3Frames) {
                //FastFourierTransform.HammingWindow(0, )
                //FastFourierTransform.FFT(true, 2, )
                long sum = frame.RawData.Sum(x=> (uint) x);
                
                byte a = (byte)( ((sum / frame.RawData.Length) + lastInput) / 2);
                avgE[i++] = a;
                lastInput = a;

                Array.Copy(frame.RawData, 0, totalData, index, frame.RawData.Length);
                //totalData[index] 
                //test.SetData(frame.RawData, index, frame.RawData.Length);
                index += MaxFrameLength;
            }
            test.SetData(totalData);

            CustomTerrainRenderer.song = test;

            reader.Seek(0, System.IO.SeekOrigin.Begin);

            waveOut.Init(reader);
            
            //Time.timers.Add(new EventTimer(incCountDown, 2));

            //FastFourierTransform.
        }
        /*
        public int countdown = 100;
        public void incCountDown() {
            countdown--;
            terrainRenderer.heightAlter = (1) / (countdown+1);
            if (countdown <= 0) {
                vert = terrainRenderer.lastRowDepth;
                Time.timers.Add(new EventTimer(PlayEvent, 0.5f));
            }
            else Time.timers.Add(new EventTimer(incCountDown, 0.001f));
        }
        public int vert;

        public void PlayEvent() {
            if(vert == terrainRenderer.lastRowDepth % 200) waveOut.Play();
            else Time.timers.Add(new EventTimer(PlayEvent, 0.0001f));
        }*/

        public void newHoop() {
            hoopObject.Initialize();
            Time.timers.Add(new EventTimer(newHoop, 5f));
        }

        public override void LoadContent() {

            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GameScreenManager.GraphicsDevice);
            background = content.Load<Texture2D>("back");

            // Load the hudfont
            HUDfont = content.Load<SpriteFont>("menufont");

            // Setting up render target
            PresentationParameters pp = ScreenManager.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Color, DepthFormat.Depth24);
            postProcess = content.Load<Effect>("PostProcess");
            postProcess.Parameters["darkenFactor"].SetValue(1f);

            virtualTerrain = content.Load<Effect>("virtualTerrain");
            CustomTerrainRenderer.wire = content.Load<Texture2D>("wire");
            CustomTerrainRenderer.effect = virtualTerrain;

            terrainRenderer = new CustomTerrainRenderer(Vector2.One*terrainSize);

            terrainObject = GameObject3d.Initialize();
            terrainRenderer.obj = terrainObject;

            terrainObject.material = terrainRenderer;

            //SET CAM
            camera = new Camera();
            camera.Transform.LocalPosition += new Vector3(terrainSize/2, 0, 10);
            camera.Transform.Rotate(Vector3.Up, (float)Math.PI);
            Vector3 pos = camera.Transform.Position;
            pos.Y = 2 + terrainRenderer.GetAltitude(camera.Transform.Position);
            camera.Transform.LocalPosition = pos;
            camera.NearPlane = 0.001f;

            // Setup skybox
            camera.skybox = new Skybox { skyboxModel = content.Load<Model>("Box"), skyboxTexture = background };
            Skybox.shader = content.Load<Effect>("skybox");

            //SET HOOP
            hoopLogic.player = camera;
            hoopObject.lastPos = camera.Transform.LocalPosition + new Vector3(0, 2, 100);
            hoopObject.Initialize();

            //sfx 
            staticNoise = content.Load<Song>("static");
            MediaPlayer.Volume = 0.2f;

            foreach (GameObject3d gameObject in GameObject3d.activeGameObjects) gameObject.Start();
            GameObject.gameStarted = true;
        }

        public bool songStarted = false;

        
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {

            InputManager.Update();
            Time.Update(gameTime);

            GameObject3d.UpdateObjects();

            while (terrainRenderer.lastRowDepth < camera.Transform.Position.Z - 5) {
                terrainRenderer.updateNormals(Math.Abs(terrainRenderer.lastRowDepth - 5), Vector3.Up);
                terrainRenderer.updateDepth(terrainRenderer.lastRowDepth);
                terrainRenderer.updateNormals(terrainRenderer.lastRowDepth + 1, Vector3.Down);
            }

            terrainRenderer.songPos = (float)waveOut.GetPosition() / (float)reader.Length;
            if (terrainRenderer.songPos < 1) terrainRenderer.avgE = 1 / (float)(avgE[(int)(avgE.Length*terrainRenderer.songPos)] - 100);
            
            if (!songStarted && terrainRenderer.lastRowDepth > 100) {
                songStarted = true;
                newHoop();
                waveOut.Play();
            }
            
            if (InputManager.IsKeyPressed(Keys.T)) postToggle = !postToggle;
            if (InputManager.IsKeyPressed(Keys.N)) noisyToggle = !noisyToggle;
            if (InputManager.IsKeyPressed(Keys.F)) GameScreenManager.graphics.ToggleFullScreen();

            if (InputManager.IsKeyDown(Keys.Up)) camera.FieldOfView += 0.01f;
            if (InputManager.IsKeyDown(Keys.Down)) camera.FieldOfView -= 0.01f;

            if (!noisyToggle) updateCam();

            checkSongEnding();
            //pos.Y += terrainRenderer.avgE;
            //terrainRenderer.totalFrames = mp3Frames.Count();
        }

        public void checkSongEnding () {
            // Fade out
            float timeRemaining = reader.Length - waveOut.GetPosition();
            if (timeRemaining <= 0) {
                foreach(GameObject3d g in GameObject3d.activeGameObjects.ToList()) {
                    g.Destroy();
                }
                ScreenManager.FadeBackBufferToBlack(0);
                Time.timers.Clear();
                ExitScreen();
            }
        }
        public void ExitGameplay() {
            waveOut.Stop();
            foreach (GameObject3d g in GameObject3d.activeGameObjects.ToList()) {
                g.Destroy();
            }
            ScreenManager.FadeBackBufferToBlack(0);
            Time.timers.Clear();
            ExitScreen();
        }

        public void cameraRestore() {
            Vector3 cameraPosition = new Vector3(100,0 , camera.Transform.LocalPosition.Z);
            cameraPosition.Y = 5 + terrainRenderer.GetAltitude(cameraPosition);
            camera.Transform.LocalPosition = cameraPosition;

            noisyToggle = false;

            // Reset speed effects
            camera.FieldOfView = 1;
            hoopLogic.MaxFOV = hoopLogic.lowMaxSpeed; 
            cameraCurrectVelocity = Vector3.Zero;
            curSpeed = camForwardSpeed;
            curUpDown = 0;
            curLeftRight = 0;

            // Start song again
            MediaPlayer.Stop();
            waveOut.Play();
        }

        // Camera update function because Im lazy
        public Vector3 cameraCurrectVelocity;
        public const float camForwardSpeed = 15;
        public const float leftRightSpeed = 0.065f;
        public const float upDownSpeed = 0.04f;

        public float curSpeed = camForwardSpeed;
        public float curUpDown = 0;
        public float curLeftRight = 0;

       
        
        public void updateCam() {
            if (songStarted && t > 2) {
                var height = terrainRenderer.GetAltitude(camera.Transform.Position);
                if (0.5f + height > camera.Transform.Position.Y) {
                    // camera crashes
                    noisyToggle = true;
                    waveOut.Pause();
                    MediaPlayer.Play(staticNoise);
                    Time.timers.Add(new EventTimer(cameraRestore, 2));
                }
                else if (2 + height > camera.Transform.Position.Y) {
                    // graze
                    HUD.grazeCount++;
                    HUD.grazeTimer = 0;

                } else {
                    HUD.grazeTimer++;
                }
            }
            else
                if (camera.Transform.LocalPosition.Y < 22.5f) cameraCurrectVelocity.Y += 0.025f;

            // Input
            if ((int)ControllingPlayer < 5) {
                GamePadState state = GamePad.GetState(ControllingPlayer);
                if (state.IsButtonDown(Buttons.Back)) ExitGameplay();
                curLeftRight += Time.ElapsedGameTime * leftRightSpeed * -state.ThumbSticks.Left.X;
                curUpDown += Time.ElapsedGameTime * upDownSpeed * -state.ThumbSticks.Left.Y;
            }
            else {
                if(InputManager.IsKeyPressed(Keys.Escape)) ExitGameplay();

                if (InputManager.IsKeyDown(Keys.A)) curLeftRight += Time.ElapsedGameTime * leftRightSpeed;
                if (InputManager.IsKeyDown(Keys.D)) curLeftRight -= Time.ElapsedGameTime * leftRightSpeed;

                if (InputManager.IsKeyDown(Keys.W)) curUpDown -= Time.ElapsedGameTime * upDownSpeed;
                if (InputManager.IsKeyDown(Keys.S)) curUpDown += Time.ElapsedGameTime * upDownSpeed;
            }

            cameraCurrectVelocity.X += curLeftRight;
            cameraCurrectVelocity.Y += curUpDown;

            cameraCurrectVelocity.Y = MathHelper.Clamp( cameraCurrectVelocity.Y, -0.12f, 0.12f);
            cameraCurrectVelocity.X = MathHelper.Clamp(cameraCurrectVelocity.X, -0.2f, 0.2f);
            cameraCurrectVelocity.Z = (curSpeed) * Time.ElapsedGameTime + cameraCurrectVelocity.Y / 5;
            
            camera.Transform.lookAt(camera.Transform.LocalPosition + cameraCurrectVelocity);
            camera.Transform.LocalPosition += cameraCurrectVelocity;
            camera.Transform.Rotate(camera.Transform.Forward, 2*cameraCurrectVelocity.X);

            curLeftRight /= 1.2f;
            curUpDown /= 1.2f;
        }

        bool postToggle=true;
        bool noisyToggle = false;
        float t;

        public override void Draw(GameTime gameTime) {
            // Set our graphics device to draw to a texture
            ScreenManager.GraphicsDevice.SetRenderTarget(renderTarget);
            ScreenManager.GraphicsDevice.Clear(Color.Purple);

            camera.drawSkybox(GameScreenManager.GraphicsDevice);

            foreach (GameObject3d gameObject in GameObject3d.activeGameObjects.ToList())
                gameObject.Render(Tuple.Create(camera, GameScreenManager.GraphicsDevice));

            // Make sure to clear the graphics device render target
            ScreenManager.GraphicsDevice.SetRenderTarget(null);

            // Handle post processing effects
            using (SpriteBatch sprite = new SpriteBatch(ScreenManager.GraphicsDevice)) {

                postProcess.Parameters["toggle"].SetValue(postToggle);
                postProcess.Parameters["noisy"].SetValue(noisyToggle);
                if (songStarted) postProcess.Parameters["time"].SetValue(t += 0.02f);

                double timeRemainingSeconds = reader.Length - waveOut.GetPosition();
                if (timeRemainingSeconds < 500000) {
                    postProcess.Parameters["darkenFactor"].SetValue((float)timeRemainingSeconds / 500000);
                }

                sprite.Begin(SpriteSortMode.Deferred, null, null, null, null, postProcess);

                sprite.Draw(renderTarget, new Rectangle(ScreenManager.GraphicsDevice.Viewport.X, ScreenManager.GraphicsDevice.Viewport.Y, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);
                sprite.End();
            }

            // Draw the Hud over everything else
            HUD.Draw(gameTime, HUDfont);

            //THIS IS THE TESTING CODE FOR DRAWING SOUND
            /*
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(test, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
            */
            //test, new Rectangle(500, 500, 250, 1000),null, Color.White, 3.141f,Vector2.One*500,SpriteEffects.None,0.5f);
        }
    }
}
