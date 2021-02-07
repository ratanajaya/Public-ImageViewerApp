using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharedLibrary;
using System.Threading.Tasks;
using NaturalSort.Extension;

namespace MetadataEditor.AL
{
    public interface IAppLogic
    {
        Task<AlbumViewModel> GetAlbumViewModelAsync(string path, Album oldAlbum);
        Task<string> SaveAlbumJson(AlbumViewModel vm);
        Task<string> PostAlbumMetadata(AlbumViewModel vm);
        Task<string> PostAlbumJson(AlbumViewModel vm, IProgress<FileDisplayModel> progress);

        string[] GetTags();
        string[] GetLanguages();
        string[] GetCategories();
        string[] GetOrientations();
    }

    public class AppLogic : IAppLogic
    {
        IAlbumInfoProvider _ai;
        IFileSystemAccess _fs;
        IApiAccess _api;

        public AppLogic(IAlbumInfoProvider albumInfo, IFileSystemAccess fileSystemAccess, IApiAccess apiAccess) {
            _ai = albumInfo;
            _fs = fileSystemAccess;
            _api = apiAccess;
        }

        #region AlbumInfo
        public string[] GetTags() {
            return _ai.Tags;
        }

        public string[] GetLanguages() {
            return _ai.Languages;
        }

        public string[] GetCategories() {
            return _ai.Categories;
        }

        public string[] GetOrientations() {
            return _ai.Orientations;
        }
        #endregion

        #region QUERY
        public async Task<AlbumViewModel> GetAlbumViewModelAsync(string path, Album oldAlbum) {
            AlbumViewModel result = new AlbumViewModel();

            if (_fs.FileExist(Path.Combine(path, _ai.JsonFileName))) {
                result.Album = _fs.DeserializeAlbum(Path.Combine(path, _ai.JsonFileName));
            }
            else {
                result.Album = await CreateStarterAlbumAsync(path, oldAlbum);
            }
            result.Path = path;
            result.AlbumFiles = await GetAllFilesAsync(path);
            
            return result;
        }

        async Task<Album> CreateStarterAlbumAsync(string path, Album oldAlbum = null) {
            var cover = await GetCover(path);
            string folderName = path.Split('\\').Last();
            string[] elements = folderName.Split(new char[] { '[', ']', '(', ')', '=' });
            var title = elements.Length >= 3 ? elements[2].Trim() : "";
            var artists = elements.Length >= 3 ? (from a in elements[1].Split(',') select a.Trim()).ToList() : new List<string>();
            var languages = elements.ContainsContains("eng") ? new List<string> { "English" } :
                            elements.ContainsContains("chinese") ? new List<string> { "Chinese" } :
                            new List<string>();
            bool isWip = elements.ContainsContains("wip") ? true : elements.ContainsContains("ongoing") ? true : false;

            Album result = new Album() {
                Title = title,
                Category = !string.IsNullOrEmpty(oldAlbum?.Category) ? oldAlbum.Category : "Manga",
                Orientation = !string.IsNullOrEmpty(oldAlbum?.Orientation) ? oldAlbum.Orientation : "Portrait",

                Artists = artists,
                Tags = oldAlbum?.Tags != null ? oldAlbum.Tags : new List<string>(),
                Languages = languages,
                Flags = new List<string>(),

                Tier = 0,
                //Cover = cover,

                IsWip = isWip,
                IsRead = false,

                EntryDate = DateTime.Now
            };

            return result;
        }

        //Only suitable files
        private async Task<List<string>> GetAllFilesAsync(string path) {
            List<string> result = new List<string>();

            string[] files = await Task.Run(() => _fs.GetFiles(path));
            files = files.OrderBy(s => s, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToArray();
            //Array.Sort(files, new AlphanumComparatorFast());

            foreach(string file in files) {
                if(file.ContainsAny(_ai.SuitableFileFormats)) {
                    result.Add(file);
                }
            }
            string[] subDirs = await Task.Run(() => _fs.GetDirectories(path));
            foreach(string subDir in subDirs) {
                result.AddRange(await GetAllFilesAsync(subDir));
            }

            return result;
        }

        //Get first image in the directory of SUITABLE FILES type. If not found search in subdirectories
        public async Task<string> GetCover(string path) {
            string albumPath = path;//Used to get relative path
            return await GetCoverDirty(path, albumPath);
        }
        async Task<string> GetCoverDirty(string path, string albumPath) {
            string[] files = await Task.Run(() => _fs.GetFiles(path));
            foreach(string file in files) {
                if(file.ContainsAny(_ai.SuitableFileFormats)) {
                    string subPath = file.Replace(albumPath + "\\", "").Replace("\\", "/"); //Forward slash for android filesystem
                    return subPath;
                }
            }
            string[] subDirs = await Task.Run(() => _fs.GetDirectories(path));
            foreach(string subDir in subDirs) {
                return await GetCoverDirty(subDir, albumPath);
            }
            return "";
        }
        #endregion

        #region COMMAND
        public async Task<string> SaveAlbumJson(AlbumViewModel vm) {
            string result = "";

            try {
                bool retval = await Task.Run(() => _fs.SerializeAlbum(Path.Combine(vm.Path, _ai.JsonFileName), vm.Album));

                result = "Success";
            }
            catch (Exception e) {
                result = "Failed | " + e.ToString();
            }

            return result;
        }

        public async Task<string> PostAlbumMetadata(AlbumViewModel vm) {
            string originalFolder = new DirectoryInfo(vm.Path).Name;
            string albumId = await _api.PostAlbum(originalFolder, vm.Album);

            return albumId;
        }

        public async Task<string> PostAlbumJson(AlbumViewModel vm, IProgress<FileDisplayModel> progress) {
            string result = "";

            string originalFolder = new DirectoryInfo(vm.Path).Name;
            try {
                string albumId = await _api.PostAlbum(originalFolder, vm.Album);
                foreach(string filePath in vm.AlbumFiles) {
                    var reportModel = new FileDisplayModel {
                        Path = filePath,
                        UploadStatus = "Uploading.."
                    };
                    progress.Report(reportModel);
                    //await Task.Delay(200);
                    try {
                        string fileBase64 = _fs.ReadFileBase64(filePath);
                        string fileName = Path.GetFileName(filePath);
                        string subDir = filePath.Replace(vm.Path, "").Replace(fileName, "").Replace("\\", "");

                        string retval = await _api.PostFileToAlbum(albumId, subDir, fileName, fileBase64);
                        reportModel.UploadStatus = retval;
                    }
                    catch(Exception e) {
                        reportModel.UploadStatus = e.Message;
                    }

                    progress.Report(reportModel);
                }
                int pageCount = await _api.RecountPageCount(albumId);

                result = albumId;
            }
            catch(Exception e) {
                result = "Failed | " + e.ToString();
            }

            return result;
        }

        #endregion


        #region OBSOLETE Create 256px thumbnail
        //Bitmap cover256 = ResizeImage(new Bitmap(vm.GetCoverPath()), 256);
        //cover256.Save(vm.Path + "\\#metadata\\#cover.jpg", ImageFormat.Jpeg);
        //Bitmap ResizeImage(Image image, int maxRes) {
        //    int width, height;

        //    if (image.Width > image.Height) {
        //        width = maxRes;
        //        height = Convert.ToInt32(maxRes * ((float)image.Height / (float)image.Width));
        //    }
        //    else {
        //        width = Convert.ToInt32(maxRes * ((float)image.Width / (float)image.Height));
        //        height = maxRes;
        //    }

        //    Rectangle destRect = new Rectangle(0, 0, width, height);
        //    Bitmap destImage = new Bitmap(width, height);

        //    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        //    using (var graphics = Graphics.FromImage(destImage)) {
        //        graphics.CompositingMode = CompositingMode.SourceCopy;
        //        graphics.CompositingQuality = CompositingQuality.HighQuality;
        //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphics.SmoothingMode = SmoothingMode.HighQuality;
        //        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        using (var wrapMode = new ImageAttributes()) {
        //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        //            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        //        }
        //    }

        //    return destImage;
        //}
        #endregion
    }
}
