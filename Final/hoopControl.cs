using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CPI311.GameEngine;
using Microsoft.Xna.Framework;

namespace Final {
    public class hoopControl : Behavior3d {
        float r = 0;
        public float rate = 0.1f;
        public float rotationRate = 0.1f;

        Hoop hoop;

        public override void Start() {
            base.Start();
            hoop = (((obj) as GameObject3d).material as Hoop);
        }

        public override void Update() {
            base.Update();
            r += rate;
            hoop.rotation = r;
            transform.Rotate(Vector3.Forward, rotationRate);
        }

        public override void OnDestory() {
            hoop = null;
            base.OnDestory();
        }
    }
    
    public class hoopLogic : Behavior3d {
        public static Camera player;
        public const float lowMaxSpeed = 1.75f;
        public static float MaxSpeed = lowMaxSpeed;
        public static void cameraSpeedCoroutineInc () {
            if (player.FieldOfView < MaxSpeed) {
                player.FieldOfView *= 1.05f;
                Time.timers.Add(new EventTimer(cameraSpeedCoroutineInc, 0));
            }
            else Time.timers.Add(new EventTimer(cameraSpeedCoroutineDec, 1));
        }
        public static void cameraSpeedCoroutineDec() {
            if (player.FieldOfView >= 1.1f) {
                player.FieldOfView -= 0.02f;
                Time.timers.Add(new EventTimer(cameraSpeedCoroutineDec, 0));
            }
            else
                player.FieldOfView = 1;
        }
        bool active = false;
        public override void Update() {
            base.Update();

            // first check if we're close
            if (transform.LocalPosition.Z < player.Transform.LocalPosition.Z + 1) {

                // if we're through then activate 
                if(!active && (int)transform.LocalPosition.Z == (int)player.Transform.LocalPosition.Z && Vector3.Distance(transform.LocalPosition, player.Transform.LocalPosition) < 5) {
                    MaxSpeed += 0.1f;
                    
                    active = true;
                    Time.timers.Add(new EventTimer(cameraSpeedCoroutineInc, 0) );
                }
            
                // if we're behind then destroy
                if (transform.LocalPosition.Z < player.Transform.LocalPosition.Z - 5) {
                    // If the player missed the hoop
                    if (!active) {
                        //MaxSpeed = lowMaxSpeed;
                        Time.timers.Add(new EventTimer(cameraSpeedCoroutineDec, 0));
                    }

                    // Either way
                    (obj as hoopObject).Destroy();
                }
            }
        }
        public override void OnDestory() {
            active = false;
            base.OnDestory();
        }
    }
}
