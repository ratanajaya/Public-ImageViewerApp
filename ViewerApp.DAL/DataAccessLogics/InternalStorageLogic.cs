using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.DAL
{
    public class InternalStorageLogic : IInternalStorageLogic
    {
        IFileSystemAccess fa;
        string storagePath;
        
        public InternalStorageLogic(IFileSystemAccess fa) {
            this.fa = fa;
        }

        public void Initialize() {
            storagePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }

        public void SaveStringifiedDb(string stringifiedDb) {
            fa.WriteAllText(Path.Combine(storagePath, "_libraryDatabase.json"), stringifiedDb);
        }

        public string GetStringifiedDb() {
            var result = fa.ReadText(Path.Combine(storagePath, "_libraryDatabase.json"));
            return result;
        }
    }
}
