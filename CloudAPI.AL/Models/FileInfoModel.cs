using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models
{    
    public class FileInfoModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public string UncPathEncoded { get; set; } //Universal Naming Convention Path
    }
}
