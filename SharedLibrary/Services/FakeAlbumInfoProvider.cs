using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLibrary
{
    public class FakeAlbumInfoProvider : IAlbumInfoProvider
    {
        public string[] Languages { get; } = {
            "English", "Japanese", "Chinese", "Other"
        };
        public string[] Tags {
            get {
                var list = new List<string> { "Cat", "Chicken", "Cow", "Dog", "Horse", "Human" };
                var arr = list.OrderBy(s => s).ToArray();
                return arr;
            }
        }

        public string[] Categories { get; } = {
            "Animal", "Tree"
        };
        public string[] Orientations { get; } = {
            "Portrait", "Landscape"
        };

        public string[] SuitableImageFormats { get; } = {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        public string[] SuitableVideoFormats { get; } = {
            ".webm", ".mp4"
        };
        public string[] SuitableFileFormats {
            get {
                return SuitableImageFormats.Concat(SuitableVideoFormats).ToArray();
            }
        }

        public string JsonFileName { get; } = "_album.json";

        public string[][] GenreNameAndActions { get => CreateGenreNameAndActions(); }

        string[][] CreateGenreNameAndActions() {
            return new string[][] {
                new string[2] { "Pet Animals", "Not Pet Animals" },
            };
        }

        public List<QueryModel> GenreQueries { 
            get {
                var result = new List<QueryModel> {
                   new QueryModel{ 
                       Name = "Pet Animals", 
                       Query = "tag:Cat|Dog", 
                       Group = 0 
                   },
                   new QueryModel{ 
                       Name = "Not Pet Animals", 
                       Query = "tag!Cat,tag!Dog", 
                       Group = 0 
                   }
                };
                return result;
            }
        }

        public string[] Tier1Artists { get; } = { 
            "Alan", 
            "Bob",
            "Greg",
            "Jack",
            "Sam"
        };
    }
}
