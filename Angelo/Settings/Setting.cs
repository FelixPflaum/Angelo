using System;

namespace Angelo.Settings
{
    internal class Setting<T>
    {
        private readonly Object _lock = new();
        private T _value;
        private readonly T _default;

        public Setting(T initialValue)
        {
            _value = initialValue;
            _default = initialValue;
        }

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }

            }
            set
            {
                lock (_lock)
                {
                    _value = value;
                }
            }
        }

        /// <summary>
        /// Sets this setting to its default value.
        /// </summary>
        public void ResetToDefault()
        {
            lock (_lock)
            {
                _value = _default;
            }
        }
    }
}
