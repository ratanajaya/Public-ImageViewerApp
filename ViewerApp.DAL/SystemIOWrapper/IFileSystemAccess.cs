using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;

namespace ViewerApp.DAL
{
    public interface IFileSystemAccess
    {
        string[] GetFiles(string path);
        string[] GetDirectories(string path);
        bool IsFileExists(string path);
        bool WriteFile(string path, byte[] file);
        bool WriteAllText(string path, string content);
        byte[] ReadFile(string path);
        string ReadText(string path);
        Album DeserializeAlbum(string fullAlbumFilePath);
    }
}
