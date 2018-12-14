using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine {
    public static class GameScreenManager {
        public static GraphicsDeviceManager graphics;

        private static bool IsFullScreen {
            get => graphics.IsFullScreen;
            set => graphics.IsFullScreen = value;
        }

        public static int Width {
            get => GraphicsDevice.PresentationParameters.BackBufferWidth;
            set {
                graphics.PreferredBackBufferWidth = value;
                graphics.ApplyChanges();
            }
        }

        public static int Height {
            get => GraphicsDevice.PresentationParameters.BackBufferHeight;
            set {
                graphics.PreferredBackBufferHeight = value;
                graphics.ApplyChanges();
            }
        }

        public static GraphicsDevice GraphicsDevice {
            get { return graphics.GraphicsDevice; }
        }

        public static void Initialize(GraphicsDeviceManager g) {
            GameScreenManager.graphics = g;
        }
        public static Viewport DefaultViewport {
            get { return new Viewport(0, 0, Width, Height);  }
        }

        public static void Setup(int width = 0, int height = 0) {
            Setup(IsFullScreen, width, height);
        }

        public static void Setup(bool fullScreen, int width = 0, int height = 0) {
            if (width > 0)   graphics.PreferredBackBufferWidth = width;
            if (height > 0)  graphics.PreferredBackBufferHeight = height;
            IsFullScreen = fullScreen;
            graphics.ApplyChanges();
        }
    }
}
