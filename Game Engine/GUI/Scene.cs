using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI311.GameEngine.GUI {
    public class Scene {
        public Action Update;
        public Action Draw;
        public Scene(Action update, Action draw) { Update = update; Draw = draw; }



    }

}
