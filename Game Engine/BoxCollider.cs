using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CPI311.GameEngine {
    public class BoxCollider : Collider {
        public float Size { get; set; }

        private static Vector3[] normals = { Vector3.Up, Vector3.Down, Vector3.Right, Vector3.Left,Vector3.Forward, Vector3.Backward,};

        private static Vector3[] vertices = 
            { new Vector3(-1,-1,1), new Vector3(1,-1,1),
              new Vector3(1,-1,-1), new Vector3(-1,-1,-1),
              new Vector3(-1,1,1), new Vector3(1,1,1),
              new Vector3(1,1,-1), new Vector3(-1,1,-1), };

        private static int[] indices = {
            0,1,2,  0,2,3, // Down: using vertices[0][1][2] , [0][2][3] ...
            5,4,7,  5,7,6, // Up
            4,0,3,  4,3,7, // Left
            1,5,6,  1,6,2, // Right
            4,5,1,  4,1,0, // Front
            3,2,6,  3,6,7, }; // Back

        public override bool Collides(Collider other, out Vector3 normal) {

            if (other is SphereCollider) {
                SphereCollider collider = other as SphereCollider;
                normal = Vector3.Zero; // no collision
                bool isColliding = false;

                for (int i = 0; i < 6; i++) {
                    for (int j = 0; j < 2; j++) {
                        int baseIndex = i * 6 + j * 3;
                        Vector3 a = vertices[indices[baseIndex]] * Size;
                        Vector3 b = vertices[indices[baseIndex + 1]] * Size;
                        Vector3 c = vertices[indices[baseIndex + 2]] * Size;
                        Vector3 n = normals[i];
                        Vector3 aPrime = collider.transform.Position - a;
                        float d = Math.Abs(Vector3.Dot(aPrime,n));// calculate the distance to the plane 

                        if (d < collider.Radius) {
                            Vector3 pNormal = Vector3.Dot(aPrime, n) * n;
                            Vector3 pTangent = aPrime - pNormal;
                            Vector3 pointOnPlane = pTangent + a;  // calculate the closest point
                                                                  // Compute vectors        
                            Vector3 v0 = c - a;
                            Vector3 v1 = b - a;
                            Vector3 v2 = pointOnPlane - a;

                            // Compute dot products
                            float dot00 = Vector3.Dot(v0, v0);
                            float dot01 = Vector3.Dot(v0, v1);
                            float dot02 = Vector3.Dot(v0, v2);
                            float dot11 = Vector3.Dot(v1, v1);
                            float dot12 = Vector3.Dot(v1, v2);

                            // Compute barycentric coordinates
                            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
                            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                            // Check if point is in triangle
                            if((u >= 0) && (v >= 0) && (u + v < 1)) { 
                                normal += n;
                                j = 1; // skip second triangle, if necessary
                                if (i % 2 == 0) i += 1; // skip opposite side if necessary
                                isColliding = true;
                            }
                        } 
                    }
                }
                normal.Normalize();
                return isColliding;
            }
            return base.Collides(other, out normal);
        }

        public override float? Intersects(Ray ray) {
            Matrix worldInv = Matrix.Invert(transform.World);
            ray.Position = Vector3.Transform(ray.Position, worldInv);
            ray.Direction = Vector3.TransformNormal(ray.Direction, worldInv);

            return new BoundingBox(-Vector3.One * Size, Vector3.One * Size).Intersects(ray);
        }
    }
}
