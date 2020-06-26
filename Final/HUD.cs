using CPI311.GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final {
    class HUD {
        public static void Draw(GameTime gameTime, SpriteFont font, int Score) {
            SpriteBatch spriteBatch = new SpriteBatch(GameScreenManager.GraphicsDevice);

            spriteBatch.Begin();

            String test = Score.ToString();
            Vector2 scorePos = new Vector2(GameScreenManager.Width * 0.05f, GameScreenManager.Height * 0.9f);
            spriteBatch.DrawString(font, test, scorePos, Color.White);
            Vector2 styleOffset = new Vector2(2, 2);
            spriteBatch.DrawString(font, test, scorePos + styleOffset, Color.Black);
            spriteBatch.End();
        }
    }
}
