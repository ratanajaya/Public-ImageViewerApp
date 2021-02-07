using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI
{
    public class AlbumOuterValueParam
    {
        public string AlbumId { get; set; }
        public int LastPageIndex { get; set; }
    }

    public class AlbumTierParam
    {
        public string AlbumId { get; set; }
        public int Tier { get; set; }
    }
}
