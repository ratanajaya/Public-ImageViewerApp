using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewerApp
{
    class SceneManager
    {
        List<string> scenes;

        public SceneManager() {
            scenes = new List<string>();
        }

        public string Pop() {
            if(scenes.Count == 1) {
                return "";
            }
            scenes.RemoveAt(scenes.Count - 1);
            string result = scenes.Last();
            return result;
        }

        public string Push(string scene) {
            scenes.Add(scene);
            return scene;
        }
    }
}