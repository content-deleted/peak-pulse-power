using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine
{

    public class Transform
    {
        private Vector3 localPosition;
        private Quaternion localRotation;
        private Vector3 localScale;
        private Matrix world;

        private void UpdateWorld()
        {
            world = Matrix.CreateScale(localScale) * Matrix.CreateFromQuaternion(localRotation) * Matrix.CreateTranslation(localPosition);
            if (parent != null)  world *= parent.World;
            foreach (Transform child in Children) child.UpdateWorld();
        }

        public Matrix World { get => world; }

        public Vector3 LocalPosition
        {
            get => localPosition;
            set { localPosition = value; UpdateWorld(); }
        }

        public Vector3 LocalScale
        {
            get => localScale;
            set { localScale = value; UpdateWorld(); }
        }

        public Quaternion LocalRotation
        {
            get => localRotation;
            set { localRotation = value; UpdateWorld(); }
        }

        public Quaternion Rotation {
            get { return Quaternion.CreateFromRotationMatrix(World); }
            set {
                if (Parent == null) LocalRotation = value;
                else {
                    Vector3 scale, pos; Quaternion rot;
                    world.Decompose(out scale, out rot, out pos);

                    Matrix total = Matrix.CreateScale(scale) *
                          Matrix.CreateFromQuaternion(value) *
                          Matrix.CreateTranslation(pos);

                    LocalRotation = Quaternion.CreateFromRotationMatrix(
                         Matrix.Invert(Matrix.CreateScale(LocalScale)) * total *
                         Matrix.Invert(Matrix.CreateTranslation(LocalPosition)
                         * Parent.world));
                }
            }
        }


        public Transform()
        {
            localPosition = Vector3.Zero;
            localRotation = Quaternion.Identity;
            localScale = Vector3.One;
            UpdateWorld();
        }

        private Transform parent;

        public Transform Parent
        {
            get => parent;
            set
            {
                parent?.Children.Remove(this);
                parent = value;
                parent?.Children.Add(this);
                UpdateWorld();
            }
        }

        private List<Transform> children = new List<Transform>();

        private List<Transform> Children { get => children; }

        public void Rotate(Vector3 axis, float angle) => LocalRotation *= Quaternion.CreateFromAxisAngle(axis, angle);

        public Vector3 Position { get => World.Translation; }

        public Vector3 Forward { get => World.Forward; }

        public Vector3 Backward { get => World.Backward; }

        public Vector3 Up { get => World.Up; }

        public Vector3 Down { get => World.Down; }

        public Vector3 Left { get => World.Left; }

        public Vector3 Right { get => World.Right; }

        public void lookAt(Vector3 to) {
            Vector3 tmp = Vector3.Up;
            Vector3 forward = Vector3.Normalize(Position - to);
            Vector3 right = Vector3.Cross(Vector3.Normalize(tmp), forward);
            Vector3 up = Vector3.Cross(forward, right);

            Matrix camToWorld = new Matrix();

            camToWorld.M11 = right.X;
            camToWorld.M12 = right.Y;
            camToWorld.M13 = right.Z;
            camToWorld.M21 = up.X;
            camToWorld.M22 = up.Y;
            camToWorld.M23 = up.Z;
            camToWorld.M31 = forward.X;
            camToWorld.M32 = forward.Y;
            camToWorld.M33 = forward.Z;

            camToWorld.M41 = Position.X;
            camToWorld.M42 = Position.Y;
            camToWorld.M43 = Position.Z;

            world = camToWorld;
            localRotation = world.Rotation;
        }
    }
}
