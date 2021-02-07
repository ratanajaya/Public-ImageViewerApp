using System;
using System.Collections.Generic;
using System.Text;

namespace CloudAPI.AL.Models
{
    public class ConfigurationModel
    {
        public string LibraryPath { get; set; }
        public string FullPageCachePath { get; set; }
        public string FullAlbumDbPath { get; set; }
        public string FullArtistDbPath { get; set; }

        public string AlbumMetadataFileName { get; set; }

        public string BuildType { get; set; }

        public bool IsPublicBuild { 
            get {
                return BuildType == "Public";
            } 
        }
    }
}
