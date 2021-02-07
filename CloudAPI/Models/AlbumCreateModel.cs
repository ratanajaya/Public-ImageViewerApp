using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI
{
    public class AlbumCreateModel
    {
        public string OriginalFolderName { get; set; }
        public Album Album { get; set; }
    }
}
