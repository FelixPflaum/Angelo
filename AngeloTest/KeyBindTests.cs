using Angelo.KBM;
using Angelo.Keybinds;
using System.Windows.Input;

namespace AngeloTest
{
    internal struct KeybindToTest
    {
        public Key key;
        public KeyboardModifiers modifiers;
        public byte expectedVirtualKey;
        public string expectedString;
    }

    [TestClass]
    public class KeyBindTests
    {
        [TestMethod]
        public void TestKeybind()
        {
            KeybindToTest[] tests = new KeybindToTest[]
            {
                new KeybindToTest() {
                    key = Key.A,
                    modifiers = KeyboardModifiers.NONE,
                    expectedVirtualKey = 65,
                    expectedString = "A",
                },
                new KeybindToTest() {
                    key = Key.H,
                    modifiers = KeyboardModifiers.ALT,
                    expectedVirtualKey = 72,
                    expectedString = "Alt + H",
                },
                new KeybindToTest() {
                    key = Key.D6,
                    modifiers = KeyboardModifiers.CTRL,
                    expectedVirtualKey = 54,
                    expectedString = "Ctrl + 6",
                },
                new KeybindToTest() {
                    key = Key.D8,
                    modifiers = KeyboardModifiers.SHIFT,
                    expectedVirtualKey = 56,
                    expectedString = "Shift + 8",
                },
                new KeybindToTest() {
                    key = Key.C,
                    modifiers = KeyboardModifiers.SHIFT | KeyboardModifiers.CTRL,
                    expectedVirtualKey = 67,
                    expectedString = "Shift + Ctrl + C",
                }
            };

            foreach (var test in tests)
            {
                var kb = KeyBind.FromKey(test.key, test.modifiers);
                
                Assert.AreEqual(test.expectedVirtualKey, kb.VirtualKey, "Virtual key doesn't match expected value!");
                Assert.AreEqual(test.expectedString, kb.ToString(), "String output doesn't match expected value!");
                Assert.AreEqual(test.modifiers, kb.Modifiers, "Modifiers don't match anymore!");

                var kbfp = KeyBind.FromPackedInt(kb.PackInt());

                Assert.AreEqual(kbfp.VirtualKey, kb.VirtualKey, "Virtual key doesn't match anymore after pack!");
                Assert.AreEqual(kbfp.Modifiers, kb.Modifiers, "Modifiers don't match anymore after pack!");
            }
        }
    }
}
