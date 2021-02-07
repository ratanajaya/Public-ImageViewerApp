using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace MetadataEditor.DAL
{
    public interface IApiAccess
    {
        bool PostAlbum(AlbumViewModel albumVm);
    }


    public class ApiAccess : IApiAccess
    {
        public bool PostAlbum(AlbumViewModel albumVm) {

        }
    }
}
