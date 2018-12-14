using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine
{
    public static class InputManager
    {
        static KeyboardState PreviousKeyboardState { get; set; }
        static KeyboardState CurrentKeyboardState { get; set; }
        static MouseState PreviousMouseState { get; set; }
        static MouseState CurrentMouseState { get; set; }
        public static Vector2 GetMousePosition() => new Vector2(CurrentMouseState.X, CurrentMouseState.Y);


        public static void Initialize()
        {
            PreviousKeyboardState = CurrentKeyboardState = Keyboard.GetState();
            PreviousMouseState = CurrentMouseState =  Mouse.GetState();
        }

        public static void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
        }

        public static bool IsKeyDown(Keys key) => CurrentKeyboardState.IsKeyDown(key);

        public static bool IsKeyPressed(Keys key) => CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

        public static bool IsMouseDown() => CurrentMouseState.LeftButton == ButtonState.Pressed || CurrentMouseState.RightButton == ButtonState.Pressed || CurrentMouseState.MiddleButton == ButtonState.Pressed;

        public static bool IsMouseReleased() => CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;

        // public static bool IsMousePressed(MouseState m) => CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

    }
}
