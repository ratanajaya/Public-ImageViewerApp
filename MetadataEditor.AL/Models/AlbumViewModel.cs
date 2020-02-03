using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;

namespace MetadataEditor.AL
{
    //Used by FormMain to display album to UI
    public class AlbumViewModel
    {
        public Album Album { get; set; }
        public string Path { get; set; } //Folder path
        public List<string> AlbumFiles { get; set; }

        public string GetCoverPath() { return System.IO.Path.Combine(Path, Album.Cover); }
        public string GetJsonPath() { return System.IO.Path.Combine(Path, "_album.json"); }
    }
}
