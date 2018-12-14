using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CPI311.GameEngine {
    public class SphereCollider : Collider {
        public float Radius = 1f;

        public override bool Collides(Collider other, out Vector3 normal) {
            if (other is SphereCollider) {
                SphereCollider collider = other as SphereCollider;
                float dist = (transform.Position - collider.transform.Position).LengthSquared();
                double pow = Math.Pow(Radius + collider.Radius, 2);
                if ( dist < pow ) {
                    normal = Vector3.Normalize(transform.Position - collider.transform.Position);
                    normal.Normalize();
                    return true;
                }
            }
            else if (other is BoxCollider) return other.Collides(this, out normal);

            return base.Collides(other, out normal);
        }

        public override float? Intersects(Ray ray) {
            Matrix worldInv = Matrix.Invert(transform.World);

            ray.Position = Vector3.Transform(ray.Position, worldInv);
            ray.Direction = Vector3.TransformNormal(ray.Direction, worldInv);
            float length = ray.Direction.Length();
            ray.Direction /= length; // same as normalization

            float? p = new BoundingSphere(Vector3.Zero, Radius).Intersects(ray);

            if (p != null) return (float)p * length;
            return null;
        }

    }
}