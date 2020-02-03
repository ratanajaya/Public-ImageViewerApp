using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.DAL
{
    public interface IExternalStorageLogic
    {
        bool Initialize();

        List<string> GetAlbumPaths();
        Album GetAlbum(string libRelAlbumPath);
        List<string> GetBookmarkPaths(string libRelAlbumPath);
        string GetFirstSuitableFile(string libRelPath, int depth);
        List<string> GetPagePaths(string libRelAlbumPath);
        bool IsCoverValid(string libRelCoverPath);

        string LEAKGetLibraryPath();
        void LEAKSetLibraryPath(string libraryPath);
    }
}
