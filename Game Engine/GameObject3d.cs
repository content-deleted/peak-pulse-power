using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace CPI311.GameEngine
{
    public class GameObject3d : GameObject
    {
        public bool drawable = true;

        public static List<GameObject3d> activeGameObjects = new List<GameObject3d>();

        public new T GetBehavior<T>() where T : class => behaviors.Where(x => x is T).FirstOrDefault() as T;

        public Transform transform;

        public Model mesh;

        public Material material = new DefaultMaterial();

        public GameObject3d() => transform = new Transform();

        public static GameObject3d Initialize()
        {
            GameObject3d g = new GameObject3d();

            activeGameObjects.Add(g);
            return g;
        }

        public static void UpdateObjects() {
            lock (activeGameObjects) {
                foreach (GameObject3d gameObject in activeGameObjects.ToList()) gameObject.Update();

                foreach (GameObject3d gameObject in activeGameObjects.ToList()) gameObject.LateUpdate();
            }
        }

        override public void Start() {
            foreach (Behavior3d behavior in behaviors) behavior.Start();
        }
        override public void Update() {
            foreach (Behavior3d behavior in behaviors.ToList()) behavior.Update();
        }
        override public void LateUpdate() {
            foreach (Behavior3d behavior in behaviors.ToList()) behavior.LateUpdate();
        }

        override public void Render(dynamic Renderer) {
            if (!drawable) return;
            Tuple<Camera, GraphicsDevice> t = Renderer as Tuple<Camera, GraphicsDevice>;
            Camera c = t.Item1;
            GraphicsDevice g = t.Item2;
            g.Viewport = c.Viewport;
            material.Render(c, transform, mesh, g);
        }

        override public void Destroy()
        {
            foreach(Behavior3d behavior in behaviors) behavior.OnDestory();
            activeGameObjects.Remove(this);
        }
    }
}
