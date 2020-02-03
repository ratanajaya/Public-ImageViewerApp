using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public interface IAppLogic
    {
        AppResponse Initialize();
        AppResponse GetAlbumCards(string query);
        AppResponse GetArtistCards(int minTier);
        AppResponse GetGenreCards();

        AppResponse DeleteAlbum(string action);
        AppResponse FullScan();
        AppResponse QuickScan();
        AppResponse SaveDb();
        AppResponse DEBUGGetDb();
        AppResponse UndoDeletes();
        AppResponse UndoPendingOperations();
        AppResponse GetPagePaths(string albumPath);
        AppResponse GetPageItems(string albumPath);

        string LEAKGetLibraryPath();
        void TESTSetLibraryData(LibraryDatabase ld);
    }
}
