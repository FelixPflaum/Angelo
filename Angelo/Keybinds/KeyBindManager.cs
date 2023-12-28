using Angelo.KBM;
using Angelo.Settings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Angelo.Keybinds
{
    internal class KeyBindManager
    {
        private static readonly KeyBindManager _instance = new();
        private readonly Dictionary<KeyBindId, KeyBind> _keyBinds;
        private KeyboardModifiers _modifiers;

        private KeyBindManager()
        {
            _keyBinds = new();
            _modifiers = KeyboardModifiers.NONE;
        }

        public static KeyBindManager GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Get a KeyBind.
        /// </summary>
        /// <param name="keyBindId"></param>
        /// <returns>The KeyBind</returns>
        private KeyBind GetBind(KeyBindId keyBindId)
        {
            _keyBinds.TryGetValue(keyBindId, out KeyBind? bind);
            if (bind == null)
            {
                bind = new();
                _keyBinds.Add(keyBindId, bind);
            }
            return bind;
        }

        /// <summary>
        /// Update modifier key states.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="active"></param>
        /// <returns>True if key is a modifier key.</returns>
        public bool UpdateModifier(Key key, bool active)
        {
            KeyboardModifiers modifierKey = KeyboardModifiers.NONE;

            switch (key)
            {
                case Key.LeftShift:
                    modifierKey = KeyboardModifiers.SHIFT;
                    break;
                case Key.LeftCtrl:
                    modifierKey = KeyboardModifiers.CTRL;
                    break;
                case Key.System:
                    modifierKey = KeyboardModifiers.ALT;
                    break;
            }

            if (modifierKey != KeyboardModifiers.NONE)
            {
                if (active)
                    _modifiers |= modifierKey;
                else
                    _modifiers &= ~modifierKey;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Set key or modifier. If key is not a modifier then key bind will be updated.
        /// </summary>
        /// <param name="keyBindId"></param>
        /// <param name="key"></param>
        /// <returns>True if key bind was updated.</returns>
        public bool SetKey(KeyBindId keyBindId, Key key)
        {
            if (UpdateModifier(key, true))
                return false;

            KeyBind bind = GetBind(keyBindId);

            if (!bind.SetKeybind(key, _modifiers))
                return false;

            string? enumName = Enum.GetName<KeyBindId>(keyBindId);
            if (enumName != null)
                SettingsManager.settings.KeyBinds[enumName] = bind.PackInt();

            return true;
        }

        /// <summary>
        /// Restore key bindings from settings.
        /// </summary>
        public void LoadSettings()
        {
            foreach (var entry in SettingsManager.settings.KeyBinds)
            {
                KeyBindId id;
                try
                {
                    id = Enum.Parse<KeyBindId>(entry.Key);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, String.Format("Couldn't load bind with key {0}!", entry.Key), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var bind = GetBind(id);
                bind.SetFromPackedInt(entry.Value);
            }
        }

        /// <summary>
        /// Get a string representation of a key bind.
        /// </summary>
        /// <param name="keyBindId"></param>
        /// <returns>The string describing the key bind.</returns>
        public string GetKeyBindString(KeyBindId keyBindId)
        {
            return GetBind(keyBindId).GetKeyBindString();
        }

        /// <summary>
        /// Get array of virtual key codes for keyboard functions.
        /// </summary>
        /// <param name="keyBindId"></param>
        /// <returns>Array of virtual key codes in the order they should be used. Array may be empty if no key bind is set.</returns>
        public byte[] GetVKeyArray(KeyBindId keyBindId)
        {
            return GetBind(keyBindId).GetVKeyArray();
        }
    }
}
