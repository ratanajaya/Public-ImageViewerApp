using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudAPI.AL;
using System.Web;
using SharedLibrary;
using CloudAPI.AL.Services;
using CloudAPI.AL.Models;
using CloudAPI.CustomMiddlewares;
using System.Diagnostics;
using Serilog;

namespace CloudAPI
{
    [ApiController]
    [Route("Crud")]
    public class CrudController : ControllerBase
    {
        AuthorizationService _auth;
        LibraryRepository _library;
        FileRepository _file;
        IAlbumInfoProvider _ai;
        ILogger _logger;
        ConfigurationModel _config;

        public CrudController(AuthorizationService auth, LibraryRepository library, FileRepository file, IAlbumInfoProvider ai, ILogger logger, ConfigurationModel config) {
            _auth = auth;
            _library = library;
            _file = file;
            _ai = ai;
            _logger = logger;
            _config = config;
        }

        #region Query
        [HttpGet("GetAlbumCardModels")]
        public IActionResult GetAlbumCardModels(int page, int row, string query) {
            #region Local Function
            bool ContainsConnector(string source) {
                return source.IndexOfAny( new char[] { ':', '!', '>', '<' }) > - 1;
            }
            #endregion

            var cleanQuery = HttpUtility.UrlDecode(query);
            var modifiedQuery = cleanQuery != null && !ContainsConnector(cleanQuery) ? $"title:{cleanQuery}" : cleanQuery;
            var data = _library.GetAlbumVMs(page, row, modifiedQuery);

            var result = data.Select(a => new AlbumCardModel {
                AlbumId = a.AlbumId,
                FullTitle = a.Album.GetFullTitleDisplay(),
                Languages = a.Album.Languages,
                IsRead = a.Album.IsRead,
                IsWip = a.Album.IsWip,
                Tier = a.Album.Tier,
                LastPageIndex = a.LastPageIndex,
                PageCount = a.PageCount,
                CoverInfo = a.CoverInfo
            })
            .OrderBy(ac => ac.IsRead).ThenBy(ac => ac.FullTitle)
            .ToList();

            return Ok(result);
        }
        
        [HttpGet("GetGenreCardModels")]
        public IActionResult GetGenreCardModels() {
            var data = _ai.GenreQueries.GroupBy(gq => gq.Group)
                .Select(gr => new AlbumCardGroup {
                    Name = gr.Key.ToString(),
                    AlbumCms = gr.Select(gi => new AlbumCardModel {
                        AlbumId = gi.Query,
                        FullTitle = gi.Name,
                        CoverInfo = _file.GetRandomCoverPageInfoByQuery(gi.Query),

                        IsRead = true,
                        IsWip = false,
                        Languages = new List<string>(),
                        LastPageIndex = 0,
                        PageCount = 0,
                        Tier = 0,
                    }).ToList()
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("GetArtistCardModels")]
        public IActionResult GetArtistCardModels(int? tier) {
            var albumCMs = _library.GetArtistVMs(tier)
                .Select(a => new AlbumCardModel {
                    AlbumId = $"artist:{a.Name}",
                    FullTitle = a.Name,
                    CoverInfo = _file.GetRandomCoverPageInfoByQuery($"artist:{a.Name}"),
                    #region Useless fields
                    IsRead = true,
                    IsWip = false,
                    Languages = new List<string>(),
                    LastPageIndex = 0,
                    PageCount = 0,
                    Tier = 0
                    #endregion
                })
                .OrderBy(a => a.FullTitle)
                .ToList();

            var result = new List<AlbumCardGroup>{ 
                new AlbumCardGroup {
                    Name = $"Tier {tier.GetValueOrDefault()} Artists",
                    AlbumCms = albumCMs
                }
            };

            return Ok(result);

            #region LEGACY
            //var data = _library.GetArtistVMs(minTier).GroupBy(aq => aq.Tier.RoundDownToNearest10())
            //    .Select(gr => new AlbumCardGroup {
            //        Name = $"{gr.Key.ToString()}+",
            //        AlbumCms = gr.Select(gi => new AlbumCardModel {
            //            AlbumId = $"artist:{gi.Name}",
            //            FullTitle = gi.Name,
            //            CoverInfo = _file.GetRandomCoverPageInfoByQuery($"artist:{gi.Name}"),

            //            IsRead = true,
            //            IsWip = false,
            //            Languages = new List<string>(),
            //            LastPageIndex = 0,
            //            PageCount = 0,
            //            Tier = 0,
            //        })
            //        .OrderBy(ac => ac.FullTitle)
            //        .ToList()
            //    })
            //    .OrderByDescending(cm => cm.Name)
            //    .ToList();
            #endregion
        }

        [HttpGet("GetAlbumChapters")]
        public IActionResult GetAlbumChapters(string id) {
            var result = _library.GetAlbumChapters(id);

            return Ok(result);
        }

        [HttpGet("GetAlbumInfo")]
        public IActionResult GetAlbumInfo() {
            var data = new AlbumInfoVm {
                Tags = _ai.Tags,
                Categories = _ai.Categories,
                Orientations = _ai.Orientations,
                Languages = _ai.Languages,
                SuitableImageFormats = _ai.SuitableImageFormats,
                SuitableVideoFormats = _ai.SuitableVideoFormats
            };

            return Ok(data);
        }

        [HttpGet("GetTagVMs")]
        public IActionResult GetTagVMs() {
            var data = _library.GetTagVMs();
            return Ok(data);
        }
        #endregion

        #region Command
        [HttpPost("InsertAlbum")]
        public IActionResult InsertAlbum(AlbumCreateModel albumCEV) {
            _auth.DisableRouteOnPublicBuild();
            string albumId = _library.InsertAlbum(albumCEV.OriginalFolderName, albumCEV.Album);

            return Ok(albumId);
        }

        [HttpPost("UpdateAlbum")]
        public IActionResult UpdateAlbum(AlbumVM albumVM) {
            _auth.DisableRouteOnPublicBuild();
            string albumId = _library.UpdateAlbum(albumVM);

            return Ok(albumId);
        }

        [HttpPost("UpdateAlbumOuterValue")]
        public IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param) {
            param.LastPageIndex = _config.IsPublicBuild ? 0 : param.LastPageIndex;

            string albumId = _library.UpdateAlbumOuterValue(param.AlbumId, param.LastPageIndex);

            return Ok(albumId);
        }

        [HttpPut("UpdateAlbumTier")]
        public IActionResult UpdateAlbumTier(AlbumTierParam param) {
            _auth.DisableRouteOnPublicBuild();
            string albumId = _library.UpdateAlbumTier(param.AlbumId, param.Tier);

            return Ok("Success");
        }

        [HttpGet("RecountAlbumPages/{id}")]
        public IActionResult RecountAlbumPages(string id) {
            _auth.DisableRouteOnPublicBuild();
            int pageCount = _library.RecountAlbumPages(id);

            return Ok(pageCount);
        }

        [HttpDelete("DeleteAlbum/{id}")]
        public IActionResult DeleteAlbum(string id) {
            _auth.DisableRouteOnPublicBuild();
            _library.DeleteAlbum(id);

            return Ok(id);
        }

        [HttpDelete("DeleteAlbumChapter/{id}/{chapterName}")]
        public IActionResult DeleteAlbumChapter(string id, string chapterName) {
            _auth.DisableRouteOnPublicBuild();
            var newPageCount = _library.DeleteAlbumChapter(id, chapterName);

            return Ok(newPageCount);
        }

        [HttpGet("RescanDatabase")]
        public async Task<IActionResult> RescanDatabase() {
            _auth.DisableRouteOnPublicBuild();
            await _library.RescanDatabase();

            return Ok("Success");
        }

        [HttpGet("ReloadDatabase")]
        public async Task<IActionResult> ReloadDatabase() {
            _auth.DisableRouteOnPublicBuild();
            await _library.ReloadDatabase();

            return Ok("Success");
        }
        #endregion

        #region LEGACY
        [HttpGet("GetAlbumVm")]
        public IActionResult GetAlbumVM(string id) {
            var result = _library.GetAlbumVM(id);

            return Ok(result);
        }

        [HttpGet("GetArtistVMs")]
        public IActionResult GetArtistVMs(int? tier) {
            if(_config.IsPublicBuild) {
                tier = null;
            }
            var data = _library.GetArtistVMs(tier);
            return Ok(data);
        }

        #endregion
    }
}
