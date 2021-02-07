using MessagePack;
using MessagePack.Resolvers;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace CloudAPI.AL.DataAccess
{
    public interface ISystemIOAbstraction
    {
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
        Task<T> DeserializeJson<T>(string path);
        Task<T> DeserializeMsgpack<T>(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        List<string> GetSuitableFilePaths(string folderPath, string[] suitableFileFormats, int depth);
        IEnumerable<string> GetSuitableFilePathsWithNaturalSort(string folderPath, string[] suitableFileFormats, int depth);
        bool IsFileExists(string path);
        void MoveFile(string currentPath, string newPath);
        Bitmap ReadBitmap(string path);
        byte[] ReadFile(string path);
        Task<string> ReadText(string path);
        void SerializeToJson(string path, dynamic item);
        void SerializeToMsgpack(string path, dynamic item);
        Task WriteAllText(string path, string content);
        Task<bool> WriteFile(string path, byte[] file);

        #region LEGACY
        //Album DeserializeAlbum(string path);
        //List<AlbumVM> DeserializeAlbumVMs(string path);
        //List<QueryVM> DeserializeArtistVMs(string path);
        //Task SerializeAlbum(string path, Album album);
        //Task SerializeAlbumVMs(string path, List<AlbumVM> albumVMs);
        //Task SerializeArtistVMs(string path, List<QueryVM> artistVMs);
        #endregion
    }

    //Very thin wrapper of System.IO
    //Must not require any dependency
    public class SystemIOAbstraction : ISystemIOAbstraction
    {
        #region QUERY
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

        public byte[] ReadFile(string path) {
            byte[] result = File.ReadAllBytes(path);
            return result;
        }

        public Bitmap ReadBitmap(string path) {
            var result = new Bitmap(path);
            return result;
        }

        public async Task<string> ReadText(string path) {
            string result = await File.ReadAllTextAsync(path);
            return result;
        }

        public List<string> GetSuitableFilePaths(string folderPath, string[] suitableFileFormats, int depth) {
            #region LEGACY
            //var result = new List<string>();

            //string[] files = GetFiles(folderPath);
            //foreach(string file in files) {
            //    var ext = Path.GetExtension(file);
            //    if(Array.IndexOf(suitableFileFormats, ext) > -1) {
            //        result.Add(file);
            //    }
            //}

            //if(depth > 0) {
            //    string[] subDirs = GetDirectories(folderPath);
            //    foreach(string subDir in subDirs) {
            //        result.AddRange(GetSuitableFilePathsRecursive(subDir, suitableFileFormats, depth - 1));
            //    }
            //}

            //return result;
            #endregion

            var searchOption = depth > 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var result = Directory.EnumerateFiles(folderPath, "*.*", searchOption)
                .Where(file => Array.IndexOf(suitableFileFormats, Path.GetExtension(file)) > -1)
                .ToList();

            return result;
        }

        public IEnumerable<string> GetSuitableFilePathsWithNaturalSort(string folderPath, string[] suitableFileFormats, int depth) {
            var filePaths = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => Array.IndexOf(suitableFileFormats, Path.GetExtension(f)) > -1)
                .ToList();

            var fileNames = filePaths.Select(f => Path.GetFileName(f));
            var sortedFileNames = fileNames.OrderByAlphaNumeric(f => f);
            var sortedFilePaths = sortedFileNames.Select(f => Path.Combine(folderPath, f));

            if(depth <= 0) { return sortedFilePaths; }

            string[] subDirs = GetDirectories(folderPath);
            var filePathsFromSubdir = subDirs.SelectMany(s => GetSuitableFilePathsWithNaturalSort(s, suitableFileFormats, depth - 1));

            var combinedFilePaths = sortedFilePaths.Concat(filePathsFromSubdir);

            return combinedFilePaths;
        }
        #endregion

        #region COMMAND
        public async Task<bool> WriteFile(string path, byte[] file) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllBytesAsync(path, file);
            return true;
        }

        public async Task WriteAllText(string path, string content) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllTextAsync(path, content);
        }

        public void DeleteFile(string path) {
            if(File.Exists(path)) {
                File.Delete(path);
            }
        }

        public void MoveFile(string currentPath, string newPath) {
            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
            File.Move(currentPath, newPath);
        }

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path) {
            //SOLUTION: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it
            var dir = new DirectoryInfo(path);
            if(dir.Exists) {
                NormalizeAttributesRecursive(dir);
                dir.Delete(true);
            }
        }
        private void NormalizeAttributesRecursive(DirectoryInfo dir) {
            foreach(var subDir in dir.GetDirectories())
                NormalizeAttributesRecursive(subDir);
            foreach(var file in dir.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
            dir.Attributes = FileAttributes.Normal;
        }

        public T DeserializeFileSync<T>(string path) {
            byte[] fileBytes = File.ReadAllBytes(path);
            T result = JsonSerializer.Deserialize<T>(fileBytes);
            return result;
        }

        public async Task<T> DeserializeJson<T>(string path) {
            if(!File.Exists(path)) {
                return default(T);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(path);
            T result = JsonSerializer.Deserialize<T>(fileBytes);
            return result;
        }

        public async Task<T> DeserializeMsgpack<T>(string path) {
            if(!File.Exists(path)) {
                return default(T);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(path);
            T result = MessagePackSerializer.Deserialize<T>(fileBytes, ContractlessStandardResolver.Options);
            return result;
        }

        public void SerializeToJson(string path, dynamic item) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] fileBytes = JsonSerializer.Serialize(item);
            File.WriteAllBytes(path, fileBytes);
        }

        public void SerializeToMsgpack(string path, dynamic item) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] fileBytes = MessagePackSerializer.Serialize(item, ContractlessStandardResolver.Options);
            File.WriteAllBytes(path, fileBytes);
        }

        #endregion

        #region LEGACY non generic serializer
        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public Album DeserializeAlbum(string path) {
        //    byte[] fileBytes = File.ReadAllBytes(path);
        //    Album result = JsonSerializer.Deserialize<Album>(fileBytes);
        //    return result;
        //}

        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public async Task SerializeAlbum(string path, Album album) {
        //    byte[] fileBytes = Utf8Json.JsonSerializer.Serialize(album);
        //    await File.WriteAllBytesAsync(path, fileBytes);
        //}

        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public List<AlbumVM> DeserializeAlbumVMs(string path) {
        //    byte[] fileBytes = File.ReadAllBytes(path);
        //    var result = JsonSerializer.Deserialize<List<AlbumVM>>(fileBytes);
        //    return result;
        //}

        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public async Task SerializeAlbumVMs(string path, List<AlbumVM> albumVMs) {
        //    byte[] fileBytes = Utf8Json.JsonSerializer.Serialize(albumVMs);
        //    await File.WriteAllBytesAsync(path, fileBytes);
        //}

        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public List<QueryVM> DeserializeArtistVMs(string path) {
        //    byte[] fileBytes = File.ReadAllBytes(path);
        //    var result = JsonSerializer.Deserialize<List<QueryVM>>(fileBytes);
        //    return result;
        //}

        //[Obsolete("All the non generic serializer/deserializer are deprecated, use the generic version instead")]
        //public async Task SerializeArtistVMs(string path, List<QueryVM> artistVMs) {
        //    byte[] fileBytes = Utf8Json.JsonSerializer.Serialize(artistVMs);
        //    await File.WriteAllBytesAsync(path, fileBytes);
        //}
        #endregion
    }
}
