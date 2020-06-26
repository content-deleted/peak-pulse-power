using System.Collections.Generic;
using System.Linq;
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GameStateManagement;

namespace Final {
    class Main : Game {
        public GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        public Main() {
            Content.RootDirectory = "Content";

            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            CPI311.GameEngine.GameScreenManager.Initialize(graphics);
            CPI311.GameEngine.GameScreenManager.Setup(false, 1920, 1080);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 720;
            

            CPI311.GameEngine.Time.Initialize();
            CPI311.GameEngine.InputManager.Initialize();

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            screenManager.Initialize();
            Components.Add(screenManager);
            
            //screenManager.AddScreen(new demoScreen(), null);
            screenManager.AddScreen(new MainMenu(), null);

            (screenManager.GetScreens().First() as MainMenu).initDemo();
        }

        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e) {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        }
    }
}
