using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public interface IAlbumInfoProvider
    {
        string[] Languages { get; }
        string[] Tags { get; }
        string[] Categories { get; }
        string[] Orientations { get; }

        string[] SuitableImageFormats { get; }
        string[] SuitableVideoFormats { get; }
        string[] SuitableFileFormats { get; }
        string JsonFileName { get; }

        List<QueryModel> GenreQueries { get; }
        string[] Tier1Artists { get; }
    }
}
