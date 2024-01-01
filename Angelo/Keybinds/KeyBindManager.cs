using Angelo.KBM;
using Angelo.Settings;
using System;
using System.Windows.Input;

namespace Angelo.Keybinds
{
    internal class KeyBindManager
    {
        private readonly SettingsData _settingsData;
        private KeyboardModifiers _modifiers;

        public KeyBindManager()
        {
            _modifiers = KeyboardModifiers.NONE;
            _settingsData = SettingsManager.GetSettings();
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

        private KeyBind GetKeyBind(KeyBindId keyBindId)
        {
            return keyBindId switch
            {
                KeyBindId.FISHING => _settingsData.FishingKey.Value,
                KeyBindId.LURE => _settingsData.LureKey.Value,
                _ => throw new ArgumentException("Invalid KeyBindId"),
            };
        }

        private void SetKeyBind(KeyBindId keyBindId, KeyBind keyBind)
        {
            switch (keyBindId)
            {
                case KeyBindId.FISHING:
                    _settingsData.FishingKey.Value = keyBind;
                    break;
                case KeyBindId.LURE:
                    _settingsData.LureKey.Value = keyBind;
                    break;
                default:
                    throw new ArgumentException("Invalid KeyBindId");
            }
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

            KeyBind oldBind = GetKeyBind(keyBindId);
            KeyBind newBind;

            try
            {
                newBind = KeyBind.FromKey(key, _modifiers);
            }
            catch
            {
                return false;
            }

            if (oldBind.IsEqualTo(newBind))
                return false;

            SetKeyBind(keyBindId, newBind);
            return true;
        }
    }
}
