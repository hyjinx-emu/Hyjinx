using Hyjinx.HLE.HOS.Applets;
using System.Text;

namespace Hyjinx.Tests.HLE
{
    public class SoftwareKeyboardTests
    {
        [Test]
        public void StripUnicodeControlCodes_NullInput()
        {
            ClassicAssert.IsNull(SoftwareKeyboardApplet.StripUnicodeControlCodes(null));
        }

        [Test]
        public void StripUnicodeControlCodes_EmptyInput()
        {
            ClassicAssert.AreEqual(string.Empty, SoftwareKeyboardApplet.StripUnicodeControlCodes(string.Empty));
        }

        [Test]
        public void StripUnicodeControlCodes_Passthrough()
        {
            string[] prompts = {
                "Please name him.",
                "Name her, too.",
                "Name your friend.",
                "Name another friend.",
                "Name your pet.",
                "Favorite homemade food?",
                "What’s your favorite thing?",
                "Are you sure?",
            };

            foreach (string prompt in prompts)
            {
                ClassicAssert.AreEqual(prompt, SoftwareKeyboardApplet.StripUnicodeControlCodes(prompt));
            }
        }

        [Test]
        public void StripUnicodeControlCodes_StripsNewlines()
        {
            ClassicAssert.AreEqual("I am very tall", SoftwareKeyboardApplet.StripUnicodeControlCodes("I \r\nam \r\nvery \r\ntall"));
        }

        [Test]
        public void StripUnicodeControlCodes_StripsDeviceControls()
        {
            // 0x13 is control code DC3 used by some games
            string specialInput = Encoding.UTF8.GetString(new byte[] { 0x13, 0x53, 0x68, 0x69, 0x6E, 0x65, 0x13 });
            ClassicAssert.AreEqual("Shine", SoftwareKeyboardApplet.StripUnicodeControlCodes(specialInput));
        }

        [Test]
        public void StripUnicodeControlCodes_StripsToEmptyString()
        {
            string specialInput = Encoding.UTF8.GetString(new byte[] { 17, 18, 19, 20 }); // DC1 - DC4 special codes
            ClassicAssert.AreEqual(string.Empty, SoftwareKeyboardApplet.StripUnicodeControlCodes(specialInput));
        }

        [Test]
        public void StripUnicodeControlCodes_PreservesMultiCodePoints()
        {
            // Turtles are a good example of multi-codepoint Unicode chars
            string specialInput = "♀ 🐢 🐢 ♂ ";
            ClassicAssert.AreEqual(specialInput, SoftwareKeyboardApplet.StripUnicodeControlCodes(specialInput));
        }
    }
}
