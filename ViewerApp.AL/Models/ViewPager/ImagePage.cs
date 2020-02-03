using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public class ImagePage : IPageItem
    {
        string path;

        public ImagePage(string path) {
            this.path = path;
        }

        public string GetExtension() {
            return System.IO.Path.GetExtension(path);
        }

        public string GetPath() {
            return path;
        }

        public void Preload() {
            
        }

        public void Unload() {
            
        }
    }
}
