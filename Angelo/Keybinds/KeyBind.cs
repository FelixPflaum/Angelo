using System;
using System.Linq;
using System.Windows.Input;
using Angelo.KBM;

namespace Angelo.Keybinds
{
    internal readonly struct KeyBind
    {
        public readonly byte VirtualKey;
        public readonly KeyboardModifiers Modifiers;

        public KeyBind(byte virtualKey, KeyboardModifiers modifiers)
        {
            VirtualKey = virtualKey;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Get key bind from Key.
        /// </summary>
        /// <param name="key">WPF key.</param>
        /// <param name="modifiers">Modifier mask.</param>
        /// <returns>A new KeyBind.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If Key does not translate to a vkey 1-255.</exception>
        public static KeyBind FromKey(Key key, KeyboardModifiers modifiers)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);

            if (vk > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(key), "Key results in invalid virtual key code!");

            return new KeyBind((byte)vk, modifiers);
        }

        /// <summary>
        /// Set key bind from packed uint.
        /// </summary>
        /// <param name="packed">A uint value previously packed with PackInt()</param>
        /// <exception cref="ArgumentOutOfRangeException">If packed vkey is not 1-255.</exception>
        public static KeyBind FromPackedInt(uint packed)
        {
            KeyboardModifiers mods = (KeyboardModifiers)(packed & 0xFFFF0000);
            return new KeyBind((byte)(packed & 0xFF), mods);
        }

        /// <summary>
        /// Pack key and modifier values into one uint.
        /// </summary>
        /// <returns>The uint containing the packed values.</returns>
        public uint PackInt()
        {
            return VirtualKey + (uint)Modifiers;
        }

        /// <summary>
        /// Get array of virtual key codes that make up the key bind.
        /// </summary>
        /// <returns>Array of virtual key codes: [...modifiers, key]. Empty array is no key bind is set.</returns>
        public byte[] GetVKeyArray()
        {
            if (VirtualKey == 0)
                return Array.Empty<byte>();

            // Virtual key codes for modifier keys because import for Keys enum is not working idk
            const byte VK_LSHIFT = 160;
            const byte VK_LCTRL = 162;
            const byte VK_LALT = 164;

            int modifierCount = 0;
            var values = Enum.GetValues(typeof(KeyboardModifiers)).Cast<KeyboardModifiers>();

            foreach (var flag in values)
            {
                if (Modifiers.HasFlag(flag))
                    modifierCount++;
            }

            byte[] virtualKeys = new byte[1 + modifierCount];

            virtualKeys[modifierCount] = VirtualKey;

            if (modifierCount > 0)
            {
                modifierCount--;

                if (Modifiers.HasFlag(KeyboardModifiers.SHIFT))
                {
                    virtualKeys[modifierCount] = VK_LSHIFT;
                    modifierCount--;
                }

                if (Modifiers.HasFlag(KeyboardModifiers.CTRL))
                {
                    virtualKeys[modifierCount] = VK_LCTRL;
                    modifierCount--;
                }

                if (Modifiers.HasFlag(KeyboardModifiers.ALT))
                {
                    virtualKeys[modifierCount] = VK_LALT;
                }
            }

            return virtualKeys;
        }

        /// <summary>
        /// Get a string representation of the key bind.
        /// </summary>
        /// <returns>The string describing the key bind.</returns>
        override public string ToString()
        {
            string str = "";

            if (VirtualKey == 0)
                return str;

            if (Modifiers.HasFlag(KeyboardModifiers.SHIFT))
                str += "Shift + ";

            if (Modifiers.HasFlag(KeyboardModifiers.CTRL))
                str += "Ctrl + ";

            if (Modifiers.HasFlag(KeyboardModifiers.ALT))
                str += "Alt + ";

            return str + KeyBindHelpers.GetCharFromVirtKey(VirtualKey);
        }

        /// <summary>
        /// Check if other KeyBind has same values set.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if values are the same.</returns>
        public bool IsEqualTo(KeyBind other)
        {
            return other.VirtualKey == VirtualKey && other.Modifiers == Modifiers;
        }
    }
}
