using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharedLibrary;
using SharedLibrary.Helpers;

namespace ViewerApp.DAL
{
    public class ExternalStorageLogic : IExternalStorageLogic
    {
        //Higher level layer must never know library path
        //All paths returned from here must be relative of libraryPath
        string libraryPath { get; set; }
        IFileSystemAccess fa;
        IAlbumInfoProvider ai;

        #region Initialization
        public ExternalStorageLogic(IFileSystemAccess fs, IAlbumInfoProvider ai) {
            this.fa = fs;
            this.ai = ai;
        }

        public bool Initialize() {
            libraryPath = FindLibraryPath("storage","TestLibrary");
            return true;
        }

        string FindLibraryPath(string root, string target) {
            string result = "";
            List<string> dirs = fa.GetDirectories(root).ToList();
            dirs.Remove("storage/emulated");
            dirs.Remove("storage/self");

            foreach (string dir in dirs) {
                if (dir.Contains(target, StringComparison.OrdinalIgnoreCase)) {
                    result = dir;
                    break;
                }
                else {
                    result = FindLibraryPath(dir, target);
                    if (result.Contains(target, StringComparison.OrdinalIgnoreCase)) {
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region Albums
        //Higher level layer must never know ALBUM_FILE
        const string ALBUM_FILE = "_album.json";
        /// <summary>
        /// Get all library-relative paths that contains ALBUM_FILE
        /// </summary>
        public List<string> GetAlbumPaths() {
            var result = GetAlbumPathsRecursive(libraryPath);
            return result;
        }
        List<string> GetAlbumPathsRecursive(string fullPath) {
            var result = new List<string>();
            if (fa.IsFileExists(Path.Combine(fullPath, ALBUM_FILE))) {
                string libRelAlbumPath = Path.GetRelativePath(libraryPath, fullPath);
                result.Add(libRelAlbumPath);
            }

            string[] subDirs = fa.GetDirectories(fullPath);
            foreach(string subDir in subDirs) {
                result.AddRange(GetAlbumPathsRecursive(subDir));
            }

            return result;
        }

        public Album GetAlbum(string libRelAlbumPath) {
            string fullAlbumFilePath = Path.Combine(libraryPath, libRelAlbumPath, ALBUM_FILE);
            Album result = fa.DeserializeAlbum(fullAlbumFilePath);
            return result;
        }

        public List<string> GetBookmarkPaths(string libRelAlbumPath) {
            var result = new List<string>();
            string fullAlbumPath = Path.Combine(libraryPath, libRelAlbumPath);
            string[] subDirs = fa.GetDirectories(fullAlbumPath);
            foreach(string subDir in subDirs) {
                string albumRelBookmarkPath = Path.GetRelativePath(fullAlbumPath, subDir);
                result.Add(albumRelBookmarkPath);
            }
            return result;
        }
        #endregion

        #region Pages
        /// <summary>
        /// First suitable file according to AlbumInfoProvider. Used to get Cover and Bookmark file. The returned path is relative to input path. Only check input folder
        /// </summary>
        public string GetFirstSuitableFile(string libRelPath, int depth) {
            string fullPath = Path.Combine(libraryPath, libRelPath);
            string fullSuitableFilePath = GetFirstSuitableFileRecurse(fullPath, depth, 0);
            if (!string.IsNullOrEmpty(fullSuitableFilePath)) {
                string result = Path.GetRelativePath(fullPath, fullSuitableFilePath);
                return result;
            }
            return "";
        }
        string GetFirstSuitableFileRecurse(string fullPath, int depth, int currentDepth) {
            if (currentDepth > depth) {
                return "";
            }
            string[] files = fa.GetFiles(fullPath);
            foreach (string file in files) {
                if (file.ContainsAny(ai.SuitableImageFormats)) {
                    //string inputPathRelativePath = Path.GetRelativePath(fullPath, file);
                    return file;
                }
            }
            string[] subDirs = fa.GetDirectories(fullPath);
            foreach (string subDir in subDirs) {
                string retval = GetFirstSuitableFileRecurse(subDir, depth, currentDepth++);
                if (!string.IsNullOrEmpty(retval)) {
                    return retval;
                }
            }
            return "";
        }

        /// <summary>
        /// Get all album-relative Suitable File paths
        /// </summary>
        public List<string> GetPagePaths(string libRelAlbumPath) {
            var result = new List<string>();

            string fullAlbumPath = Path.Combine(libraryPath, libRelAlbumPath);
            var pageFullPaths = GetPagePathsRecurse(fullAlbumPath, 1, 0);

            //Extra iteration that could be solved within the recursive method itself
            //Not the most efficient but more maintainable
            foreach(string pageFullPath in pageFullPaths) {
                string albumRelPagePath = Path.GetRelativePath(fullAlbumPath, pageFullPath);
                result.Add(albumRelPagePath);
            }

            return result;
        }

        List<string> GetPagePathsRecurse(string fullPath, int depth, int currentDepth) {
            var result = new List<string>();
            if (currentDepth <= depth) {
                string[] files = fa.GetFiles(fullPath);
                foreach (string file in files) {
                    if (file.ContainsAny(ai.SuitableImageFormats)) {
                        result.Add(file);
                    }
                }
                string[] subDirs = fa.GetDirectories(fullPath);
                foreach (string subDir in subDirs) {
                    result.AddRange(GetPagePathsRecurse(subDir, depth, currentDepth++));
                }
            }
            return result;
        }

        public bool IsCoverValid(string libRelCoverPath) {
            string fullPath = Path.Combine(libraryPath, libRelCoverPath);
            return fa.IsFileExists(fullPath);
        }
        #endregion

        #region Abstraction Leaks
        public string LEAKGetLibraryPath() {
            return libraryPath;
        }
        public void LEAKSetLibraryPath(string libraryPath) {
            this.libraryPath = libraryPath;
        }

        public void TESTBypassInitialize(string libraryPath) {
            this.libraryPath = libraryPath;
        }
        #endregion
    }
}
