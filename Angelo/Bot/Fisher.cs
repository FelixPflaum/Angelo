using Angelo.Settings;
using System;
using System.Threading;

namespace Angelo.Bot
{
    internal delegate void LogCallback(string message);
    internal delegate void ExitCallback();

    internal class Fisher
    {
        private readonly MouseHandler _mouse;
        private readonly ScreenHandler _screen;
        private readonly KeyboardHandler _keyboard;
        private readonly SettingsData _settings;

        private readonly LogCallback _logCallback;
        private readonly ExitCallback _exitCallback;

        private readonly Object _stopLock = new();
        private bool _shouldStop;

        public Fisher(LogCallback lcb, ExitCallback ecb)
        {
            _logCallback = lcb;
            _exitCallback = ecb;

            _mouse = new MouseHandler();
            _screen = new ScreenHandler();
            _keyboard = new KeyboardHandler();
            _settings = SettingsManager.GetSettings();
            _shouldStop = false;
        }

        /// <summary>
        /// Start bot loop.
        /// </summary>
        public void OffToSea()
        {
            try
            {
                int orig = _settings.Sensitivity.Value;

                while (!_shouldStop)
                {
                    if (orig != _settings.Sensitivity.Value)
                    {
                        _logCallback(_settings.Sensitivity.Value.ToString());
                        orig = _settings.Sensitivity.Value;
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                _logCallback("Error in fishing thread!");
                _logCallback(ex.Message);
                if (ex.StackTrace != null)
                    _logCallback(ex.StackTrace);
            }
            finally
            {
                _exitCallback();
            }
        }

        /// <summary>
        /// Signal bot loop to stop.
        /// </summary>
        public void BackToHarbor()
        {
            lock (_stopLock)
                _shouldStop = true;
        }

        /// <summary>
        /// Check if stop signal was set. Optionally throws exception in that case.
        /// </summary>
        /// <param name="throwIfShouldStop">If true will throw exception instead of returing true.</param>
        /// <returns>True if should stop.</returns>
        /// <exception cref="OperationCanceledException"></exception>
        public bool ShouldStop(bool throwIfShouldStop = false)
        {
            lock (_stopLock)
            {
                if (_shouldStop)
                {
                    if (throwIfShouldStop)
                        throw new OperationCanceledException();
                    return true;
                }
                return false;
            }
        }
    }
}
