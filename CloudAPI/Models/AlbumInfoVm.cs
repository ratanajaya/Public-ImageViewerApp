using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI
{
    public class AlbumInfoVm
    {
        public string[] Languages { get; set; }
        public string[] Tags { get; set; }
        public string[] Categories { get; set; }
        public string[] Orientations { get; set; }
        public string[] SuitableImageFormats { get; set; }
        public string[] SuitableVideoFormats { get; set; }
        public string JsonFileName { get; set; }
        public string[][] GenreNameAndActions { get; set; }
    }
}
