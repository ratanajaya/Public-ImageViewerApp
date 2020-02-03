using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public class QueryCard : ICardItem
    {
        public string Name { get; set; }
        public int Tier { get; set; }
        public string CoverPath { get; set; }
        public string Query { get; set; }

        public string GetTitle() {
            return Name;
        }

        public string GetCoverPath() {
            return CoverPath;
        }

        public int GetPageCount() {
            return 0;
        }

        public int GetLastPageIndex() {
            return 0;
        }

        public string GetAction() {
            return GetCardType() + ":" + Query;
        }

        public string GetCardType() {
            return "query";
        }

        public List<string> GetFlags() {
            return new List<string>();
        }

        public List<string> GetLanguages() {
            return new List<string>();
        }

        public bool GetIsNew() {
            return false;
        }

        public bool GetIsWip() {
            return false;
        }
    }
}
