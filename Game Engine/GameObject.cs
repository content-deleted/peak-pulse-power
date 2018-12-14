using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI311.GameEngine
{
    public abstract class GameObject
    {
        public static bool gameStarted = false;
        //public static List<GameObject> activeGameObjects = new List<GameObject>();

        protected List<Behavior> behaviors = new List<Behavior>();

        public void releaseBehavior(Behavior b) => behaviors.Remove(b);

        public void addBehavior (Behavior b)
        {
            behaviors.Add(b);
            b.assign(this);
        }

        public Behavior GetBehavior<T>() => behaviors. Where(x => x is T).FirstOrDefault();

        public bool HasBehavior<T>() => behaviors.Where(x => x is T).Any();

        //public  static  GameObject Initialize();

        public virtual void Start() {
            foreach (Behavior behavior in behaviors) behavior.Start();
        }
        public virtual void Update() {
            foreach (Behavior behavior in behaviors) behavior.Update();
        }
        public virtual void LateUpdate() {
            foreach (Behavior behavior in behaviors) behavior.LateUpdate();
        }
        public abstract void Render(dynamic Renderer);

        public abstract void Destroy();
    }
}
