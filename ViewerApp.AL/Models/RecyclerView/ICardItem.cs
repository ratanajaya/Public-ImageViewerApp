using System;
using System.Collections.Generic;
using System.Text;

namespace ViewerApp.AL
{
    public interface ICardItem
    {
        string GetTitle();
        string GetCoverPath();
        string GetAction();
        string GetCardType();
        int GetPageCount();
        int GetLastPageIndex();
        List<string> GetLanguages();
        List<string> GetFlags();
        bool GetIsNew();
        bool GetIsWip();
    }
}
