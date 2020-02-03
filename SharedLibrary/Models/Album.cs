using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharedLibrary
{
    //Used for universal _album.json file
    //Should not contains any logic
    //Should not contains any informations that: 
    //-Cann be infered through filesystem (ie. path, page count)
    //-Is volatile (ie. last page opened)
    public class Album
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Orientation { get; set; }

        public List<string> Artists { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Languages { get; set; }
        public List<string> Flags { get; set; }

        public int Tier { get; set; }
        public string Cover { get; set; }

        public bool IsWip { get; set; }
        public bool IsRead { get; set; }

        public DateTime EntryDate { get; set; }

        public string GetAllArtists() { return string.Join(",", Artists); }
        public string GetAllTags() { return string.Join(",", Tags); }
        public string GetAllLanguages() { return string.Join(",", Languages); }
        public string GetAllFlags() { return string.Join(",", Flags); }
        public string GetFullTitle() { return "[" + GetAllArtists() + "] " + Title; }
    }
}
