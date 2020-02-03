using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SharedLibrary.Helpers;

namespace SharedLibrary.Test
{
    public class HelperTest
    {
        [Fact]
        public void ContainsAny_CorrectBool() {
            //Test data 1
            string[] needles = new string[] { "ANELE", "TriHard", "cmonBruh" };
            string haystack1 = "ANELE Clap";
            string haystack2 = "cmonBroo x";
            string haystack3 = "TRIHARD7";

            bool res1 = haystack1.ContainsAny(needles);
            bool res2 = haystack2.ContainsAny(needles);
            bool res3 = haystack3.ContainsAny(needles);

            Assert.True(res1);
            Assert.False(res2);
            Assert.True(res3);
        }

        [Fact]
        public void ContainsContains_CorrectBool() {
            //Test data 1
            string[] haystack = new string[] { "wololo", "ayoyoyo", "ANELE", "TriHard" };
            string needle1 = "OLOl";
            string needle2 = "X";
            string needle3 = "tri";

            bool res1 = haystack.ContainsContains(needle1);
            bool res2 = haystack.ContainsContains(needle2);
            bool res3 = haystack.ContainsContains(needle3);

            Assert.True(res1);
            Assert.False(res2);
            Assert.True(res3);
        }

        [Fact]
        public void ContainsIgnoreCase_CorrectBool() {
            string haystack = ("Lorem 葉徳東共西あ。定ヤチ Ipsum is simply dummy text");
            string needle1 = ("wololo");
            string needle2 = ("ヤチ Ip");
            string needle3 = ("TEXT");

            Assert.False(haystack.ContainsIgnoreCase(needle1));
            Assert.True(haystack.ContainsIgnoreCase(needle2));
            Assert.True(haystack.ContainsIgnoreCase(needle3));
        }
    }
}
