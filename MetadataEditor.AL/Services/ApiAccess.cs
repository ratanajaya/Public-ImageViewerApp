using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace MetadataEditor.AL
{
    public interface IApiAccess
    {
        Task<string> PostAlbum(string originalFolder, Album albumVm);
        Task<string> PostFileToAlbum(string albumId, string subDir, string name, string base64);
        Task<int> RecountPageCount(string albumId);
    }

    public class ApiAccess : IApiAccess
    {
        ConfigurationModel _config;

        public ApiAccess(ConfigurationModel config) {
            _config = config;
        }

        public async Task<string> PostAlbum(string originalFolder, Album albumVm) {
            dynamic param = new ExpandoObject();
            param.OriginalFolderName = originalFolder;
            param.Album = albumVm;

            StringContent content = new StringContent(JsonSerializer.ToJsonString(param), Encoding.UTF8, "application/json");
            using(var httpClient = new HttpClient()) {
                var response = await httpClient.PostAsync(_config.ApiBaseUrl + "/Crud/InsertAlbum", content);
                string result = "";
                if(response.IsSuccessStatusCode) {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    result = reader.ReadToEnd();
                }

                return result;
            }
        }

        public async Task<string> PostFileToAlbum(string albumId, string subDir, string name, string base64) {
            dynamic param = new ExpandoObject();
            param.AlbumId = albumId;
            param.SubDir = subDir;
            param.Name = name;
            param.FileBase64 = base64;

            StringContent content = new StringContent(JsonSerializer.ToJsonString(param), Encoding.UTF8, "application/json");

            using(var httpClient = new HttpClient()) {
                string apiBaseUrl = _config.ApiBaseUrl;
                var response = await httpClient.PostAsync(apiBaseUrl + "/File/UploadFile", content);
                string result = "";
                if(response.IsSuccessStatusCode) {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    result = reader.ReadToEnd();
                }
                else {
                    throw new Exception("File Upload Failed");
                }

                return result;
            }
        }

        public async Task<int> RecountPageCount(string albumId) {
            //dynamic param = new ExpandoObject();
            //param.AlbumId = albumId;

            //StringContent content = new StringContent(JsonSerializer.ToJsonString(albumId), Encoding.UTF8, "application/json");

            using(var httpClient = new HttpClient()) {
                string apiBaseUrl = _config.ApiBaseUrl;
                var response = await httpClient.GetAsync(apiBaseUrl + "/Crud/RecountAlbumPages/" + albumId);
                int result = 0;
                if(response.IsSuccessStatusCode) {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    result = int.Parse(reader.ReadToEnd());
                }
                else {
                    throw new Exception("File Upload Failed");
                }

                return result;
            }
        }
    }
}
