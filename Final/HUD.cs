using CPI311.GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final {
    static class HUD {
        public class ScoreItem {
            public int amount;
            public string message;
            public int time;
            public string getMsg() => message + " (" + amount + ")";
        }

        public static List<ScoreItem> scoreIndicators;

        public static void Draw(GameTime gameTime, SpriteFont font, int Score) {
            SpriteBatch spriteBatch = new SpriteBatch(GameScreenManager.GraphicsDevice);

            spriteBatch.Begin();

            // Draw the overall score in the corner
            string score = Score.ToString();
            Vector2 scorePos = new Vector2(GameScreenManager.Width * 0.05f, GameScreenManager.Height * 0.9f);
            spriteBatch.DrawString(font, score, scorePos, Color.White);
            Vector2 styleOffset = new Vector2(2, 2);
            spriteBatch.DrawString(font, score, scorePos + styleOffset, Color.Black);

            // Draw the itemized list of score indicators
            for(int i = 0; i < scoreIndicators.Count(); i++) {
                var item = scoreIndicators[i];
                Vector2 itemPos = new Vector2(GameScreenManager.Width * 0.9f, GameScreenManager.Height * 0.2f + i * 0.05f);
                Color transparancy = new Color(Color.White, 1 - (item.time / (2 * scoreItemLifeTime)));
                spriteBatch.DrawString(font, item.getMsg(), itemPos, transparancy);
            }
            spriteBatch.End();
        }

        const int scoreItemLifeTime = 30;

        public static void UpdateHud() {
            scoreIndicators.ForEach(s => s.time++);
            scoreIndicators.RemoveAll(s => s.time >= 30);

        }
    }
}
