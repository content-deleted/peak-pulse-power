using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CPI311.GameEngine {
    public class Collider : Behavior3d {
        public static List<Collider> colliders = new List<Collider>();

        public Rigidbody rigidbody;
        
        public bool collidedThisFrame = false;


        public override void  Start() {
            colliders.Add(this);
            rigidbody = obj.GetBehavior<Rigidbody>() as Rigidbody;
        }

        public virtual bool Collides(Collider other, out Vector3 normal) {
            normal = Vector3.Zero;
            return false;
        }

        public override void OnDestory() {
            base.OnDestory();
            rigidbody = null;
            colliders.Remove(this);
        }

        public static int numberCollisions;

        public static void Update(GameTime gameTime) {
            Vector3 normal;

            // shitty way of refreshing this value
            // please reconsider this in the future
            foreach (Collider c in colliders) c.collidedThisFrame = false;

            // Note this is problematic because it computes collisions twice
            foreach (Collider outer in colliders) {
                foreach (Collider inner in colliders) {
                    if (inner == outer) continue;
                    if (outer.Collides(inner, out normal)) {
                        inner.collidedThisFrame = true;
                        numberCollisions++;
                        if (inner.rigidbody != null && outer.rigidbody != null) {

                            // Fuck this
                            /*
                            Vector3 velocityNormal = Vector3.Dot(normal, inner.rigidbody.Velocity - outer.rigidbody.Velocity) * 2
                                                    * normal * inner.rigidbody.Mass * outer.rigidbody.Mass;

                            // problematic multi collisions
                            inner.rigidbody.Impulse += -velocityNormal / 2;
                            outer.rigidbody.Impulse += velocityNormal / 2; */


                            inner.rigidbody.Velocity = -inner.rigidbody.Velocity.Length() * normal;
                            outer.rigidbody.Velocity = outer.rigidbody.Velocity.Length() * normal;

                        }
                        else {
                            if (inner.rigidbody != null || outer.rigidbody != null) {
                                Rigidbody rb = (inner.rigidbody == null) ? outer.rigidbody : inner.rigidbody;
                                if (Vector3.Dot(normal, rb.Velocity) < 0) rb.Impulse = Vector3.Dot(normal, rb.Velocity) * -2 * normal;
                            }
                        }
                    }
                }
            } 
        }

        public virtual float? Intersects(Ray ray) { return null; }
    }
}
