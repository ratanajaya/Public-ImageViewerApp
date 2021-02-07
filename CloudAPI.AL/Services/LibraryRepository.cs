using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using Microsoft.Extensions.Configuration;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.AL.Services
{
    public class LibraryRepository {
        ConfigurationModel _config;
        IAlbumInfoProvider _ai;
        ISystemIOAbstraction _io;
        IDbContext _db;

        StringComparison _comparer = StringComparison.OrdinalIgnoreCase;

        public LibraryRepository(ConfigurationModel config, IAlbumInfoProvider ai, IDbContext db, ISystemIOAbstraction io) {
            _config = config;
            _ai = ai;
            _io = io;
            _db = db;
        }

        #region Query
        readonly char[] _validOperator = new char[] { ':', '!', '>', '<'};
        bool MatchAllQueries(Album album, string[] querySegs) {
            bool IsMatch(Album album, string querySeg) {
                int connectorIndex = querySeg.IndexOfAny(_validOperator);
                var connector = querySeg[connectorIndex];
                var key = querySeg.Substring(0, connectorIndex);
                var val = querySeg.Substring(connectorIndex + 1, querySeg.Length - (connectorIndex + 1));

                if(key.Equals("tag", _comparer)) {
                    var isContains = new Func<bool>(() => {
                        if(val.IndexOf('|') != -1) {
                            var vals = val.Split('|');
                            return album.Tags.Any(tag => vals.Contains(tag, _comparer));
                        }
                        return album.Tags.Contains(val, _comparer);
                    })();

                    return connector == ':' ? isContains : !isContains;
                }
                else if(key.Equals("artist", _comparer)) {
                    return album.Artists.Any(artist => artist.Contains(val, _comparer));
                }
                else if(key.Equals("title", _comparer)) {
                    return album.Title.Contains(val, _comparer);
                }
                else if(key.Equals("category", _comparer)) {
                    return album.Category.Equals(val, _comparer);
                }
                else if(key.Equals("orientation", _comparer)) {
                    return album.Orientation.Equals(val, _comparer);
                }
                else if(key.Equals("language", _comparer)) {
                    var isContains = new Func<bool>(() => {
                        if(val.IndexOf('|') != -1) {
                            var vals = val.Split('|');
                            return album.Languages.Any(lan => vals.Contains(lan, _comparer));
                        }
                        return album.Languages.Contains(val, _comparer);
                    })();

                    return connector == ':' ? isContains : !isContains;
                }
                else if(key.Equals("flag", _comparer)) {
                    return album.Flags.Contains(val, _comparer);
                }
                else if(key.Equals("tier", _comparer)) {
                    int iVal = int.Parse(val);
                    return connector == ':' ? album.Tier == iVal :
                            connector == '>' ? album.Tier > iVal :
                            connector == '<' ? album.Tier < iVal :
                            connector == '!' ? album.Tier != iVal :
                            false;
                }
                else if(key.Equals("IsWip", _comparer)) {
                    bool bvalue = bool.Parse(val);
                    bool isEqual = album.IsWip == bvalue;
                    return connector == ':' ? isEqual : !isEqual;
                }
                else if(key.Equals("IsRead", _comparer)) {
                    bool bvalue = bool.Parse(val);
                    bool isEqual = album.IsRead == bvalue;
                    return connector == ':' ? isEqual : !isEqual;
                }
                else if(key.Equals("EntryDate", _comparer)) {
                    DateTime dVal = DateTime.Parse(val);
                    return connector == ':' ? album.EntryDate == dVal :
                            connector == '>' ? album.EntryDate > dVal :
                            connector == '<' ? album.EntryDate < dVal :
                            connector == '!' ? album.EntryDate != dVal :
                            false;
                }
                else if(key.Equals("Special", _comparer)) {
                    if(val.Equals("Tier>0OrNew", _comparer)) {
                        return album.Tier > 0 || !album.IsRead;
                    }
                }

                throw new Exception("Invalid key: " + key);
            }

            foreach(string querySeg in querySegs) {
                if(!IsMatch(album, querySeg)) {
                    return false;
                }
            }
            return true;
        }
        public IEnumerable<AlbumVM> GetAlbumVMs(int page, int row, string query) {
            var querySegments = !string.IsNullOrEmpty(query) ? query.Split(',') : new string[] { };
            var filteredAlbum = _db.AlbumVMs.Where(a => MatchAllQueries(a.Album, querySegments));
            var pagedAlbum = (page > 0 && row > 0) ? filteredAlbum.Skip((page - 1) * row).Take(row) : filteredAlbum;

            return pagedAlbum;

            #region LEGACY
            //var albumQ = _db.AlbumVMs;
            //if(!string.IsNullOrWhiteSpace(query)) {
            //    foreach(string s in query.Split(',')) {
            //        char connector = s.Contains(":") ? ':' :
            //                         s.Contains("!") ? '!' :
            //                         s.Contains(">") ? '>' :
            //                         s.Contains("<") ? '<' :
            //                         throw new Exception("Invalid search query");
            //        string key = s.Split(connector)[0];
            //        string val = s.Split(connector)[1];

            //        if(key.Equals("title", _comparer)) {
            //            albumQ = albumQ.Where(a => a.Album.GetFullTitleDisplay().Contains(val, _comparer));
            //        }
            //        else if(key.Equals("category", _comparer)) {
            //            albumQ = albumQ.Where(a => a.Album.Category.Equals(val, _comparer));
            //        }
            //        else if(key.Equals("orientation", _comparer)) {
            //            albumQ = albumQ.Where(a => a.Album.Orientation.Equals(val, _comparer));
            //        }
            //        else if(key.Equals("artist", _comparer)) {
            //            albumQ = albumQ.Where(a => a.Album.Artists.ContainsContains(val));
            //        }
            //        else if(key.Equals("tag", _comparer)) {
            //            string[] orvalues = val.Split('|');
            //            albumQ = connector == ':' ?
            //                        (orvalues.Length == 2 ? albumQ.Where(a => a.Album.Tags.ContainsExact(orvalues[0], _comparer) ||
            //                                                                 a.Album.Tags.ContainsExact(orvalues[1], _comparer)) :
            //                        orvalues.Length == 1 ? albumQ.Where(a => a.Album.Tags.ContainsExact(val, _comparer)) :
            //                        albumQ) :
            //                     connector == '!' ?
            //                        (orvalues.Length == 2 ? albumQ.Where(a => !a.Album.Tags.ContainsExact(orvalues[0], _comparer) ||
            //                                                                     !a.Album.Tags.ContainsExact(orvalues[1], _comparer)) :
            //                        orvalues.Length == 1 ? albumQ.Where(a => !a.Album.Tags.ContainsExact(val, _comparer)) :
            //                        albumQ) :
            //                     albumQ;
            //        }
            //        else if(key.Equals("language", _comparer)) {
            //            string[] orvalues = val.Split('|');
            //            albumQ = connector == ':' ?
            //                        (orvalues.Length == 2 ? albumQ.Where(a => a.Album.Languages.ContainsExact(orvalues[0], _comparer) ||
            //                                                a.Album.Languages.ContainsExact(orvalues[1], _comparer)) :
            //                         orvalues.Length == 1 ? albumQ.Where(a => a.Album.Languages.ContainsExact(val, _comparer)) :
            //                         albumQ) :
            //                     connector == '!' ?
            //                        (orvalues.Length == 2 ? albumQ.Where(a => !a.Album.Languages.ContainsExact(orvalues[0], _comparer) ||
            //                                                                    !a.Album.Languages.ContainsExact(orvalues[1], _comparer)) :
            //                         orvalues.Length == 1 ? albumQ.Where(a => !a.Album.Languages.ContainsExact(val, _comparer)) :
            //                         albumQ) :
            //                     albumQ;
            //        }
            //        else if(key.Equals("flag", _comparer)) {
            //            albumQ = albumQ.Where(a => a.Album.Flags.Contains(val, _comparer));
            //        }
            //        else if(key.Equals("tier", _comparer)) {
            //            int ivalue = int.Parse(val);
            //            albumQ = connector == ':' ? albumQ.Where(a => a.Album.Tier == ivalue) :
            //                     connector == '>' ? albumQ.Where(a => a.Album.Tier > ivalue) :
            //                     connector == '<' ? albumQ.Where(a => a.Album.Tier < ivalue) :
            //                     connector == '!' ? albumQ.Where(a => a.Album.Tier != ivalue) :
            //                     albumQ;
            //        }
            //        else if(key.Equals("iswip", _comparer)) {
            //            bool bvalue = bool.Parse(val);
            //            albumQ = connector == ':' ? albumQ.Where(a => a.Album.IsWip == bvalue) :
            //                     connector == '!' ? albumQ.Where(a => a.Album.IsWip != bvalue) :
            //                     albumQ;
            //        }
            //        else if(key.Equals("isread", _comparer)) {
            //            bool bvalue = bool.Parse(val);
            //            albumQ = connector == ':' ? albumQ.Where(a => a.Album.IsRead == bvalue) :
            //                     connector == '!' ? albumQ.Where(a => a.Album.IsRead != bvalue) :
            //                     albumQ;
            //        }
            //        else if(key.Equals("date", _comparer)) {
            //            DateTime dvalue = DateTime.Parse(val);
            //            albumQ = connector == ':' ? albumQ = albumQ.Where(a => a.Album.EntryDate == dvalue) :
            //                     connector == '!' ? albumQ = albumQ.Where(a => a.Album.EntryDate != dvalue) :
            //                     connector == '>' ? albumQ = albumQ.Where(a => a.Album.EntryDate > dvalue) :
            //                     connector == '<' ? albumQ = albumQ.Where(a => a.Album.EntryDate < dvalue) :
            //                     albumQ;
            //        }
            //    }
            //}

            //if(page > 0 && row > 0) {
            //    albumQ = albumQ.Skip((page - 1) * row).Take(row);
            //}

            //return albumQ;
            #endregion
        }

        public List<ChapterVM> GetAlbumChapters(string albumId) {
            var result = new List<ChapterVM>();

            //var targetAlbum = _db.AlbumVMs.FirstOrDefault(a => a.AlbumId == albumId);
            var albumPath = albumId.UriFriendlyBase64Decode();

            var rootFiles = _io.GetSuitableFilePathsWithNaturalSort(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 0).ToList();
            var subDirs = _io.GetDirectories(Path.Combine(_config.LibraryPath, albumPath));
            int previousCumulativePageCount = rootFiles.Count;
            foreach(string subDir in subDirs) {
                var subDirFiles = _io.GetSuitableFilePathsWithNaturalSort(subDir, _ai.SuitableFileFormats, 0).ToList();
                int subDirFileCount = subDirFiles.Count;

                if(subDirFileCount > 0) {
                    var firstFileFullPath = subDirFiles[0];
                    var firstFileLibRelPath = Path.GetRelativePath(_config.LibraryPath, firstFileFullPath);

                    result.Add(new ChapterVM {
                        Title = new DirectoryInfo(subDir).Name,
                        PageIndex = previousCumulativePageCount,
                        PageUncPath = firstFileLibRelPath.UriFriendlyBase64Encode()
                    });

                    previousCumulativePageCount += subDirFileCount;
                }
            }

            return result;
        }
        
        public AlbumVM GetAlbumVM(string albumId) {
            var result = _db.AlbumVMs.FirstOrDefault(a => a.AlbumId == albumId);
            return result;
        }

        public IQueryable<QueryVM> GetArtistVMs(int? tier) {
            var artistQ = _db.ArtistVMs;

            var filteredArtistQ = tier.HasValue ? artistQ.Where(a => a.Tier == tier.Value) : artistQ;

            return filteredArtistQ;
        }

        public List<QueryVM> GetTagVMs() {
            var tagVMs = _ai.Tags.Select(t => new QueryVM {
                Name = t,
                Tier = 0,
                Query = "tag:" + t
            });

            var result = tagVMs.ToList();
            return result;
        }

        #endregion

        #region Command
        public string InsertAlbum(string originalFolderName, Album album) {
            #region Local Function
            string GetLibRelAlbumLocation(string firstLetter) {
                firstLetter = firstLetter.ToLower();
                string result = firstLetter == "a" ? "ABC" : firstLetter == "b" ? "ABC" : firstLetter == "c" ? "ABC" :
                                firstLetter == "d" ? "DEF" : firstLetter == "e" ? "DEF" : firstLetter == "f" ? "DEF" :
                                firstLetter == "g" ? "GHI" : firstLetter == "h" ? "GHI" : firstLetter == "i" ? "GHI" :
                                firstLetter == "j" ? "JKL" : firstLetter == "k" ? "JKL" : firstLetter == "l" ? "JKL" :
                                firstLetter == "m" ? "MNO" : firstLetter == "n" ? "MNO" : firstLetter == "o" ? "MNO" :
                                firstLetter == "p" ? "PQR" : firstLetter == "q" ? "PQR" : firstLetter == "r" ? "PQR" :
                                firstLetter == "s" ? "STU" : firstLetter == "t" ? "STU" : firstLetter == "u" ? "STU" :
                                firstLetter == "v" ? "VWXYZ" : firstLetter == "w" ? "VWXYZ" : firstLetter == "x" ? "VWXYZ" :
                                firstLetter == "y" ? "VWXYZ" : firstLetter == "z" ? "VWXYZ" :
                                "_";
                return result;
            }

            string GetFirstLetter(string source) {
                string normalizedSource = source.RemoveNonLetterDigit();
                string result = normalizedSource[0].ToString();
                return result;
            }
            #endregion

            string firstLetter = GetFirstLetter(originalFolderName);
            string libRelSubDir = GetLibRelAlbumLocation(firstLetter);
            string libRelAlbumPath = Path.Combine(libRelSubDir, originalFolderName);

            _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));
            _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, _config.AlbumMetadataFileName), album);

            var existing = _db.AlbumVMs.FirstOrDefault(a => a.Path == libRelAlbumPath);

            var albumId = new Func<string>(() => {
                if(existing == null) {
                    var newAlbum = new AlbumVM {
                        Album = album,
                        //AlbumId = albumId,
                        Path = libRelAlbumPath,
                        PageCount = 0,
                        LastPageIndex = 0,
                        //CoverInfo Generated on recount
                    };

                    _db.AddAlbumVM(newAlbum);
                    return newAlbum.AlbumId;
                }
                else {
                    existing.Album = album;
                    return existing.AlbumId;
                }
            })();

            _db.SaveChanges();

            return albumId;
        }

        public string UpdateAlbum(AlbumVM albumVM) {
            var existing = _db.AlbumVMs.Where(a => a.AlbumId == albumVM.AlbumId).FirstOrDefault();

            if(existing.Album.EntryDate != albumVM.Album.EntryDate) {
                throw new InvalidOperationException("Update on album's EntryDate is forbidden");
            }
            albumVM.Album.ValidateAndCleanup();

            existing.Album = albumVM.Album;
            //existing.AlbumId = albumVM.Album.GetId();

            _io.SerializeToJson(Path.Combine(_config.LibraryPath, existing.Path, _config.AlbumMetadataFileName), existing.Album);

            _db.SaveChanges();

            return existing.AlbumId;
        }

        public string DeleteAlbum(string albumId) {
            var existing = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();

            if(existing == null) {
                throw new InvalidOperationException("Album with specified id not found");
            }

            var albumFullPath = Path.Combine(_config.LibraryPath, existing.Path);

            _io.DeleteDirectory(albumFullPath);
            _db.RemoveAlbumVM(existing);

            _db.SaveChanges();

            return albumId;
        }

        public int DeleteAlbumChapter(string albumId, string subDir) {
            var existing = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();

            if(existing == null) {
                return existing.PageCount;
            }

            var chapterPath = Path.Combine(_config.LibraryPath, existing.Path, subDir);

            _io.DeleteDirectory(chapterPath);

            int newPageCount = RecountAlbumPages(albumId);

            return newPageCount;
        }

        public string UpdateAlbumOuterValue(string albumId, int lastPageIndex) {
            var existing = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();

            existing.LastPageIndex = lastPageIndex;
            if(!existing.Album.IsRead && lastPageIndex == existing.PageCount - 1) {
                existing.Album.IsRead = true;
                existing.LastPageIndex = 0;
                _io.SerializeToJson(Path.Combine(_config.LibraryPath, existing.Path, _config.AlbumMetadataFileName), existing.Album);
            }

            _db.SaveChanges();
            return existing.AlbumId;
        }

        public string UpdateAlbumTier(string albumId, int tier) {
            var existing = _db.AlbumVMs.Where(a => a.AlbumId == albumId).FirstOrDefault();

            existing.Album.Tier = tier;

            _io.SerializeToJson(Path.Combine(_config.LibraryPath, existing.Path, _config.AlbumMetadataFileName), existing.Album);

            _db.SaveChanges();
            return existing.AlbumId;
        }

        public int RecountAlbumPages(string albumId) {
            var targetAlbum = _db.AlbumVMs.FirstOrDefault(a => a.AlbumId == albumId);
            var fullAlbumPath = Path.Combine(_config.LibraryPath, targetAlbum.Path);

            var suitableFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);

            targetAlbum.PageCount = suitableFilePaths.Count;
            targetAlbum.CoverInfo = _db.GetFirstFileInfo(suitableFilePaths);

            _db.SaveChanges();

            return targetAlbum.PageCount;
        }

        public async Task ReloadDatabase() {
            await _db.Reload();
        }

        public async Task RescanDatabase() {
            await _db.Rescan();
        }
        #endregion
    }
}
