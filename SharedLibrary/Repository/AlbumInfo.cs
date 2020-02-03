using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public class AlbumInfo : IAlbumInfoProvider {
        public string[] Languages { get; } = {
            "English", "Japanese", "Chinese", "Other"
        };
        public string[] Tags { get; } = {
            //Implement array of string
        };
        public string[] Categories { get; } = {
            "Manga", "CGSet", "SelfComp"
        };
        public string[] Orientations { get; } = {
            "Portrait", "Landscape"
        };

        public string[] SuitableImageFormats { get; } = {
            ".jpg", ".jpeg", ".png"
        };
        public string[] SuitableVideoFormats { get; } = {
            ".webm", ".mp4"
        };
        public string JsonFileName { get; } = "_album.json";

        public string[][] GenreNameAndActions { get => CreateGenreNameAndActions(); }

        string[][] CreateGenreNameAndActions() {
            return new string[][] {
                //Implement array of array of string       
            };
        }
    }
}
