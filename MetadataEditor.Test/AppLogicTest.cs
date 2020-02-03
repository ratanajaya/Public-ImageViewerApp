using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using MetadataEditor.AL;
using MetadataEditor.DAL;
using SharedLibrary;
using System.IO;

namespace MetadataEditor.Test
{
    public class AppLogicTest
    {
        [Fact]
        public void OnePlusOneEqualTwo() {
            int a = 1 + 1;
            Assert.Equal(2,a);
        }

        [Fact]
        public async void GetAlbumViewModel_GeneratedAlbum_1() {
            string path = "D:\\TestLibrary\\[MyArtist, Colab] Don't have json [English][OngoinG]";
            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.FileExist(It.IsAny<string>())).Returns(false);
            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

            var al = new AppLogic(aiMock.Object, fsmock.Object);
            var retval = await al.GetAlbumViewModelAsync(path);

            Assert.Equal("MyArtist", retval.Album.Artists[0]);
            Assert.Equal("Colab", retval.Album.Artists[1]);
            Assert.Equal("Don't have json", retval.Album.Title);
            Assert.Equal("English", retval.Album.Languages[0]);
            Assert.True(retval.Album.IsWip);
        }

        [Fact]
        public async void GetAlbumViewModel_GeneratedAlbum_2() {
            string path = "D:\\TestLibrary\\[MyArtist] 4検さ読設レ検先ルれ";
            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.FileExist(It.IsAny<string>())).Returns(false);
            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

            var al = new AppLogic(aiMock.Object, fsmock.Object);
            var retval = await al.GetAlbumViewModelAsync(path);

            Assert.Equal("MyArtist", retval.Album.Artists[0]);
            Assert.Equal("4検さ読設レ検先ルれ", retval.Album.Title);
            Assert.Equal(0, retval.Album.Languages.Count);
            Assert.False(retval.Album.IsWip);
        }

        [Theory]
        [InlineData("D:\\TestLibrary\\[MyArtist] My Title Vol.1 [English]", "image.jpg")]
        [InlineData("D:\\TestLibrary\\SecondAlbum", "Ch. 1 - Title/01.png")]
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
            aiMock.Setup(ai => ai.SuitableImageFormats).Returns(new string[] { ".jpg", ".png" });

            var al = new AppLogic(aiMock.Object, fsmock.Object);
            string retval = await al.GetCover(path);

            Assert.Equal(expected, retval);
        }

        [Theory]
        [MemberData("SaveAlbumJsonIndex", MemberType = typeof(AppLogicTestCase))]
        public async void SaveAlbumJson_ReturnValueWhenSucceedAndFailed(int i) {
            var input = AppLogicTestCase.SaveAlbumJsonTestCase[i];
            var vm = (AlbumViewModel)input[0];
            string expected = (string)input[1];

            var fsmock = new Mock<IFileSystemAccess>();
            fsmock.Setup(fs => fs.SerializeAlbum("D:\\MyLib\\[Artist]My Title\\_album.json", null));
            fsmock.Setup(fs => fs.SerializeAlbum("wololo\\_album.json", null)).Throws(new Exception());
            var aiMock = new Mock<IAlbumInfoProvider>();
            aiMock.Setup(ai => ai.JsonFileName).Returns("_album.json");

            var al = new AppLogic(aiMock.Object, fsmock.Object);

            string retval = await al.SaveAlbumJson(vm);
            Assert.Contains(expected, retval);
        }
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
                for(int i = 0; i< SaveAlbumJsonTestCase.Count; i++) {
                    result.Add(new object[] { i });
                }
                return result;
            }
        }
    }
}
