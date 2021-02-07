using System;
using System.Collections.Generic;
using System.Text;

namespace CloudAPI.AL.Models
{
    public class QueryVM
    {
        public string Name { get; set; }
        public int Tier { get; set; }
        public string Query { get; set; }
    }
}
