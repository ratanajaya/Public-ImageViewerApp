using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public class VideoPage : IPageItem
    {
        string path;

        public VideoPage(string path) {
            this.path = path;
        }

        public string GetExtension() {
            throw new NotImplementedException();
        }

        public string GetPath() {
            throw new NotImplementedException();
        }

        public void Preload() {
            throw new NotImplementedException();
        }

        public void Unload() {
            throw new NotImplementedException();
        }
    }
}
