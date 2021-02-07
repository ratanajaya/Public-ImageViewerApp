using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.Models
{
    public class FileInsertModel
    {
        public string AlbumId { get; set; }
        public string SubDir { get; set; } //empty or null for root
        public string Name { get; set; }
        public string FileBase64 { get; set; }
    }

    public class FileUpdateModel
    {
        public string AlbumId { get; set; }
        public int PageIndex { get; set; }
        public string NewAlbumId { get; set; }
    }

    public class FileRenameModel
    {
        public string AlbumId { get; set; }
        public int PageIndex { get; set; }
        public string NewName { get; set; }
    }
}
