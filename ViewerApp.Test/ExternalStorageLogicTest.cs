using System;
using System.Collections.Generic;
using System.Text;
using ViewerApp.DAL;
using Xunit;
using Moq;
using SharedLibrary;
using System.IO;

namespace ViewerApp.Test
{
    public class ExternalStorageLogicTest
    {
        [Fact]
        public static void Initialize_LibraryPathInSubDir() {
            var fam = new Mock<IFileSystemAccess>();
            fam.Setup(fa => fa.GetDirectories("storage")).Returns(new string[] { "storage\\dir1","storage\\dir2" });
            fam.Setup(fa => fa.GetDirectories("storage\\dir1")).Returns(new string[] { "storage\\dir1\\NotHere" });
            fam.Setup(fa => fa.GetDirectories("storage\\dir2")).Returns(new string[] { "storage\\dir2\\TestLibrary" });

            var es = new ExternalStorageLogic(fam.Object, new AlbumInfo());
            var retval = es.Initialize();
            Assert.True(retval);
            Assert.Equal("storage\\dir2\\TestLibrary", es.LEAKGetLibraryPath());
        }

        [Fact]
        public static void GetAlbumPaths_() {
            var fam = new Mock<IFileSystemAccess>();
            fam.Setup(fa => fa.GetDirectories("storage\\Library")).Returns(new string[] { "storage\\Library\\[art]albumfolder1", "storage\\Library\\albumfolder2", "storage\\Library\\subfolder1", "storage\\Library\\emptyfolder" });
            fam.Setup(fa => fa.IsFileExists("storage\\Library\\[art]albumfolder1\\_album.json")).Returns(true);
            fam.Setup(fa => fa.IsFileExists("storage\\Library\\albumfolder2\\_album.json")).Returns(true);
            fam.Setup(fa => fa.GetDirectories("storage\\Library\\subfolder1")).Returns(new string[] { "storage\\Library\\subfolder1\\[artist] myalbum [eng]", "storage\\Library\\subfolder1\\album on sub" });
            fam.Setup(fa => fa.IsFileExists("storage\\Library\\subfolder1\\[artist] myalbum [eng]\\_album.json")).Returns(true);
            fam.Setup(fa => fa.IsFileExists("storage\\Library\\subfolder1\\album on sub\\_album.json")).Returns(true);

            var es = new ExternalStorageLogic(fam.Object, new AlbumInfo());
            es.TESTBypassInitialize("storage\\Library");
            var albums = es.GetAlbumPaths();

            Assert.Equal(4, albums.Count);
            Assert.Equal("[art]albumfolder1", albums[0]);
            Assert.Equal("subfolder1\\album on sub", albums[3]);
        }

        [Fact] //Method contains no logic. No need to unit test
        public static void GetAlbum_() {
            
        }

        [Fact]
        public static void GetBookmarkPaths_() {
            var fam = new Mock<IFileSystemAccess>();
            fam.Setup(fa => fa.GetDirectories("storage\\Library\\[Art] MyAlbum")).Returns(new string[] { "storage\\Library\\[Art] MyAlbum\\Ch. 1", "storage\\Library\\[Art] MyAlbum\\Ch. 2", "storage\\Library\\[Art] MyAlbum\\Outro"});

            var es = new ExternalStorageLogic(fam.Object, new AlbumInfo());
            es.TESTBypassInitialize("storage\\Library");
            var bookmarks = es.GetBookmarkPaths("[Art] MyAlbum");

            Assert.Equal(3, bookmarks.Count);
            Assert.Equal("Ch. 1", bookmarks[0]);
            Assert.Equal("Outro", bookmarks[2]);
        }

        [Fact]
        public static void GetFirstSuitableFile_() {
            string lib = "storage\\Library";
            string al0 = "[Art] MyAlbum";
            string al1 = "Album0";
            string sub1 = "Ch. 1 - Videos";
            string sub2 = "Ch. 2 - Pics";
            var fam = new Mock<IFileSystemAccess>();
            //Album 1, file on root
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib,al0))).Returns(new string[] { Path.Combine(lib,al0, "00trash.zip"), Path.Combine(lib, al0, "01.jpg"), Path.Combine(lib, al0, "02.png") });

            //Album 2, file on second subfolder
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib,al1))).Returns(new string[] { Path.Combine(lib,al1,"00trash.zip") });
            fam.Setup(fa => fa.GetDirectories(Path.Combine(lib, al1))).Returns(new string[] { Path.Combine(lib,al1,sub1) , Path.Combine(lib, al1, sub2) });
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib, al1, sub1))).Returns(new string[] { Path.Combine(lib, al1, sub1, "vid1.webm"), Path.Combine(lib, al1, sub1, "vid2.mp4") });
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib, al1, sub2))).Returns(new string[] { Path.Combine(lib, al1, sub2, "00.png"), Path.Combine(lib, al1, sub2, "01.jpg") });

            var es = new ExternalStorageLogic(fam.Object, new AlbumInfo());
            es.TESTBypassInitialize(lib);

            var s1 = es.GetFirstSuitableFile(al0, 1);
            var s2 = es.GetFirstSuitableFile(al1, 1);
            var s3 = es.GetFirstSuitableFile(al1, 0);

            Assert.Equal("01.jpg", s1);
            Assert.Equal("Ch. 2 - Pics\\00.png", s2);
            Assert.True(string.IsNullOrEmpty(s3));
        }

        [Fact]
        public static void GetPagePaths_() {
            string lib = "storage\\Library";
            string al0 = "[Art] MyAlbum";
            string sub1 = "Ch. 1";
            string sub2 = "Ch. 2";

            var fam = new Mock<IFileSystemAccess>();
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib, al0))).Returns(new string[] { Path.Combine(lib,al0,"0.jpg"), Path.Combine(lib, al0, "1.jpg") });
            fam.Setup(fa => fa.GetDirectories(Path.Combine(lib, al0))).Returns(new string[] { Path.Combine(lib, al0, sub1), Path.Combine(lib, al0, sub2) });
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib, al0, sub1))).Returns(new string[] { Path.Combine(lib, al0, sub1, "0.jpg"), Path.Combine(lib, al0, sub1, "1x.jpg") });
            fam.Setup(fa => fa.GetFiles(Path.Combine(lib, al0, sub2))).Returns(new string[] { Path.Combine(lib, al0, sub2, "55.png"), Path.Combine(lib, al0, sub2, "56.jpg") });

            var es = new ExternalStorageLogic(fam.Object, new AlbumInfo());
            es.TESTBypassInitialize(lib);

            var pages = es.GetPagePaths(al0);

            Assert.Equal(6, pages.Count);
            Assert.Equal("0.jpg", pages[0]);
            Assert.Equal("Ch. 1\\1x.jpg", pages[3]);
            Assert.Equal("Ch. 2\\55.png", pages[4]);
        }
    }
}
