using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharedLibrary
{
    //Used for universal _album.json file
    //Should not contains any informations that: 
    //-Cann be infered through filesystem (ie. path, page count)
    //-Is volatile (ie. last page opened)
    //Fields that are inferred from other fields and requires logic to output must be exposed as methods
    public class Album
    {
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Orientation { get; set; } = "";

        public List<string> Artists { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
        public List<string> Flags { get; set; } = new List<string>();

        public int Tier { get; set; } = 0;
        public string Cover { get; set; } = "";

        public bool IsWip { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public DateTime EntryDate { get; set; } = DateTime.Now;

        #region Legacy
        public string GetAllArtists() { return string.Join(",", Artists); }
        public string GetAllTags() { return string.Join(",", Tags); }
        public string GetAllLanguages() { return string.Join(",", Languages); }
        public string GetAllFlags() { return string.Join(",", Flags); }
        public string GetFullTitle() { return "[" + GetAllArtists() + "] " + Title; }
        #endregion

        public string GetArtistsDisplay() { return string.Join(", ", Artists); }
        public string GetTagsDisplay() { return string.Join(", ", Tags); }
        public string GetLanguagesDisplay() { return string.Join(", ", Languages); }
        public string GetFlagsDisplay() { return string.Join(", ", Flags); }
        public string GetFullTitleDisplay() { return "[" + GetArtistsDisplay() + "] " + Title; }

        [Obsolete("Album id is not AlbumPath unc base64")]
        public string GetId() {
            string seedArtists = GetAllArtists().TakeUppercaseAlphanumeric(3);
            string seedTitle = Title.TakeUppercaseAlphanumeric(5);
            string seedDateTime = string.Format(EntryDate.Minute.MapToUpperAlphanumericChar() + "" + EntryDate.Second.MapToUpperAlphanumericChar());

            string result = seedArtists + seedTitle + seedDateTime;
            return result;
        }

        public void ValidateAndCleanup() {
            Artists = CleanListString(Artists);
            Tags = CleanListString(Tags);
            Languages = CleanListString(Languages);
            Flags = CleanListString(Flags);
        }

        private List<string> CleanListString(List<string> source) {
            var result = source.Where(s => !string.IsNullOrWhiteSpace(s))
                             .Select(s => s.Trim())
                             .OrderBy(s => s)
                             .ToList();
            return result;
        }
    }
}
