using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameStateManagement;
using Microsoft.Xna.Framework;
using CPI311.GameEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Final {
    class MainMenu : MenuScreen {
        List< GameObject3d> hoop = new List<GameObject3d>();
        Camera camera = new Camera();
        ContentManager content;

        public MainMenu() : base("") {
            MenuEntry startGame = new MenuEntry("Start");

            startGame.selectedColor = Color.WhiteSmoke;

            startGame.Selected += StartGameSelected;

            MenuEntries.Add(startGame);
        }

        void StartGameSelected(object sender, PlayerIndexEventArgs e) {
            //ScreenManager.AddScreen(new Gameplay(), null);
            ScreenManager.AddScreen(new SongSelect(), null);
            //ScreenManager.GetScreens().First().ExitScreen();
            //this.ExitScreen();
        }

        public void initDemo () {

            //background demo

            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            Vector3 pos = camera.Transform.Position;
            camera.Transform.LocalPosition = pos;

            Hoop.effect = content.Load<Effect>("hoop");

            for (int i = 0; i <= 10; i++) spawnHoop(20 * i);

            entryOffset = ScreenManager.GraphicsDevice.Viewport.Height / 2;
        }

        public void spawnHoop(int place) {
                hoop.Add (new GameObject3d());
                hoop.Last().transform.LocalPosition = camera.Transform.LocalPosition + camera.Transform.Forward * place;
                hoop.Last().material = new Hoop(5f, 7f, 1f, 10);
                hoop.Last().addBehavior(new hoopControl());
                hoop.Last().Start();
                hoop.Last().transform.Rotate(Vector3.Forward, 0.31f * place);
        }

        public override void Draw(GameTime gameTime) {
            if(IsExiting) ScreenManager.Game.Exit();
            camera.Transform.LocalPosition += camera.Transform.Forward;
            foreach (GameObject3d h in hoop) {
                if (camera.Transform.LocalPosition.Z < h.transform.LocalPosition.Z) h.transform.LocalPosition = new Vector3(0, 0, camera.Transform.LocalPosition.Z-200);
                h.Update();
                h.Render(Tuple.Create(camera, GameScreenManager.GraphicsDevice));
            }

            base.Draw(gameTime);
        }
    }
}
