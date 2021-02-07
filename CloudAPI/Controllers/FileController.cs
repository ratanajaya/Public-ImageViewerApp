using CloudAPI.AL;
using CloudAPI.AL.Services;
using CloudAPI.CustomMiddlewares;
using CloudAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace CloudAPI
{
    [ApiController]
    [Route("File")]
    public class FileController : ControllerBase
    {
        AuthorizationService _auth;
        LibraryRepository _library;
        FileRepository _file;

        public FileController(AuthorizationService auth, LibraryRepository library, FileRepository file) {
            _auth = auth;
            _library = library;
            _file = file;
        }

        #region QUERY
        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage(string id, int index, int size = 0) {
            var result = await _file.GetPageBytes(id, index, size);
            return Ok(result);
        }

        //[HttpGet("GetPageInfo")]
        //public IActionResult GetPageInfo(string id, int index) {
        //    var result = _file.GetPageInfo(id, index);
        //    return Ok(result);
        //}

        [HttpGet("GetAlbumPageInfos")]
        public IActionResult GetAlbumPageInfos(string id) {
            var result = _file.GetAlbumPageInfos(id);
            return Ok(result);
        }

        [HttpGet("GetCoverByQuery")]
        public async Task<IActionResult> GetCoverByQuery(string query, int size) {
            var cleanQuery = HttpUtility.UrlDecode(query);
            var firstAlbum = _library.GetAlbumVMs(1, 1, cleanQuery).FirstOrDefault();

            var result = await _file.GetPageBytes(firstAlbum.AlbumId, 0, size);

            return Ok(result);
        }
        #endregion

        #region COMMAND
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(FileInsertModel fileInfo) {
            _auth.DisableRouteOnPublicBuild();

            await _file.InsertFileToAlbum(fileInfo.AlbumId, fileInfo.SubDir, fileInfo.Name, fileInfo.FileBase64);
            return Ok("Success");
        }

        [HttpDelete("DeleteFile/{albumId}/{pageIndex}")]
        public IActionResult DeleteFile(string albumId, int pageIndex) {
            _auth.DisableRouteOnPublicBuild();

            _file.DeleteFileFromAlbum(albumId, pageIndex);
            return Ok("Success");
        }

        [HttpPost("MoveFile")] //UNTESTED
        public IActionResult MoveFile(FileUpdateModel param) {
            _auth.DisableRouteOnPublicBuild();

            _file.MoveFile(param.AlbumId, param.PageIndex, param.NewAlbumId);
            return Ok("Success");
        }

        [HttpPost("RenameFile")]
        public IActionResult RenameFile(FileRenameModel param) {
            _auth.DisableRouteOnPublicBuild();

            var result = _file.RenameFile(param.AlbumId, param.PageIndex, param.NewName);
            return Ok(result);
        }

        #endregion
    }
}
