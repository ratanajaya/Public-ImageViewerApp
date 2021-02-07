using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharedLibrary;
using Utf8Json;

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
    public class FileSystemAccess : IFileSystemAccess
    {
        public string[] GetDirectories(string path) {
            string[] result = Directory.GetDirectories(path);
            return result;
        }

        public string[] GetFiles(string path) {
            string[] result = Directory.GetFiles(path);
            return result;
        }

        public StreamWriter CreateText(string path) {
            var result = File.CreateText(path);
            return result;
        }

        public bool SerializeAlbum(string path, Album album) {
            var serialized = JsonSerializer.Serialize(album);
            File.WriteAllBytes(path, serialized);
            return true;
        }

        public Album DeserializeAlbum(string path) {
            Album result;
            var bytes = File.ReadAllBytes(path);
            result = JsonSerializer.Deserialize<Album>(bytes);
            return result;
        }

        public bool FileExist(string path) {
            bool result = File.Exists(path);
            return result;
        }
    }
}
