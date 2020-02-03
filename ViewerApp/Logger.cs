using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewerApp.AL;

namespace ViewerApp
{
    public static class Logger
    {
        public static List<string> Logs { get; set; }

        public static void Log(AppResponse response) {
            if(Logs == null) {
                Logs = new List<string>();
            }
            string message = "";
            message += response.Status ? "S" : "F";
            message += " | "+response.Message;
            Logs.Add(message);
        }
    }
}