using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public interface IPageItem
    {
        string GetExtension();
        string GetPath();
        void Preload();
        void Unload();
    }
}
