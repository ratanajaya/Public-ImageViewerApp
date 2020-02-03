using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    //Self implemented dbcontext
    //Will eventually mimic Entity Framework dbcontext
    public class LibraryDatabase
    {
        public List<AlbumCard> AlbumCards { get; set; }
        public List<QueryCard> ArtistCards { get; set; }
        public List<QueryCard> GenreCards { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
