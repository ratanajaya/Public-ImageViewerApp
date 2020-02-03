using SharedLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using ViewerApp.DAL;
using SharedLibrary;
using Utf8Json;
using Dynamitey;
using NaturalSort.Extension;

namespace ViewerApp.AL
{
    public class AppLogic : IAppLogic
    {
        StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        IExternalStorageLogic el;
        IInternalStorageLogic il;
        IAlbumInfoProvider ai;
        LibraryDatabase db;
        Stopwatch sw;

        public AppLogic(IExternalStorageLogic el, IInternalStorageLogic il, IAlbumInfoProvider ai) {
            this.el = el;
            this.il = il;
            this.ai = ai;
            sw = new Stopwatch();
        }

        #region Inner Interface
        public AppResponse Initialize() { return Execute("PvInitialize", null); }
        public AppResponse GetAlbumCards(string query) { return Execute("PvGetAlbumCards", Build<ExpandoObject>.NewObject(albumCards: db.AlbumCards, query: query));}
        public AppResponse GetArtistCards(int minTier) { return Execute("PvGetArtistCards", minTier); }
        public AppResponse GetGenreCards() { return Execute("PvGetGenreCards", null); }


        public AppResponse SaveDb() { return Execute("PvSaveDb", null); }
        public AppResponse DeleteAlbum(string action) { return Execute("PvDeleteAlbum", action); }
        public AppResponse UndoDeletes() { return Execute("PvUndoDeletes", null); }
        public AppResponse UndoPendingOperations() { return Execute("PvUndoPendingOperations", null); }
        public AppResponse FullScan() { return Execute("PvFullScan", null); }
        public AppResponse QuickScan() { return Execute("PvQuickScan", null); }
        public AppResponse GetPagePaths(string albumPath) { return Execute("PvGetPagePaths", albumPath); }
        public AppResponse GetPageItems(string albumPath) { return Execute("PvGetPageItems", albumPath); }

        public AppResponse DEBUGGetDb() { return Execute("PvGetDb", null); }

        public string LEAKGetLibraryPath() {
            return el.LEAKGetLibraryPath();
        }

        public void TESTSetLibraryData(LibraryDatabase ld) {
            this.db = ld;
        }

        AppResponse Execute(string name, dynamic param) {
            sw.Restart();
            var result = new AppResponse();
            result.Message = name;
            try {
                if(name == "PvInitialize") {
                    result.Data = PvInitialize();
                }
                else if(name == "PvGetAlbumCards") {
                    IEnumerable<AlbumCard> albumCards = param.albumCards;
                    string query = param.query;
                    result.Data = PvGetAlbumCards(albumCards, query);
                }
                else if(name == "PvGetArtistCards") {
                    int minTier = param;
                    result.Data = PvGetArtistCards(minTier);
                }
                else if(name == "PvGetGenreCards") {
                    result.Data = PvGetGenreCards();
                }
                else if(name == "PvDeleteAlbum") {
                    string action = param;
                    result.Data = PvDeleteAlbum(action);
                }
                else if(name == "PvSaveDb") {
                    result.Data = PvSaveDb();
                }
                else if(name == "PvGetDb") {
                    result.Data = PvGetDb();
                }
                else if(name == "PvUndoDeletes") {
                    result.Data = PvUndoDeletes();
                }
                else if(name == "PvUndoPendingOperations") {
                    result.Data = PvUndoPendingOperations();
                }
                else if(name == "PvFullScan") {
                    result.Data = PvFullScan();
                }
                else if(name == "PvQuickScan") {
                    result.Data = PvQuickScan();
                }
                else if(name == "PvGetPagePaths") {
                    string albumPath = param;
                    result.Data = PvGetPagePaths(albumPath);
                }
                else if (name == "PvGetPageItems") {
                    string albumPath = param;
                    result.Data = PvGetPageItems(albumPath);
                }

                result.Status = true;
            }
            catch(Exception e) {
                result.Status = false;
                result.Message += " | " + e.ToString();
            }
            sw.Stop();
            result.ExecutionTime = sw.Elapsed;
            return result;
        }
        #endregion


        private bool PvInitialize() {
            el.Initialize();
            il.Initialize();

            //PvGetDb();
            if(db == null) {
                PvFullScan();
            }

            return true;
        }

        #region In Memory
        List<ICardItem> PvGetAlbumCards(IEnumerable<AlbumCard> albumCards, string query) {
            albumCards = albumCards.Where(a => a.PendingOperation != "delete");
            if(!string.IsNullOrWhiteSpace(query)) {
                foreach(string s in query.Split(',')) {
                    char connector = s.Contains(":") ? ':' :
                                     s.Contains("!") ? '!' :
                                     s.Contains(">") ? '>' :
                                     s.Contains("<") ? '<' :
                                     throw new Exception("Invalid search query");
                    string key = s.Split(connector)[0];
                    string val = s.Split(connector)[1];

                    if(key.Equals("title", comparer)) {
                        albumCards = albumCards.Where(a => a.Album.Title.Contains(val, comparer));
                    }
                    else if(key.Equals("category", comparer)) {
                        albumCards = albumCards.Where(a => a.Album.Category.Equals(val, comparer));
                    }
                    else if(key.Equals("orientation", comparer)) {
                        albumCards = albumCards.Where(a => a.Album.Orientation.Equals(val, comparer));
                    }
                    else if(key.Equals("artist", comparer)) {
                        albumCards = albumCards.Where(a => a.Album.Artists.ContainsExact(val, comparer));
                    }
                    else if(key.Equals("tag", comparer)) {
                        string[] orvalues = val.Split('|');
                        albumCards = connector == ':' ?
                                    (orvalues.Length == 2 ? albumCards.Where(a => a.Album.Tags.ContainsExact(orvalues[0], comparer) ||
                                                                             a.Album.Tags.ContainsExact(orvalues[1], comparer)) :
                                    orvalues.Length == 1 ? albumCards.Where(a => a.Album.Tags.ContainsExact(val, comparer)) :
                                    albumCards) :
                                 connector == '!' ?
                                    (orvalues.Length == 2 ? albumCards.Where(a => !a.Album.Tags.ContainsExact(orvalues[0], comparer) ||
                                                                                 !a.Album.Tags.ContainsExact(orvalues[1], comparer)) :
                                    orvalues.Length == 1 ? albumCards.Where(a => !a.Album.Tags.ContainsExact(val, comparer)) :
                                    albumCards) :
                                 albumCards;
                    }
                    else if(key.Equals("language", comparer)) {
                        string[] orvalues = val.Split('|');
                        albumCards = connector == ':' ?
                                    (orvalues.Length == 2 ? albumCards.Where(a => a.Album.Languages.ContainsExact(orvalues[0], comparer) ||
                                                            a.Album.Languages.ContainsExact(orvalues[1], comparer)) :
                                     orvalues.Length == 1 ? albumCards.Where(a => a.Album.Languages.ContainsExact(val, comparer)) :
                                     albumCards) :
                                 connector == '!' ?
                                    (orvalues.Length == 2 ? albumCards.Where(a => !a.Album.Languages.ContainsExact(orvalues[0], comparer) ||
                                                                                !a.Album.Languages.ContainsExact(orvalues[1], comparer)) :
                                     orvalues.Length == 1 ? albumCards.Where(a => !a.Album.Languages.ContainsExact(val, comparer)) :
                                     albumCards) :
                                 albumCards;
                    }
                    else if(key.Equals("flag", comparer)) {
                        albumCards = albumCards.Where(a => a.Album.Flags.Contains(val, comparer));
                    }
                    else if(key.Equals("tier", comparer)) {
                        int ivalue = int.Parse(val);
                        albumCards = connector == ':' ? albumCards.Where(a => a.Album.Tier == ivalue) :
                                 connector == '>' ? albumCards.Where(a => a.Album.Tier > ivalue) :
                                 connector == '<' ? albumCards.Where(a => a.Album.Tier < ivalue) :
                                 connector == '!' ? albumCards.Where(a => a.Album.Tier != ivalue) :
                                 albumCards;
                    }
                    else if(key.Equals("iswip", comparer)) {
                        bool bvalue = bool.Parse(val);
                        albumCards = connector == ':' ? albumCards.Where(a => a.Album.IsWip == bvalue) :
                                 connector == '!' ? albumCards.Where(a => a.Album.IsWip != bvalue) :
                                 albumCards;
                    }
                    else if(key.Equals("isread", comparer)) {
                        bool bvalue = bool.Parse(val);
                        albumCards = connector == ':' ? albumCards.Where(a => a.Album.IsRead == bvalue) :
                                 connector == '!' ? albumCards.Where(a => a.Album.IsRead != bvalue) :
                                 albumCards;
                    }
                    else if(key.Equals("date", comparer)) {
                        DateTime dvalue = DateTime.Parse(val);
                        albumCards = connector == ':' ? albumCards = albumCards.Where(a => a.Album.EntryDate == dvalue) :
                                 connector == '!' ? albumCards = albumCards.Where(a => a.Album.EntryDate != dvalue) :
                                 connector == '>' ? albumCards = albumCards.Where(a => a.Album.EntryDate > dvalue) :
                                 connector == '<' ? albumCards = albumCards.Where(a => a.Album.EntryDate < dvalue) :
                                 albumCards;
                    }
                }
            }
            albumCards = albumCards.OrderBy(a => a.Album.GetFullTitle());
            var result = albumCards.ToList<ICardItem>();
            return result;
        }

        List<ICardItem> PvGetArtistCards(int minTier) {
            var result = db.ArtistCards.Where(a => a.Tier >= minTier).ToList<ICardItem>();
            return result;
        }

        List<ICardItem> PvGetGenreCards() {
            var result = db.GenreCards.ToList<ICardItem>();
            return result;
        }
        #endregion

        #region External Storage
        bool PvFullScan() {
            var albumCards = new List<AlbumCard>();
            var albumPaths = el.GetAlbumPaths();
            foreach(string albumPath in albumPaths) {
                var albumCard = new AlbumCard();
                var album = el.GetAlbum(albumPath);
                int pageCount = el.GetPagePaths(albumPath).Count;

                var bookmarks = new List<string>();
                var bookmarkPaths = el.GetBookmarkPaths(albumPath);
                foreach(string bookmarkPath in bookmarkPaths) {
                    string bookmark = Path.GetRelativePath(albumPath, bookmarkPath);
                    bookmarks.Add(bookmark);
                }

                albumCards.Add(new AlbumCard { Album = album, Bookmarks = bookmarks, PageCount = pageCount, Path = albumPath });
            }

            var artistNames = new List<string>();
            var artistCards = new List<QueryCard>();
            foreach(AlbumCard albumCard in albumCards) {
                foreach(string artistName in albumCard.Album.Artists) {
                    if(!artistNames.Contains(artistName)) {
                        artistNames.Add(artistName);
                        string action = "artist:" + artistName;
                        string coverPath = Path.Combine(albumCard.Path, albumCard.Album.Cover);
                        int tier = PvGetAlbumCards(albumCards, action).Count;
                        artistCards.Add(new QueryCard {
                            Name = artistName,
                            Query = action,
                            CoverPath = coverPath,
                            Tier = tier
                        });
                    }
                }
            }


            var genreCards = new List<QueryCard>();
            string[][] genreNameAndActions = ai.GenreNameAndActions;
            foreach(string[] genre in genreNameAndActions) {
                var coverAlbumCard = (AlbumCard)PvGetAlbumCards(albumCards, genre[1]).FirstOrDefault();
                string coverPath = coverAlbumCard != null ? coverAlbumCard.GetCoverPath() : "";

                genreCards.Add(new QueryCard { Name = genre[0], Query = genre[1], CoverPath = coverPath });
            }

            db = new LibraryDatabase { AlbumCards = albumCards, ArtistCards = artistCards, GenreCards = genreCards };

            return true;
        }

        bool PvQuickScan() {
            var albumCards = new List<AlbumCard>();
            var albumPaths = el.GetAlbumPaths();
            foreach(string potentialNewAlbumPath in albumPaths) {
                var newAlbumCards = new List<AlbumCard>();
                if(!db.AlbumCards.Any(a => a.Path.Equals(potentialNewAlbumPath))) { //If there is new album
                    string newAlbumPath = potentialNewAlbumPath;
                    var albumCard = new AlbumCard();
                    var album = el.GetAlbum(newAlbumPath);
                    int pageCount = el.GetPagePaths(newAlbumPath).Count;
                    newAlbumCards.Add(new AlbumCard { Album = album, PageCount = pageCount, Path = newAlbumPath });

                    var newArtistCards = new List<QueryCard>();
                    foreach(string potentialNewArtist in albumCard.Album.Artists) {
                        if(!db.ArtistCards.Any(a => a.Name.Equals(potentialNewArtist))) {
                            string newArtistName = potentialNewArtist;
                            string newAction = "artist:" + newArtistName;
                            string newCoverPath = Path.Combine(albumCard.Path, albumCard.Album.Cover);
                            newArtistCards.Add(new QueryCard {
                                Name = newArtistName,
                                Query = newAction,
                                CoverPath = newCoverPath,
                                Tier = 0
                            });
                        }
                    }
                    db.ArtistCards.AddRange(newArtistCards);
                }
                db.AlbumCards.AddRange(newAlbumCards);

                //Recalibrate album count for artist referenced by new albumms
                foreach(var newAlbumCard in newAlbumCards) {
                    foreach(string artist in newAlbumCard.Album.Artists) {
                        db.ArtistCards.Where(a => a.Name.Equals(artist)).First().Tier++;
                    }
                }
            }

            return true;
        }

        List<string> PvGetPagePaths(string albumPath) {
            var pagePaths = el.GetPagePaths(albumPath);
            var result = pagePaths.OrderBy(s => s, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToList<string>();
            return result;
        }

        List<IPageItem> PvGetPageItems(string albumPath) {
            var pagePaths = el.GetPagePaths(albumPath);
            var sortedPagePaths = pagePaths.OrderBy(s => s, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToList<string>();
            var result = new List<IPageItem>();
            foreach(string pagePath in sortedPagePaths) {
                result.Add(new ImagePage(Path.Combine(albumPath, pagePath)));
            }
            return result;
        }
        #endregion

        #region Internal Storage
        bool PvSaveDb() {
            string jsonDb = JsonSerializer.ToJsonString(db);
            il.SaveStringifiedDb(jsonDb);
            return true;
        }

        bool PvGetDb() {
            string jsonDb = il.GetStringifiedDb();
            db = JsonSerializer.Deserialize<LibraryDatabase>(jsonDb);
            return true;
        }

        bool PvDeleteAlbum(string action) {
            string actionType = action.ActionType();
            if(!actionType.Equals("album")) {
                return false;
            }
            string albumPath = action.ActionQuery();
            var albumCard = db.AlbumCards.Where(a => a.Path == albumPath).FirstOrDefault();
            albumCard.PendingOperation = "delete";
            return true;
        }

        bool PvUndoDeletes() {
            IEnumerable<AlbumCard> toBeUndone = db.AlbumCards.Where(a => a.PendingOperation == "delete");
            foreach(AlbumCard ac in toBeUndone) {
                ac.PendingOperation = "";
            }
            return true;
        }

        bool PvUndoPendingOperations() {
            IEnumerable<AlbumCard> toBeUndone = db.AlbumCards.Where(a => string.IsNullOrEmpty(a.PendingOperation) && 
                                                                         a.PendingOperation != "delete");
            foreach(AlbumCard a in toBeUndone) {
                a.PendingOperation = "";
            }
            return true;
        }
        #endregion

    }
}

#region Deprecated
//public AppResponse FixAlbumsCover() {
//    sw.Restart();
//    string message = "FixAlbumsCover()";
//    AppResponse result;
//    try {
//        foreach (AlbumCard albumCard in db.albumCards) {
//            if (!fl.IsCoverValid(albumCard)) {
//                albumCard.Album.Cover = fl.GetCover(albumCard.Path);
//                try {
//                    fl.UpdateAlbumCard(albumCard);
//                }
//                catch(Exception e) {
//                    //Logic is correct but no permission
//                    //May be accessible in rooted device
//                    message += " | " + albumCard.Path + ":" + e.Message;
//                }
//            }
//        }
//        result = new AppResponse { Status = true, Message = message };
//    }
//    catch (Exception e) {
//        message += " | " + e.ToString();
//        result = new AppResponse { Status = false, Message = message };
//    }

//    sw.Stop();
//    result.ExecutionTime = sw.Elapsed;
//    return result;
//}
#endregion