using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public class AppResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}
