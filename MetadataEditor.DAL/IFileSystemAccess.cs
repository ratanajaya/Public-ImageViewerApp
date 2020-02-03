using SharedLibrary;
using System;
using System.IO;

namespace MetadataEditor.DAL
{
    public interface IFileSystemAccess
    {
        string[] GetFiles(string path);
        string[] GetDirectories(string path);
        bool SerializeAlbum(string path, Album album);
        Album DeserializeAlbum(string path);
        bool FileExist(string path);
    }
}
