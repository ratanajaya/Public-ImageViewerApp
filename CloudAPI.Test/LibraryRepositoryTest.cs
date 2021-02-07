using CloudAPI.AL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;
using Serilog;
using System.Linq;
using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Services;

namespace CloudAPI.Test
{
    [TestClass]
    public class LibraryRepositoryTest
    {
        //ISystemIOAbstraction _io;
        Mock<ISystemIOAbstraction> _ioMock;
        ConfigurationModel _config;
        LibraryRepository _lib;

        [TestInitialize]
        public void TestInitialize() {
            #region Mock Data
            _config = new ConfigurationModel {
                AlbumMetadataFileName = "_album.json",
                LibraryPath = "D:\\",
                FullAlbumDbPath = "D:\\_cache\\_dbAlbum.json",
                FullArtistDbPath = "D:\\_cache\\_dbArtist.json",
                FullPageCachePath = "D:\\_cache\\ResizedPage"
            };

            var artistVMs = new List<QueryVM> {
                new QueryVM { Name = "Bob Smith" },
                new QueryVM { Name = "Greg"  },
                new QueryVM { Name = "John"  },
            };

            var albumVMs = new List<AlbumVM> {
                new AlbumVM {
                    //AlbumId = "j001",
                    Path = "JKL\\[John] House cats playing",
                    Album = new Album {
                        Title = "House cats playing",
                        Category = "Vertebrate",
                        Artists = { "John" },
                        Tags = { "Pet", "Carnivore", "Land" },
                        IsRead = false,
                        Tier = 0
                    }
                },
                new AlbumVM {
                    //AlbumId = "j002",
                    Album = new Album {
                        Title = "Horse Racing",
                        Category = "Vertebrate",
                        Artists = { "John" },
                        Tags = { "Pet", "Livestock", "Herbivore", "Land" },
                        IsRead = false,
                        Tier = 0
                    }
                },
                new AlbumVM {
                    //AlbumId = "j003",
                    Path = "JKL\\[John] Birds With Jobs",
                    Album = new Album {
                        Title = "Birds With Jobs",
                        Category = "Vertebrate",
                        Artists = {"John"},
                        Tags = { "Pet", "Livestock", "Omnivore", "Air" },
                        IsRead = true,
                        Tier = 2
                    }
                },
                new AlbumVM {
                    //AlbumId = "b001",
                    Album = new Album {
                        Title = "Cows In Barn",
                        Category = "Vertebrate",
                        Artists = { "Bob Smith" },
                        Tags = { "Livestock", "Herbivore", "Land" },
                        IsRead = false,
                        Tier = 0
                    }
                },
                new AlbumVM {
                    //AlbumId = "b002",
                    Album = new Album {
                        Title = "Leech Farm",
                        Category = "Invertebrate",
                        Artists = { "Bob Smith" },
                        Tags = { "Livestock", "Herbivore", "Land" },
                        IsRead = false,
                        Tier = 0
                    }
                },
                new AlbumVM {
                    //AlbumId = "b003",
                    Album = new Album {
                        Title = "挙ん深順フナツホ",
                        Category = "Invertebrate",
                        Artists = { "Bob Smith", "John" },
                        Tags = { "Livestock", "Pet", "Herbivore", "Land", "Water" },
                        IsRead = true,
                        Tier = 2
                    }
                },
                new AlbumVM {
                    //AlbumId = "g001",
                    Album = new Album {
                        Title = "😂 🤣 Greg's masterpiece 😋 😛",
                        Category = "Invertebrate",
                        Artists = { "Greg" },
                        Tags = { "Livestock", "Herbivore", "Land", "Air", "Water" },
                        IsRead = false,
                        Tier = 3
                    }
                },
                new AlbumVM {
                    //AlbumId = "g002",
                    Album = new Album {
                        Title = "Album with empty tag",
                        Category = "Vertebrate",
                        Artists = { "Greg" },
                        Tags = { },
                        IsRead = true,
                        Tier = 3
                    }
                },
                new AlbumVM {
                    //AlbumId = "g003",
                    Album = new Album {
                        Title = "Greg new album",
                        Category = "Vertebrate",
                        Artists = { "Greg" },
                        Tags = { "Pet", "Livestock" },
                        IsRead = false,
                        Tier = 3
                    }
                },
                new AlbumVM {
                    //AlbumId = "JOSLAB",
                    Path = "JKL\\[Josef Mengele] Lab Monkeys",
                    Album = new Album {
                        Title = "Lab Monkeys",
                        Category = "Vertebrate",
                        Artists = {"Josef Mengele"},
                        Tags = {"Intelligent"},
                        IsRead = true,
                        Tier = 2,
                        EntryDate = new DateTime(2016, 7, 15, 3, 15, 0)
                    }
                }
            };

            #endregion

            var loggerMock = new Mock<ILogger>();

            _ioMock = new Mock<ISystemIOAbstraction>();
            _ioMock.Setup(m => m.DeserializeMsgpack<List<AlbumVM>>(It.IsAny<string>())).ReturnsAsync(albumVMs);

            var dbContext = new JsonDbContext(_config, loggerMock.Object, new FakeAlbumInfoProvider(), _ioMock.Object);
            _lib = new LibraryRepository(_config, new FakeAlbumInfoProvider(), dbContext, _ioMock.Object);
        }

        [TestMethod]
        public void GetAlbumVMs_NoQuery() {
            var res1 = _lib.GetAlbumVMs(0, 0, "").ToList();

            Assert.IsTrue(res1.Count >= 9);
        }

        [TestMethod]
        public void GetAlbumVMs_PetNotCarnivore() {
            var res1 = _lib.GetAlbumVMs(0, 0, "tag:pet,tag!carnivore");

            Assert.IsTrue(res1.Any(r => r.Album.Title == "挙ん深順フナツホ"));
            Assert.IsTrue(res1.Any(r => r.Album.Title == "Horse Racing"));
            Assert.IsFalse(res1.Any(r => r.Album.Title == "House cats playing"));
            Assert.IsFalse(res1.Any(r => r.Album.Title == "Album with empty tag"));
        }

        [TestMethod]
        public void GetAlbumVMs_AirOrWaterNotLand() {
            var res1 = _lib.GetAlbumVMs(0, 0, "tag:air|water,tag!land");

            Assert.IsTrue(res1.Any(r => r.Album.Title == "Birds With Jobs"));
            Assert.IsFalse(res1.Any(r => r.Album.Title == "😂 🤣 Greg's masterpiece 😋 😛"));
        }

        [TestMethod]
        public void GetAlbumVMs_TierOver1NotPet() {
            var res1 = _lib.GetAlbumVMs(0, 0, "tier>1,tag!pet");

            Assert.IsTrue(res1.Any(r => r.Album.Title == "Album with empty tag"));
            Assert.IsTrue(res1.Any(r => r.Album.Title == "😂 🤣 Greg's masterpiece 😋 😛"));
            Assert.IsFalse(res1.Any(r => r.Album.Title == "Birds With Job"));
            Assert.IsFalse(res1.Any(r => r.Album.Title == "Greg new album"));
        }

        [TestMethod]
        public void InsertAlbum_NewAlbum() {
            var originalFolderName = "[Forsen] Pics of forsenE twitch spam [English]";
            var album = new Album {
                Artists = {"Forsen"},
                Title = "Pics of forsenE twitch spam",
                Tags = {"Internet", "Human"},
                Languages = {"English"},
                EntryDate = new DateTime(2016, 7, 15, 3, 15, 0)
            };
            var albumId = _lib.InsertAlbum(originalFolderName, album);


            _ioMock.Verify(io => io.SerializeToJson(It.Is<string>(s => s.Contains(originalFolderName)), album), Times.Exactly(1));
            _ioMock.Verify(io => io.SerializeToMsgpack(It.Is<string>(s => s.Equals(_config.FullAlbumDbPath)), It.IsAny<object>()), Times.Exactly(1));
        }

        [TestMethod]
        public void InsertAlbum_OverwriteExsiting() {
            var originalFolderName = "[Josef Mengele] Lab Monkeys";
            var album = new Album {
                Artists = { "Josef Mengele" },
                Title = "Lab Monkeys",
                Tags = { "Intelligent", "Land" },
                Languages = { "Japanese" },
                EntryDate = new DateTime(2017, 7, 15, 3, 15, 0)
            };
            var albumId = _lib.InsertAlbum(originalFolderName, album);

            //Assert.IsTrue(albumId.Contains("JOSLAB"));
            //Assert.IsFalse(albumId.Equals("JOSLAB"));
            _ioMock.Verify(io => io.SerializeToJson(It.Is<string>(s => s.Contains(originalFolderName)), album), Times.Exactly(1));
        }

        #region LEGACY Test no longer valid as 2020-11-29 due to breaking changes on AlbumId
        //[TestMethod]
        //public void UpdateAlbum_Success() {
        //    var existing = _lib.GetAlbumVM("JOSLAB");

        //    var albumVm = new AlbumVM {
        //        //AlbumId = "JOSLAB",
        //        Path = "JKL\\[Josef Mengele] Lab Monkeys",
        //        Album = new Album {
        //            Artists = { "Josef Mengele" },
        //            Title = "Vaccine testing with Lab Monkeys",
        //            Tags = { "Intelligent", "Land" },
        //            Languages = { "Japanese" },
        //            EntryDate = new DateTime(2016, 7, 15, 3, 15, 0)
        //        }
        //    };

        //    var albumId = _lib.UpdateAlbum(albumVm);
        //    Assert.IsFalse(albumId.Equals("JOSLAB"));
        //    _ioMock.Verify(io => io.SerializeToJson(It.Is<string>(s => s.Contains(existing.Path)), It.IsAny<object>()), Times.Exactly(1));
        //}

        //[TestMethod]
        //public void UpdateAlbum_DifferentEntryDate_Failed() {
        //    var albumVm = new AlbumVM {
        //        //AlbumId = "JOSLAB",
        //        Path = "JKL\\[Josef Mengele] Lab Monkeys",
        //        Album = new Album {
        //            Artists = { "Josef Mengele" },
        //            Title = "Vaccine testing with Lab Monkeys",
        //            Tags = { "Intelligent", "Land" },
        //            Languages = { "Japanese" },
        //            EntryDate = new DateTime(2017, 7, 15, 3, 15, 0)
        //        }
        //    };

        //    Assert.ThrowsException<InvalidOperationException>(() => _lib.UpdateAlbum(albumVm));
        //}

        //[TestMethod]
        //public void UpdateAlbumOuterValue_ExistingAlbumHasNotBeenRead() {
        //    var existing = _lib.GetAlbumVM("j001");

        //    _lib.UpdateAlbumOuterValue("j001", 5);
        //    _ioMock.Verify(io => io.SerializeToJson(It.Is<string>(s => s.Contains(existing.Path)), It.IsAny<object>()), Times.Exactly(1));
        //    _ioMock.Verify(io => io.SerializeToMsgpack(It.Is<string>(s => s.Equals(_config.FullAlbumDbPath)), It.IsAny<object>()), Times.Exactly(1));
        //}

        //[TestMethod]
        //public void UpdateAlbumOuterValue_ExistingAlbumHasBeenRead() {
        //    var existing = _lib.GetAlbumVM("j003");

        //    _lib.UpdateAlbumOuterValue("j003", 5);
        //    _ioMock.Verify(io => io.SerializeToJson(It.Is<string>(s => s.Contains(existing.Path)), It.IsAny<object>()), Times.Never);
        //    _ioMock.Verify(io => io.SerializeToMsgpack(It.Is<string>(s => s.Equals(_config.FullAlbumDbPath)), It.IsAny<object>()), Times.Exactly(1));
        //}

        #endregion
    }
}
