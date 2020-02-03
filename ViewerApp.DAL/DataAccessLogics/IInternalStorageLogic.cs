using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.DAL
{
    public interface IInternalStorageLogic
    {
        void Initialize();
        void SaveStringifiedDb(string stringifiedDb);
        string GetStringifiedDb();
    }
}
