using System;
using System.Linq;
using System.Windows.Input;
using Angelo.KBM;

namespace Angelo.Keybinds
{
    internal class KeyBind
    {
        private byte _virtualKey;
        private KeyboardModifiers _modifiers;

        public KeyBind()
        {
            SetKeybind(Key.None, 0);
        }

        /// <summary>
        /// Set key bind.
        /// </summary>
        /// <param name="key">WPF key code.</param>
        /// <param name="modifiers">Modifier mask.</param>
        /// <returns>True if key was valid.</returns>
        public bool SetKeybind(Key key, KeyboardModifiers modifiers)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk <= 0xFF)
            {
                _virtualKey = (byte)KeyInterop.VirtualKeyFromKey(key);
                _modifiers = modifiers;

                if (_virtualKey == 0)
                    _modifiers = 0;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the virtual key code.
        /// </summary>
        /// <returns>The virtual key code.</returns>
        public byte GetKey()
        {
            return _virtualKey;
        }

        /// <summary>
        /// Get the modifier mask.
        /// </summary>
        /// <returns>The modifier mask.</returns>
        public KeyboardModifiers GetMods()
        {
            return _modifiers;
        }

        /// <summary>
        /// Pack key and modifier values into one uint.
        /// </summary>
        /// <returns>The uint containing the packed values.</returns>
        public uint PackInt()
        {
            return _virtualKey + (uint)_modifiers;
        }

        /// <summary>
        /// Set key bind from packed uint.
        /// </summary>
        /// <param name="packed">A uint value previously packed with PackInt()</param>
        public void SetFromPackedInt(uint packed)
        {
            _modifiers = (KeyboardModifiers)(packed & 0xFFFF0000);
            _virtualKey = (byte)(packed & 0xFF);
        }

        /// <summary>
        /// Get array of virtual key codes that make up the key bind.
        /// </summary>
        /// <returns>Array of virtual key codes: [...modifiers, key]. Empty array is no key bind is set.</returns>
        public byte[] GetVKeyArray()
        {
            if (_virtualKey == 0)
                return Array.Empty<byte>();

            // Virtual key codes for modifier keys because import for Keys enum is not working idk
            const byte VK_LSHIFT = 160;
            const byte VK_LCTRL = 162;
            const byte VK_LALT = 164;

            int modifierCount = 0;
            var values = Enum.GetValues(typeof(KeyboardModifiers)).Cast<KeyboardModifiers>();

            foreach (var flag in values)
            {
                if (_modifiers.HasFlag(flag))
                    modifierCount++;
            }

            byte[] virtualKeys = new byte[1 + modifierCount];

            virtualKeys[modifierCount] = _virtualKey;

            if (modifierCount > 0)
            {
                modifierCount--;

                if (_modifiers.HasFlag(KeyboardModifiers.SHIFT))
                {
                    virtualKeys[modifierCount] = VK_LSHIFT;
                    modifierCount--;
                }

                if (_modifiers.HasFlag(KeyboardModifiers.CTRL))
                {
                    virtualKeys[modifierCount] = VK_LCTRL;
                    modifierCount--;
                }

                if (_modifiers.HasFlag(KeyboardModifiers.ALT))
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
        public string GetKeyBindString()
        {
            string str = "";

            if (_virtualKey == 0)
                return str;

            if (_modifiers.HasFlag(KeyboardModifiers.SHIFT))
                str += "Shift + ";

            if (_modifiers.HasFlag(KeyboardModifiers.CTRL))
                str += "Ctrl + ";

            if (_modifiers.HasFlag(KeyboardModifiers.ALT))
                str += "Alt + ";

            return str + KeyBindHelpers.GetCharFromVirtKey(_virtualKey);
        }
    }
}
