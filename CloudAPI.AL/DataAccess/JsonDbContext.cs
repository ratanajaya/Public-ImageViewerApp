using CloudAPI.AL.Models;
using Serilog;
using SharedLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.DataAccess
{
    public interface IDbContext
    {
        List<AlbumVM> AlbumVMs { get; }
        //IQueryable<AlbumVM> AlbumVMs { get; }
        IQueryable<QueryVM> ArtistVMs { get; }

        void AddAlbumVM(AlbumVM newEntity);
        void RemoveAlbumVM(AlbumVM existing);
        bool SaveChanges();
        Task Reload();
        Task Rescan();

        FileInfoModel GetFirstFileInfo(List<string> filePaths);
    }

    /// <summary>
    /// WARNING This IDbContext implementation may not be thread safe (untested) and doesn't guarantee ACID transaction. 
    /// If this app will ever be made public, an actual RDBMS implemnentation will be done beforehand.
    /// </summary>
    public class JsonDbContext : IDbContext
    {
        ConfigurationModel _config;
        IAlbumInfoProvider _ai;
        ISystemIOAbstraction _io;
        List<AlbumVM> _albumVMs;
        List<QueryVM> _artistVMs;
        ILogger _logger;

        public JsonDbContext(ConfigurationModel config, ILogger logger, IAlbumInfoProvider ai, ISystemIOAbstraction io) {
            _config = config;
            _logger = logger;
            _ai = ai;
            _io = io;
        }

        public bool SaveChanges() {
            _io.SerializeToMsgpack(_config.FullAlbumDbPath, _albumVMs);

            DeleteArtistVMsCache();
            _artistVMs = null;

            return true;
        }

        public List<AlbumVM> AlbumVMs { 
            get {
                if(_albumVMs == null) {
                    //_albumVMs = LoadAlbumVMs().GetAwaiter().GetResult(); //NOTE may cause deadlock
                    _albumVMs = Task.Run(() => LoadAlbumVMs()).GetAwaiter().GetResult();
                }
                return _albumVMs;
            }
        }

        public void AddAlbumVM(AlbumVM newEntity) {
            _albumVMs.Add(newEntity);
        }

        public void RemoveAlbumVM(AlbumVM existing) {
            _albumVMs.Remove(existing);
        }

        public IQueryable<QueryVM> ArtistVMs {
            get {
                if(_artistVMs == null) {
                    //_queryVMs = LoadArtistVMs().GetAwaiter().GetResult(); //NOTE may cause deadlock
                    _artistVMs = Task.Run(() => LoadArtistVMs()).GetAwaiter().GetResult();
                }
                return _artistVMs.AsQueryable();
            }
        }

        //const string ALBUM_METADATA_FILENAME = "_album.json";
        private async Task<List<AlbumVM>> LoadAlbumVMs() {
            List<AlbumVM> result = null;
            try {
                result = await _io.DeserializeMsgpack<List<AlbumVM>>(_config.FullAlbumDbPath);
            }
            catch(Exception e) {
                _logger.Error("LoadAlbumVMs() - " + e.Message);
            }

            if(result == null) {
                result = await LoadAlbumVMsRecursive(_config.LibraryPath);
                _io.SerializeToMsgpack(_config.FullAlbumDbPath, result);
            }

            return result;
        }

        private async Task<List<QueryVM>> LoadArtistVMs() {
            List<QueryVM> result = null;
            try {
                result = await _io.DeserializeMsgpack<List<QueryVM>>(_config.FullArtistDbPath);
            }
            catch(Exception e) {
                _logger.Error("LoadArtistVMs() - " + e.Message);
            }

            if(result == null) {
                //var albumVMs = await LoadAlbumVMs(); //Experimental
                var albumVMs = AlbumVMs;

                result = new List<QueryVM>();
                foreach(var albumVM in albumVMs) {
                    foreach(string artist in albumVM.Album.Artists) {
                        var existing = result.FirstOrDefault(a => a.Name == artist);
                        if(existing == null) { 
                            result.Add(new QueryVM {
                                Name = artist,
                                Tier = _ai.Tier1Artists.Contains(artist) ? 1 : 0,
                                Query = "artist:" + artist
                            });
                        }
                    }
                }

                _io.SerializeToMsgpack(_config.FullArtistDbPath, result);
            }

            return result;
        }

        private void DeleteArtistVMsCache() {
            _io.DeleteFile(_config.FullArtistDbPath);
        }

        private async Task<List<AlbumVM>> LoadAlbumVMsRecursive(string folderPath) {
            var result = new List<AlbumVM>();

            if(_io.IsFileExists(Path.Combine(folderPath, _config.AlbumMetadataFileName))) {
                var album = await _io.DeserializeJson<Album>(Path.Combine(folderPath, _config.AlbumMetadataFileName));

                var suitableFilePaths = _io.GetSuitableFilePaths(folderPath, _ai.SuitableFileFormats, 1);

                var coverInfo = GetFirstFileInfo(suitableFilePaths);

                var albumVM = new AlbumVM {
                    Path = Path.GetRelativePath(_config.LibraryPath, folderPath),
                    PageCount = suitableFilePaths.Count,
                    LastPageIndex = 0,
                    CoverInfo = coverInfo,
                    Album = album
                };
                result.Add(albumVM);
            }

            string[] subDirs = _io.GetDirectories(folderPath);
            foreach(string subDir in subDirs) {
                result.AddRange(await LoadAlbumVMsRecursive(subDir));
            }

            return result;
        }

        public FileInfoModel GetFirstFileInfo(List<string> filePaths) {
            var coverPath = filePaths.FirstOrDefault();

            var coverLibRelUncPath = new Func<string>(() => {
                if(coverPath != null) {
                    var libRelCoverPath = Path.GetRelativePath(_config.LibraryPath, coverPath);
                    var result = libRelCoverPath.UriFriendlyBase64Encode();
                    return result;
                }
                return null;
            })();

            var fileInfo = coverPath != null ? new FileInfo(coverPath) : null;

            var result = new FileInfoModel {
                Name = fileInfo?.Name,
                Extension = fileInfo?.Extension,
                Size = fileInfo?.Length ?? 0,
                UncPathEncoded = coverLibRelUncPath
            };

            return result;
        }


        public async Task Reload() {
            _albumVMs = await LoadAlbumVMs();
            _artistVMs = await LoadArtistVMs();
        }

        public async Task Rescan() {
            _albumVMs = null;
            _artistVMs = null;

            _io.DeleteFile(_config.FullAlbumDbPath);
            _io.DeleteFile(_config.FullArtistDbPath);

            _albumVMs = await LoadAlbumVMs();
            _artistVMs = await LoadArtistVMs();
        }
    }
}
