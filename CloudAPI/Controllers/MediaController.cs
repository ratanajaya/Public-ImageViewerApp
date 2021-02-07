using CloudAPI.AL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using SharedLibrary;
using CloudAPI.AL.Models;
using CloudAPI.AL.Services;

namespace CloudAPI.Controllers
{
    [ApiController]
    [Route("Media")]
    public class MediaController : ControllerBase
    {
        ConfigurationModel _config;
        FileRepository _file;
        public MediaController(ConfigurationModel config, FileRepository file) {
            _config = config;
            _file = file;
        }

        [HttpGet("StreamPage")]
        public async Task<IActionResult> StreamImage(string albumId, string alRelPathBase64) {
            string libRelalbumPath = albumId.UriFriendlyBase64Decode();
            string alRelPagePath = alRelPathBase64.UriFriendlyBase64Decode();
            string fullPath = Path.Combine(_config.LibraryPath, libRelalbumPath, alRelPagePath);

            var memory = new MemoryStream();
            using(var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", Path.GetFileName(fullPath), true); //enableRangeProcessing = true
        }

        [HttpGet("StreamImage")]
        public async Task<IActionResult> StreamImage(string librelPathBase64) {
            string libRelPath = librelPathBase64.UriFriendlyBase64Decode();
            string fullPath = Path.Combine(_config.LibraryPath, libRelPath);

            var memory = new MemoryStream();
            using(var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", Path.GetFileName(fullPath), true); //enableRangeProcessing = true
        }

        [HttpGet("StreamResizedImage")]
        public async Task<IActionResult> StreamResizedImage(string librelPathBase64, int maxSize) {
            string libRelPath = librelPathBase64.UriFriendlyBase64Decode();
            string fullCachedPath = await _file.GetFullCachedPath(libRelPath, maxSize);

            var memory = new MemoryStream();
            using(var stream = new FileStream(fullCachedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", Path.GetFileName(fullCachedPath), true); //enableRangeProcessing = true
        }

        [HttpGet("StreamVideo")]
        public async Task<IActionResult> StreamVideo(string librelPathBase64) {
            string libRelPath = librelPathBase64.UriFriendlyBase64Decode();
            string fullPath = Path.Combine(_config.LibraryPath, libRelPath);

            var memory = new MemoryStream();
            using(var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", Path.GetFileName(fullPath), true); //enableRangeProcessing = true
        }

        ////Can be used to download any large file
        //[HttpGet("StreamVideo2")]
        //public async Task<IActionResult> StreamVideo2() {
        //    var path = "D:\\LargeFileTest.zip";
        //    var memory = new MemoryStream();
        //    using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan)) {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
        //    return File(memory, "application/octet-stream", Path.GetFileName(path), true); //enableRangeProcessing = true
        //}
    }
}
