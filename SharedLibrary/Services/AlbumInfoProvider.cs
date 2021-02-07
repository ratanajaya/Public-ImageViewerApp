using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLibrary
{
    public class AlbumInfoProvider : IAlbumInfoProvider {
        public string[] Languages { get; } = {
            "English", "Japanese", "Chinese", "Other"
        };
        public string[] Tags { get; } = { }; //REDACTED
        public string[] Categories { get; } = {
            "Manga", "CGSet", "SelfComp"
        };
        public string[] Orientations { get; } = {
            "Portrait", "Landscape"
        };

        public string[] SuitableImageFormats { get; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public string[] SuitableVideoFormats { get; } = {
            ".webm", ".mp4"
        };
        public string[] SuitableFileFormats {
            get {
                return SuitableImageFormats.Concat(SuitableVideoFormats).ToArray();
            }
        }

        public string JsonFileName { get; } = "_album.json";

        public List<QueryModel> GenreQueries { 
            get {} //REDACTED
        }

        public string[] Tier1Artists { get; } = { 
        };
    }
}
