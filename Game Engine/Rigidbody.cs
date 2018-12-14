using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CPI311.GameEngine {
    public class Rigidbody : Behavior3d {
        public Vector3 Velocity = Vector3.Zero;
        public float Mass = 1;
        public Vector3 Acceleration = Vector3.Zero;
        public Vector3 Impulse = Vector3.Zero;

        public override void Update() {
            Velocity += Acceleration * Time.ElapsedGameTime + Impulse / Mass;
            
            transform.LocalPosition += Velocity * Time.ElapsedGameTime;
            Impulse = Vector3.Zero;
        }
    }

}
