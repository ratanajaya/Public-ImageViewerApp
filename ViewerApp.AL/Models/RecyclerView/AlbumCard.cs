using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;

namespace ViewerApp.AL
{
    public class AlbumCard : ICardItem
    {
        public string Path { get; set; }//PRIMARY KEY
        public string PendingOperation { get; set; }//DELETE|UPDATE
        public int PageCount { get; set; }
        public int LastPageIndex { get; set; }
        public List<string> Bookmarks { get; set; }
        public Album Album { get; set; }

        public string GetTitle() {
            return Album.GetFullTitle();
        }

        public string GetCoverPath() {
            return System.IO.Path.Combine(Path, Album.Cover);
        }

        public int GetPageCount() {
            return PageCount;
        }

        public int GetLastPageIndex() {
            return LastPageIndex;
        }

        public string GetAction() {
            return GetCardType() + ":" + Path;
        }

        public string GetCardType() {
            return "album";
        }

        public List<string> GetLanguages() {
            return Album.Languages;
        }

        public List<string> GetFlags() {
            return Album.Flags;
        }

        public bool GetIsNew() {
            return !Album.IsRead;
        }

        public bool GetIsWip() {
            return Album.IsWip;
        }
    }
}
