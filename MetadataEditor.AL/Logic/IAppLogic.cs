using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MetadataEditor.AL
{
    public interface IAppLogic
    {
        Task<AlbumViewModel> GetAlbumViewModelAsync(string path);
        Task<string> SaveAlbumJson(AlbumViewModel vm);
        string[] GetTags();
        string[] GetLanguages();
        string[] GetCategories();
        string[] GetOrientations();
    }
}
