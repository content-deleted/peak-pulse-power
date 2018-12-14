using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine
{
    public class EventTimer {
        public float ticks;
        public Action action;

        public EventTimer(Action a, float t) {
            action = a;
            ticks = t;
        }
    }

    public static class Time {
        public static GameTime gameTime;
        public static float ElapsedGameTime { get => (float)gameTime.ElapsedGameTime.TotalSeconds; }
        public static TimeSpan TotalGameTime { get => gameTime.TotalGameTime; }
        public static double TotalGameTimeMilli { get => TotalGameTime.TotalMilliseconds; }

        public static List<EventTimer> timers = new List<EventTimer>();
        
        public static void Initialize()
        {
            //gameTime = g;
            //ElapsedGameTime = 0;
            //TotalGameTime = new TimeSpan(0);
        }

        public static void Update(GameTime g)
        {
            //ElapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //TotalGameTime = gameTime.TotalGameTime;
            gameTime = g;

            // Update timers
            foreach (EventTimer timer in timers.ToList()) {
                timer.ticks -= ElapsedGameTime;
                if (timer.ticks <= 0) {
                    timer.action();
                    timers.Remove(timer);
                }
            }
            //timers.RemoveAll(x => x.ticks <= 0);
        }
    }

}
