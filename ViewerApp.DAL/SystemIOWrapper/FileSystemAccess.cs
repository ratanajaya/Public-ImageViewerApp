using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharedLibrary;
using Utf8Json;

namespace ViewerApp.DAL
{
    public class FileSystemAccess : IFileSystemAccess
    {
        public string[] GetFiles(string path) {
            string[] result = Directory.GetFiles(path);
            return result;
        }

        public string[] GetDirectories(string path) {
            string[] result = Directory.GetDirectories(path);
            return result;
        }

        public bool IsFileExists(string path) {
            bool result = File.Exists(path);
            return result;
        }

        public bool WriteFile(string path, byte[] file) {
            File.WriteAllBytes(path, file);
            return true;
        }

        public bool WriteAllText(string path, string content) {
            File.WriteAllText(path, content);
            return true;
        }

        public byte[] ReadFile(string path) {
            byte[] result = File.ReadAllBytes(path);
            return result;
        }

        public string ReadText(string path) {
            string result = File.ReadAllText(path);
            return result;
        }

        public Album DeserializeAlbum(string fullAlbumFilePath) {
            byte[] albumByte = File.ReadAllBytes(fullAlbumFilePath);
            Album result = JsonSerializer.Deserialize<Album>(albumByte);
            return result;
        }
    }
}
