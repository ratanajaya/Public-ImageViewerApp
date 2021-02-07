using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using SharedLibrary;

namespace CloudAPI.AL.Models
{
    public class  AlbumVM
    {
        [IgnoreMember]
        public string AlbumId { 
            get {
                return Path.UriFriendlyBase64Encode();
            }
        }
        public string Path { get; set; }
        public int PageCount { get; set; }
        public int LastPageIndex { get; set; }
        public FileInfoModel CoverInfo { get; set; }
        public Album Album { get; set; }
    }
}
