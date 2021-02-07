using CloudAPI.AL;
using CloudAPI.AL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI
{
    public class AlbumCardModel
    {
        public string AlbumId { get; set; }
        public string FullTitle { get; set; }
        public List<string> Languages { get; set; }
        public bool IsRead { get; set; }
        public bool IsWip { get; set; }
        public int Tier { get; set; }
        public int LastPageIndex { get; set; }
        public int PageCount { get; set; }
        public FileInfoModel CoverInfo { get; set; }
    }

    public class AlbumCardGroup
    {
        public string Name { get; set; }
        public List<AlbumCardModel> AlbumCms { get; set; }
    }
}
