using MetadataEditor.AL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharedLibrary;
using System;
using System.Collections.Generic;

namespace MetadataEditor.Test
{
    [TestClass]
    public class AppLogicTest
    {
        [TestMethod]
        public void OnePlusOneEqualTwo() {
            int a = 1 + 1;
            Assert.AreEqual(2, a);
        }

        [TestMethod]
        public async void GetAlbumViewModel_GeneratedAlbum_1() {
            string path = "D:\\TestLibrary\\[MyArtist, Colab] Don't have json [English][OngoinG]";

            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.FileExist(It.IsAny<string>())).Returns(false);

            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

            var apiMock = new Mock<IApiAccess>();

            var al = new AppLogic(aiMock.Object, fsmock.Object, apiMock.Object);
            var retval = await al.GetAlbumViewModelAsync(path, null);

            Assert.AreEqual("MyArtist", retval.Album.Artists[0]);
            Assert.AreEqual("Colab", retval.Album.Artists[1]);
            Assert.AreEqual("Don't have json", retval.Album.Title);
            Assert.AreEqual("English", retval.Album.Languages[0]);
            Assert.IsTrue(retval.Album.IsWip);
        }

        [TestMethod]
        public async void GetAlbumViewModel_GeneratedAlbum_2() {
            string path = "D:\\TestLibrary\\[MyArtist] 4検さ読設レ検先ルれ";

            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.FileExist(It.IsAny<string>())).Returns(false);

            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

            var apiMock = new Mock<IApiAccess>();

            var al = new AppLogic(aiMock.Object, fsmock.Object, apiMock.Object);
            var retval = await al.GetAlbumViewModelAsync(path, null);

            Assert.AreEqual("MyArtist", retval.Album.Artists[0]);
            Assert.AreEqual("4検さ読設レ検先ルれ", retval.Album.Title);
            Assert.AreEqual(0, retval.Album.Languages.Count);
            Assert.IsFalse(retval.Album.IsWip);
        }

        [TestMethod]
        [TestProperty("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]", "image.jpg")]
        [TestProperty("D:\\TestLibrary\\SecondAlbum", "Ch. 1 - Title/01.png")]
        public async void GetCover_CorrectCover(string path, string expected) {
            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]")).Returns(new string[] { "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 1 - Title", "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 2" });
            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 1 - Title")).Returns(new string[] { });
            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 2")).Returns(new string[] { });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]")).Returns(new string[] { "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\aaa.txt", "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\image.jpg" });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 1 - Title")).Returns(new string[] { "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\01.png" });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\Ch. 2")).Returns(new string[] { "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\01.jpg", "D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]\\02.jpg" });

            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\SecondAlbum")).Returns(new string[] { "D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title", "D:\\TestLibrary\\SecondAlbum\\Ch. 2" });
            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title")).Returns(new string[] { });
            fsmock.Setup(fs => fs.GetDirectories("D:\\TestLibrary\\SecondAlbum\\Ch. 2")).Returns(new string[] { });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\SecondAlbum")).Returns(new string[] { "D:\\TestLibrary\\SecondAlbum\\aaa.txt" });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title")).Returns(new string[] { "D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title\\01.png" });
            fsmock.Setup(fs => fs.GetFiles("D:\\TestLibrary\\SecondAlbum\\Ch. 2")).Returns(new string[] { "D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title\\01.jpg", "D:\\TestLibrary\\SecondAlbum\\Ch. 1 - Title\\02.jpg" });

            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.SuitableFileFormats).Returns(new string[] { ".jpg", ".png" });

            var apiMock = new Mock<IApiAccess>();

            var al = new AppLogic(aiMock.Object, fsmock.Object, apiMock.Object);
            string retval = await al.GetCover(path);

            Assert.AreEqual(expected, retval);
        }

        //[TestMethod]
        //[MemberData("SaveAlbumJsonIndex", MemberType = typeof(AppLogicTestCase))]
        //public async void SaveAlbumJson_ReturnValueWhenSucceedAndFailed(int i) {
        //    var input = AppLogicTestCase.SaveAlbumJsonTestCase[i];
        //    var vm = (AlbumViewModel)input[0];
        //    string expected = (string)input[1];

        //    var fsmock = new Mock<IFileSystemAccess>();
        //    fsmock.Setup(fs => fs.SerializeAlbum("D:\\MyLib\\[Artist]My Title\\_album.json", null));
        //    fsmock.Setup(fs => fs.SerializeAlbum("wololo\\_album.json", null)).Throws(new Exception());
        //    var aiMock = new Mock<IAlbumInfoProvider>();
        //    aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

        //    var apiMock = new Mock<IApiAccess>();

        //    var al = new AppLogic(aiMock.Object, fsmock.Object, apiMock.Object);

        //    string retval = await al.SaveAlbumJson(vm);
        //    Assert.Contains(expected, retval);
        //}
    }

    class AppLogicTestCase
    {
        public static readonly List<object[]> SaveAlbumJsonTestCase = new List<object[]> {
            new object[]{new AlbumViewModel { Path = "D:\\MyLib\\[Artist]My Title" }, "Success" },
            new object[]{new AlbumViewModel { Path = "wololo" }, "Failed" }
        };

        public static IEnumerable<object[]> SaveAlbumJsonIndex {
            get {
                List<object[]> result = new List<object[]>();
                for(int i = 0; i < SaveAlbumJsonTestCase.Count; i++) {
                    result.Add(new object[] { i });
                }
                return result;
            }
        }
    }
}
