using System;
using System.Collections.Generic;
using System.Text;
using ViewerApp.AL;
using ViewerApp.DAL;
using Xunit;
using Moq;
using SharedLibrary;
using System.IO;

namespace ViewerApp.Test
{
    public class AppLogicTest
    {
        [Fact]
        public static void GetAlbumCards_() {
            var esm = new Mock<IExternalStorageLogic>();
            var ism = new Mock<IInternalStorageLogic>();
            var aim = new Mock<IAlbumInfoProvider>();
            var al = new AppLogic(esm.Object, ism.Object, aim.Object);
            var db = GetMockLibraryDb();
            al.TESTSetLibraryData(db);


            List<QueryCard> queryCards = new List<QueryCard> {
                new QueryCard {//0
                    Name = "All John Smith Works",
                    Query = "artist:John Smith"
                },
                new QueryCard {//1
                    Name = "All with Pics in its title",
                    Query = "title:Pics"
                },
                new QueryCard {//2
                    Name = "Blue or brown tags",
                    Query = "tag:Blue|Brown"
                },
                new QueryCard {//3
                    Name = "Blue or brown and not white",
                    Query = "tag:Blue|Brown,tag!White"
                },
                new QueryCard {//4
                    Name = "John Lenon with Green and not brown",
                    Query = "artist:John Lenon,tag:Green,tag!Brown"
                },
                new QueryCard {//5
                    Name = "Bob with tier > 0",
                    Query = "artist:Bob,tier>0"
                }
            };

            List<ICardItem> retval0 = al.GetAlbumCards(queryCards[0].Query).Data;
            List<ICardItem> retval1 = al.GetAlbumCards(queryCards[1].Query).Data;
            List<ICardItem> retval2 = al.GetAlbumCards(queryCards[2].Query).Data;
            List<ICardItem> retval3 = al.GetAlbumCards(queryCards[3].Query).Data;
            List<ICardItem> retval4 = al.GetAlbumCards(queryCards[4].Query).Data;
            List<ICardItem> retval5 = al.GetAlbumCards(queryCards[5].Query).Data;

            Assert.Equal(2, retval0.Count);
            Assert.Equal(2, retval1.Count);
            Assert.Equal(3, retval2.Count);
            Assert.Equal(1, retval3.Count);
            Assert.Equal(1, retval4.Count);

            string debug = retval5[0].GetTitle();
            Assert.Equal("[John Lenon,Greg,Bob] House Cats & Wild Cats", retval5[0].GetTitle());
        }

        static LibraryDatabase GetMockLibraryDb() {
            var albumCards = new List<AlbumCard> { 
                new AlbumCard {
                    Album = new Album {
                        Title = "Ocean Pics",
                        Artists = new List<string>{"John Smith"},
                        Tags = new List<string>{"Blue", "White", "Green"},
                        Tier = 3
                    }
                },
                new AlbumCard {
                    Album = new Album {
                        Title = "House Cats & Wild Cats",
                        Artists = new List<string>{"John Lenon","Greg","Bob"},
                        Tags = new List<string>{"White", "Black", "Orange", "Brown"},
                        Tier = 2
                    }
                },
                new AlbumCard {
                    Album = new Album {
                        Title = "Forest Pics",
                        Artists = new List<string>{"John Smith","John Lenon"},
                        Tags = new List<string>{"Green", "Brown"},
                        Tier = 1
                    }
                },
                new AlbumCard {
                    Album = new Album {
                        Title = "Disco",
                        Artists = new List<string>{"John Lenon","Khabib"},
                        Tags = new List<string>{"Green", "Yellow", "Pink", "Red"},
                        Tier = 0
                    }
                },
                new AlbumCard {
                    Album = new Album {
                        Title = "Autumn Leaves",
                        Artists = new List<string>{"Eric Clapton","Bob"},
                        Tags = new List<string>{"Orange", "Red"},
                        Tier = 0
                    }
                }
            };
            var result = new LibraryDatabase { AlbumCards = albumCards };


            return result;
        }
    }
}
