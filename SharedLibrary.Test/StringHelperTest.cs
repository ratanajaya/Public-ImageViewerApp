using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedLibrary;

namespace SharedLibrary.Test
{
    [TestClass]
    public class StringHelperTest
    {
        [TestMethod]
        public void ContainsAny_Multiple() {
            string[] needles = new string[] { "ANELE", "TriHard", "cmonBruh" };
            string haystack1 = "ANELE Clap";
            string haystack2 = "cmonBroo x";
            string haystack3 = "TRIHARD7";

            bool res1 = haystack1.ContainsAny(needles);
            bool res2 = haystack2.ContainsAny(needles);
            bool res3 = haystack3.ContainsAny(needles);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }

        [TestMethod]
        public void ContainsContains_Multiple() {
            string[] haystack = new string[] { "wololo", "ayoyoyo", "ANELE", "TriHard" };
            string needle1 = "OLOl";
            string needle2 = "X";
            string needle3 = "tri";

            bool res1 = haystack.ContainsContains(needle1);
            bool res2 = haystack.ContainsContains(needle2);
            bool res3 = haystack.ContainsContains(needle3);

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }

        [TestMethod]
        public void ContainsIgnoreCase_Multiple() {
            string haystack = ("Lorem 葉徳東共西あ。定ヤチ Ipsum is simply dummy text");
            string needle1 = ("wololo");
            string needle2 = ("ヤチ Ip");
            string needle3 = ("TEXT");

            Assert.IsFalse(haystack.ContainsIgnoreCase(needle1));
            Assert.IsTrue(haystack.ContainsIgnoreCase(needle2));
            Assert.IsTrue(haystack.ContainsIgnoreCase(needle3));
        }

        [TestMethod]
        public void RemoveNonLetterDigit_Multiple() {
            string str1 = "[Artist1, Artist2] Title Ch. 1 ~Subtitle~ (Eng)";
            string str2 = "ざごひ化28神ざせさ。芸らみ付市8";
            string str3 = "içeriğinin okuyucunun dikkatini dağıttığı bilinen bir gerçektir.";

            string res1 = str1.RemoveNonLetterDigit();
            string res2 = str2.RemoveNonLetterDigit();
            string res3 = str3.RemoveNonLetterDigit();

            Assert.AreEqual("Artist1Artist2TitleCh1SubtitleEng", res1);
            Assert.AreEqual("ざごひ化28神ざせさ芸らみ付市8", res2);
            Assert.AreEqual("içeriğininokuyucunundikkatinidağıttığıbilinenbirgerçektir", res3);
        }

        [TestMethod]
        public void MapToUpperAlphanumericChar_Multiple() {
            char c1 = 'a';
            char c2 = 'X';
            char c3 = '6';
            string source = "団トئ文学にきぬげょへげべぼぞとぉ";

            char r1 = c1.MapToUpperAlphanumericChar();
            char r2 = c2.MapToUpperAlphanumericChar();
            char r3 = c3.MapToUpperAlphanumericChar();

            string result = "";
            foreach(char c in source) {
                result += c.MapToUpperAlphanumericChar();
            }

            Assert.AreEqual('A', r1);
            Assert.AreEqual('X', r2);
            Assert.AreEqual('6', r3);
            Assert.AreEqual("56098LRMWDYWZ28IN", result);
        }

        [TestMethod]
        public void MapToUpperAlphanumericString_Multiple() {
            string source1 = "団トئ文学にきぬげょへげべぼぞとぉ";
            string source2 = "içeriğininokuyucunundikkatinidağıttığıbilinenbirgerçektir";

            string result1 = source1.MapToUpperAlphanumericString();
            string result2 = source2.MapToUpperAlphanumericString();

            Assert.AreEqual("56098LRMWDYWZ28IN", result1);
            Assert.AreEqual("IPERI9ININOKUYUCUNUNDIKKATINIDA9ITTI9IBILINENBIRGERPEKTIR", result2);
        }

        [TestMethod]
        public void MapToUpperAlphanumericCharIntVersion_Multiple() {
            int source1 = 18;
            int source2 = 73;

            char r1 = source1.MapToUpperAlphanumericChar();
            char r2 = source2.MapToUpperAlphanumericChar();

            Assert.AreEqual('S', r1);
            Assert.AreEqual('B', r2);
        }

        [TestMethod]
        public void TakeUppercaseAlphanumeric_Multiple() {
            string[] sources = {
                "AX",
                "Wololo",
                " bCdaX",
                "--文学ç--",
                "[ar] c"
            };

            var result = new List<string>();
            foreach(string s in sources) {
                result.Add(s.TakeUppercaseAlphanumeric(3));
            }

            Assert.AreEqual("AX0", result[0]);
            Assert.AreEqual("WOL", result[1]);
            Assert.AreEqual("BCD", result[2]);
            Assert.AreEqual("98P", result[3]);
            Assert.AreEqual("ARC", result[4]);
        }
    }
}
