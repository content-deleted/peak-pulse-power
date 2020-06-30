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

            public ScoreItem(string Message, int Amount) {
                amount = Amount;
                message = Message;
                time = 0;

                totalScore += amount;
            }
        }

        // Scoring stuff oh god this is a fucking mess
        public static int grazeCount; // Total frames youve been grazing
        public const int grazeLoss = 30; // Frames until you lose graze
        public static int grazeTimer = grazeLoss; // Frames since you last grazed

        public static int totalScore = 0;

        public static List<ScoreItem> scoreIndicators = new List<ScoreItem>();

        public static void Draw(GameTime gameTime, SpriteFont font) {
            // Change position of stuff to draw 
            UpdateHud();
    
            UpdateGraze();
            SpriteBatch spriteBatch = new SpriteBatch(GameScreenManager.GraphicsDevice);

            spriteBatch.Begin();

            // Draw the overall score in the corner
            string score = totalScore.ToString();
            Vector2 scorePos = new Vector2(GameScreenManager.Width * 0.05f, GameScreenManager.Height * 0.9f);
            spriteBatch.DrawString(font, score, scorePos, Color.White);
            Vector2 styleOffset = new Vector2(2, 2);
            spriteBatch.DrawString(font, score, scorePos + styleOffset, Color.Black);

            // Draw the itemized list of score indicators
            for(int i = 0; i < scoreIndicators.Count(); i++) {
                var item = scoreIndicators[i];
                Vector2 itemPos = new Vector2(GameScreenManager.Width * 0.8f, GameScreenManager.Height * (0.3f + i * 0.1f) );
                Color transparancy = new Color(Color.White, 1 - (item.time / (2 * scoreItemLifeTime)));
                spriteBatch.DrawString(font, item.getMsg(), itemPos, transparancy);
            }

            // Draw graze
            if (grazeTimer < grazeLoss) {
                Vector2 grazePos = new Vector2(GameScreenManager.Width * 0.75f, GameScreenManager.Height * 0.12f);
                float grazeRotation = 0.5f * (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * 2);
                float grazeScale = 0.5f + ((grazeLoss - grazeTimer) / grazeLoss) * 1.5f + 0.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2);
                spriteBatch.DrawString(spriteFont: font, text: "GRAZE ~ " + grazeCount + " ~", position: grazePos, color: Color.White, rotation: grazeRotation, origin: new Vector2(60, 0), scale: grazeScale, effects: SpriteEffects.None, layerDepth: 0);
            }
            spriteBatch.End();
        }

        const int scoreItemLifeTime = 60;

        public static void UpdateHud() {
            scoreIndicators.ForEach(s => s.time++);
            scoreIndicators.RemoveAll(s => s.time >= scoreItemLifeTime);
        }

        public static void UpdateGraze() {
            // End graze
            if (grazeTimer >= grazeLoss && grazeCount != 0) {
                scoreIndicators.Add(new ScoreItem("Rad Graze: ", grazeCount * 10));
                grazeCount = 0;
            }
        }
    }
}
