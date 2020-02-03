using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharedLibrary;
using SharedLibrary.Helpers;
using MetadataEditor.DAL;
using System.Threading.Tasks;
using NaturalSort.Extension;

namespace MetadataEditor.AL
{
    public class AppLogic : IAppLogic
    {
        IAlbumInfoProvider ai;
        IFileSystemAccess fs;
        public AppLogic(IAlbumInfoProvider albumInfo, IFileSystemAccess fileSystemAccess) {
            ai = albumInfo;
            fs = fileSystemAccess;
        }

        public async Task<AlbumViewModel> GetAlbumViewModelAsync(string path) {
            AlbumViewModel result = new AlbumViewModel();

            if (fs.FileExist(Path.Combine(path, ai.JsonFileName))) {
                result.Album = fs.DeserializeAlbum(Path.Combine(path, ai.JsonFileName));
            }
            else {
                result.Album = await CreateStarterAlbumAsync(path);
            }
            result.Path = path;
            result.AlbumFiles = await GetAllFilesAsyync(path);
            
            return result;
        }

        public string[] GetTags() {
            return ai.Tags;
        }

        public string[] GetLanguages() {
            return ai.Languages;
        }

        public string[] GetCategories() {
            return ai.Categories;
        }

        public string[] GetOrientations() {
            return ai.Orientations;
        }

        public async Task<string> SaveAlbumJson(AlbumViewModel vm) {
            string result = "";

            try {
                bool retval = await Task.Run(() => fs.SerializeAlbum(Path.Combine(vm.Path, ai.JsonFileName), vm.Album));

                result = "Success";
            }
            catch (Exception e) {
                result = "Failed | " + e.ToString();
            }

            return result;
        }

        async Task<Album> CreateStarterAlbumAsync(string path) {
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
                Category = "Manga",
                Orientation = "Portrait",

                Artists = artists,
                Tags = new List<string>(),
                Languages = languages,
                Flags = new List<string>(),

                Tier = 0,
                Cover = cover,

                IsWip = isWip,
                IsRead = true,

                EntryDate = DateTime.Now
            };

            return result;
        }

        //Only suitable files
        private async Task<List<string>> GetAllFilesAsyync(string path) {
            List<string> result = new List<string>();

            string[] files = await Task.Run(() => fs.GetFiles(path));
            files = files.OrderBy(s => s, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToArray();
            //Array.Sort(files, new AlphanumComparatorFast());

            foreach (string file in files) {
                if (file.ContainsAny(ai.SuitableImageFormats)) {
                    result.Add(file);
                }
            }
            string[] subDirs = await Task.Run(() => fs.GetDirectories(path));
            foreach (string subDir in subDirs) {
                result.AddRange(await GetAllFilesAsyync(subDir));
            }

            return result;
        }

        //Get first image in the directory of SUITABLE FILES type. If not found search in subdirectories
        public async Task<string> GetCover(string path) {
            string albumPath = path;//Used to get relative path
            return await GetCoverDirty(path, albumPath);
        }
        async Task<string> GetCoverDirty(string path, string albumPath) {
            string[] files = await Task.Run(() => fs.GetFiles(path));
            foreach (string file in files) {
                if (file.ContainsAny(ai.SuitableImageFormats)) {
                    string subPath = file.Replace(albumPath + "\\", "").Replace("\\", "/"); //Forward slash for android filesystem
                    return subPath;
                }
            }
            string[] subDirs = await Task.Run(() => fs.GetDirectories(path));
            foreach (string subDir in subDirs) {
                return await GetCoverDirty(subDir, albumPath);
            }
            return "";
        }

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
