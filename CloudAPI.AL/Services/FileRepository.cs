using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using Serilog;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace CloudAPI.AL.Services
{
    public class FileRepository
    {
        ConfigurationModel _config;
        IAlbumInfoProvider _ai;
        ISystemIOAbstraction _io;
        IDbContext _db;
        LibraryRepository _lib;
        ILogger _logger;

        public FileRepository(ConfigurationModel config, IAlbumInfoProvider ai, IDbContext db, ISystemIOAbstraction io, LibraryRepository lib, ILogger logger) {
            _config = config;
            _ai = ai;
            _io = io;
            _db = db;
            _lib = lib;
            _logger = logger;
        }

        #region QUERY
        public byte[] GetPageBytes(string albumId, int pageIndex) {
            //var albumPath = _db.AlbumVMs.Where(vm => vm.AlbumId == albumId).FirstOrDefault()?.Path;
            var albumPath = albumId.UriFriendlyBase64Decode();

            var allFilePaths = _io.GetSuitableFilePaths(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1);
            var result = _io.ReadFile(allFilePaths[pageIndex]);

            return result;
        }

        public async Task<byte[]> GetPageBytes(string albumId, int pageIndex, int maxSize) {
            //var albumPath = _db.AlbumVMs.Where(vm => vm.AlbumId == albumId).FirstOrDefault()?.Path;

            var albumPath = albumId.UriFriendlyBase64Decode();

            var allFilePaths = _io.GetSuitableFilePaths(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1);

            var targetPage = await new Func<Task<string>>(async () => {
                if(maxSize == 0) { return allFilePaths[pageIndex]; }

                var libRelOriginalPagePath = Path.GetRelativePath(_config.LibraryPath, allFilePaths[pageIndex]);
                var fullCachedPagePath = await GetFullCachedPath(libRelOriginalPagePath, maxSize);
                return fullCachedPagePath;
            })();

            var result = _io.ReadFile(targetPage);

            return result;
        }

        public async Task<string> GetFullCachedPath(string libRelOriginalPagePath, int maxSize) {
            #region Local Function
            bool IsWebp(string ext) { return Path.GetExtension(ext) == ".webp"; }
            bool IsImage(string ext) { return _ai.SuitableImageFormats.Contains(Path.GetExtension(ext)); }
            bool IsVideo(string ext) { return _ai.SuitableVideoFormats.Contains(Path.GetExtension(ext)); }
            #endregion

            var fullCachedPagePath = Path.Combine(_config.FullPageCachePath, maxSize.ToString(), libRelOriginalPagePath + ".jpg");
            if(_io.IsFileExists(fullCachedPagePath)) { return fullCachedPagePath; }

            var fullOriginalPagePath = Path.Combine(_config.LibraryPath, libRelOriginalPagePath);

            var bitmapOriginal = await new Func<Task<Bitmap>>(async () => {
                var extension = Path.GetExtension(fullOriginalPagePath);
                if(IsWebp(extension)) {
                    var webpBytes = _io.ReadFile(fullOriginalPagePath);
                    var result = new Imazen.WebP.SimpleDecoder().DecodeFromBytes(webpBytes, webpBytes.Length);
                    return result;
                }
                else if(IsImage(extension)) {
                    return _io.ReadBitmap(fullOriginalPagePath);
                }
                else if(IsVideo(extension)) {
                    return await GenerateVideoThumbnail(fullOriginalPagePath);
                }
                else {
                    throw new Exception($"File format is not contained in SuitableImageFormats or SuitableVideoFormats | {fullOriginalPagePath}");
                }
            })();

            var bitmapResized = bitmapOriginal.ResizeToResolutionLimit(maxSize);
            bitmapOriginal.Dispose();
            var byteResized = bitmapResized.ToByteArray();
            bitmapResized.Dispose();
            await _io.WriteFile(fullCachedPagePath, byteResized);

            return fullCachedPagePath;
        }

        private async Task<Bitmap> GenerateVideoThumbnail(string path) {
            var vidThumbnailDir = Path.Combine(_config.FullPageCachePath, "Vid");
            _io.CreateDirectory(vidThumbnailDir);

            var tempThumbnailPath = Path.Combine(vidThumbnailDir, Guid.NewGuid().ToString() + ".jpg");
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(path, tempThumbnailPath, TimeSpan.FromSeconds(0));
            await conversion.Start();

            var result = _io.ReadBitmap(tempThumbnailPath);
            return result;
        }

        #region PageInfo
        public List<FileInfoModel> GetAlbumPageInfos(string albumId) {
            var libRelalbumPath = albumId.UriFriendlyBase64Decode();
            var fullAlbumPath = Path.Combine(_config.LibraryPath, libRelalbumPath);
            var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(fullAlbumPath, _ai.SuitableFileFormats, 1);

            var result = allFilePaths.Select(f => GenerateFileInfo(fullAlbumPath, f)
            ).ToList();

            return result;
        }

        private FileInfoModel GenerateFileInfo(string rootPath, string fullFilePath) {
            var result = new FileInfoModel {
                Name = Path.GetFileName(fullFilePath),
                UncPathEncoded = Path.GetRelativePath(rootPath, fullFilePath).UriFriendlyBase64Encode(),
                Extension = Path.GetExtension(fullFilePath)
            };
            return result;
        }

        #region LEGACY
        //public FileInfoModel GetPageInfo(string albumId, int pageIndex) {
        //    var albumPath = albumId.UriFriendlyBase64Decode();

        //    var result = GetPageInfoByPath(albumPath, pageIndex);

        //    return result;
        //}

        //public FileInfoModel GetPageInfoByPath(string albumPath, int pageIndex) {
        //    var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1).ToList();
        //    if(allFilePaths.Count == 0) {
        //        return new FileInfoModel();
        //    }
        //    var targetFullPath = allFilePaths[pageIndex];
        //    var fileInfo = new FileInfo(targetFullPath);

        //    var targetLibRelPath = Path.GetRelativePath(_config.LibraryPath, targetFullPath);

        //    var result = new FileInfoModel {
        //        Name = Path.GetFileName(targetFullPath),
        //        Size = fileInfo.Length,
        //        UncPathEncoded = targetLibRelPath.UriFriendlyBase64Encode(),
        //        Extension = Path.GetExtension(targetFullPath)
        //    };

        //    return result;
        //}
        #endregion

        public FileInfoModel GetRandomCoverPageInfoByQuery(string query) {
            #region Local Function
            FileInfoModel GetPageInfoByPath(string albumPath, int pageIndex) {
                var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1).ToList();
                if(allFilePaths.Count == 0) {
                    return new FileInfoModel();
                }
                var targetFullPath = allFilePaths[pageIndex];
                var fileInfo = new FileInfo(targetFullPath);

                var targetLibRelPath = Path.GetRelativePath(_config.LibraryPath, targetFullPath);

                var result = new FileInfoModel {
                    Name = Path.GetFileName(targetFullPath),
                    Size = fileInfo.Length,
                    UncPathEncoded = targetLibRelPath.UriFriendlyBase64Encode(),
                    Extension = Path.GetExtension(targetFullPath)
                };

                return result;
            }
            #endregion

            var empty = new FileInfoModel {
                Extension = "",
                Name = "",
                Size = 0,
                UncPathEncoded = ""
            };

            var albumsOfQuery = _lib.GetAlbumVMs(0, 0, query).ToList();
            var count = albumsOfQuery.Count;

            if(count == 0) { return empty; }

            var now = DateTime.Now;
            var seed = now.Year + now.Month + now.Day;
            var index = NumHelper.GenerateNumberFromSeed(seed, count);
            var albumForCover = albumsOfQuery[index];

            var result = new Func<FileInfoModel>(() => {
                try {
                    return GetPageInfoByPath(albumForCover.Path, 0);
                }
                catch(Exception e) {
                    _logger.Warning($"GetRandomCoverPageInfoByQuery | {query} | {albumForCover.Path}");
                    return empty;
                }
            })();

            return result;
        }
        #endregion

        public async Task<string> GetPagePath(string albumId, int pageIndex) {
            var albumPath = albumId.UriFriendlyBase64Decode();
            var allFilePaths = await Task.Run(() => _io.GetSuitableFilePaths(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1));
            var targetFile = allFilePaths[pageIndex];
            return targetFile;
        }
        #endregion

        #region COMMAND
        public async Task InsertFileToAlbum(string albumId, string subDir, string name, string base64) {
            var albumPath = albumId.UriFriendlyBase64Decode();

            var filePath = Path.Combine(_config.LibraryPath, albumPath, subDir, name);

            byte[] bytes = Convert.FromBase64String(base64);

            await _io.WriteFile(filePath, bytes);
            //Currently only used by metadata editor
            //Recounting album pageCount after each insert is unnecessary
        }

        public void DeleteFileFromAlbum(string albumId, int pageIndex) {
            var targetAlbum = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();
            var allFilePaths = _io.GetSuitableFilePaths(Path.Combine(_config.LibraryPath, targetAlbum.Path), _ai.SuitableFileFormats, 1);
            var targetFilePath = allFilePaths[pageIndex];
            _io.DeleteFile(targetFilePath);

            targetAlbum.PageCount -= 1;
            targetAlbum.LastPageIndex = pageIndex < allFilePaths.Count ? pageIndex : pageIndex - 1;

            _db.SaveChanges();
        }

        //currently does not support moving to subDir
        public void MoveFile(string albumId, int pageIndex, string newAlbumId) {
            var currentAlbum = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();

            var allFilePaths = _io.GetSuitableFilePaths(Path.Combine(_config.LibraryPath, currentAlbum.Path), _ai.SuitableFileFormats, 1);
            var currentFilePath = allFilePaths[pageIndex];
            var fileName = Path.GetFileName(currentFilePath);

            var targetAlbum = _db.AlbumVMs.Where(a => a.AlbumId == newAlbumId).FirstOrDefault();

            var newPath = Path.Combine(_config.LibraryPath, targetAlbum.Path, fileName);

            _io.MoveFile(currentFilePath, newPath);

            currentAlbum.PageCount -= 1;
            targetAlbum.PageCount += 1;
            _db.SaveChanges();
        }

        public FileInfoModel RenameFile(string albumId, int pageIndex, string newName) {
            var libRelAlbumPath = albumId.UriFriendlyBase64Decode();
            var fullAlbumPath = Path.Combine(_config.LibraryPath, libRelAlbumPath);

            var allFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);
            var currentFilePath = allFilePaths[pageIndex];
            var newFilePath = Path.Combine(Path.GetDirectoryName(currentFilePath), newName);

            _io.MoveFile(currentFilePath, newFilePath);

            var result = GenerateFileInfo(libRelAlbumPath, newFilePath);
            return result;
        }
        #endregion
    }
}
