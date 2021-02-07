using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharedLibrary.Test
{
    [TestClass]
    public class AlbumTest
    {
        [TestMethod]
        public void Album1_Getters() {
            var album = new Album {
                Artists = new List<string>() { "My Author", "Artist1" },
                Title = "Placeholder for Title",
                EntryDate = new DateTime(2020, 6, 1, 0, 15, 20)
            };

            string title = album.GetFullTitleDisplay();
            string id = album.GetId();

            Assert.AreEqual("[My Author, Artist1] Placeholder for Title", title);
            Assert.AreEqual("MYAPLACEPU", id);
        }
    }
}
