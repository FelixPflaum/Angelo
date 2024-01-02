using Angelo.Settings;
using System;
using System.Drawing;
using System.Threading;
using static Angelo.Bot.GameWindowHelpers;

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
        private readonly Random _rnd;

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
            _rnd = new Random();
            _shouldStop = false;
        }

        /// <summary>
        /// Write line to log.
        /// </summary>
        /// <param name="message"></param>
        private void Log(string message)
        {
            _logCallback(message);
        }

        /// <summary>
        /// Start bot loop.
        /// </summary>
        public void OffToSea()
        {
            try
            {
                WaitForGameWindow();

                Log("Searching for fishing area anchors...");
                while (true)
                {
                    if (WaitForGameWindow())
                        Log("Searching for fishing area anchors...");

                    if (_screen.SetupAnchors())
                        break;

                    SleepChecked(500);
                }
                Log("Anchors found.");

                FishingLoop();
            }
            catch (OperationCanceledException)
            {
                // Wanted cancellation.
            }
            catch (Exception ex)
            {
                Log("Error in fishing thread!");
                Log(ex.Message);
                if (ex.StackTrace != null)
                    Log(ex.StackTrace);
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
        /// Check if stop signal was set.
        /// </summary>
        /// <returns>True if should stop.</returns>
        public bool ShouldStop()
        {
            lock (_stopLock)
                return _shouldStop;
        }

        /// <summary>
        /// Throws exception if stop flag is set.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        private void ThrowIfShouldStop()
        {
            if (ShouldStop())
                throw new OperationCanceledException();
        }

        /// <summary>
        /// Sleeps for the given duration (optionally random duration) and then checks if stop flag was set in the meantime.
        /// If duration is long it will split the wait time and check multiple times.
        /// </summary>
        /// <param name="duration">The sleep duration in ms.</param>
        /// <param name="durationMax">If not 0 and bigger than duration a random value between the two will be chosen.</param>
        private void SleepChecked(int duration, int durationMax = 0)
        {
            if (duration < 0)
                return;

            if (durationMax > 0 && durationMax > duration)
                duration = _rnd.Next(duration, durationMax);

            if (duration < 250)
            {
                Thread.Sleep(duration);
                ThrowIfShouldStop();
                return;
            }

            int stages = (int)Math.Ceiling(duration / 250.0);
            int stageDur = duration / stages;

            for (int i = 0; i < stages; i++)
            {
                Thread.Sleep(stageDur);
                ThrowIfShouldStop();
            }
        }

        /// <summary>
        /// Waits until game window is in foreground, then returns. If anchors are already setup also checks their visibility.
        /// </summary>
        /// <returns>true if we waited for game window, false if window was immediately found and in foreground</returns>
        private bool WaitForGameWindow()
        {
            if (IsGameInForeground() && (!_screen.HaveAnchorPositions() || _screen.AreAnchorsVisible()))
                return false;

            Log("Waiting for game window to be in foreground and unobstructed...");
            while (true)
            {
                SleepChecked(1000);
                if (IsGameInForeground() && (!_screen.HaveAnchorPositions() || _screen.AreAnchorsVisible()))
                    return true;
            }
        }

        /// <summary>
        /// Waits until mouse is not used anymore.
        /// </summary>
        /// <returns>true if we waited for the mouse, false if mouse was unused.</returns>
        private bool WaitForMouse()
        {
            if (!_mouse.UpdateUsageState())
                return false;

            Log("You moved the mouse, I'm waiting until you don't need it anymore...");
            while (true)
            {
                SleepChecked(500);
                if (!_mouse.UpdateUsageState())
                    return true;
            }
        }

        /// <summary>
        /// Waits until game window is in foreground and mouse is not used.
        /// </summary>
        /// <returns>true if we waited, false if we were immediately ready.</returns>
        private bool WaitForMouseAndGame()
        {
            if (!WaitForMouse() && !WaitForGameWindow())
                return false;

            while (true)
            {
                if (!WaitForMouse() && !WaitForGameWindow())
                    return true;
            }
        }

        /// <summary>
        /// Waits if we are in combat.
        /// </summary>
        /// <returns>true if we waited, false we weren't in combat at all.</returns>
        private bool WaitForCombat()
        {
            if (!_screen.CheckDataPixel(DataColors.InCombat))
                return false;

            Log("I'm in combat! Pretending to not exist...");
            while (true)
            {
                SleepChecked(2000);
                if (!_screen.CheckDataPixel(DataColors.InCombat))
                    return true;
            }
        }

        /// <summary>
        /// Attempt to apply lure. Tries to send keys, checks cast and subsequent lure state.
        /// </summary>
        /// <returns>true if lure was applied, false if it failed.</returns>
        private bool ApplyLure()
        {
            if (!_keyboard.SendLure())
                return false;

            SleepChecked(333);
            if (!_screen.CheckDataPixel(DataColors.Casting))
                return false;

            SleepChecked(5000);
            return _screen.CheckDataPixel(DataColors.LureActive);
        }

        /// <summary>
        /// Attempt to cast fishing. Tries to send keys and checks if cast starts.
        /// </summary>
        /// <returns>true if cast was started, false otherwise.</returns>
        private bool CastFishing()
        {
            if (!_keyboard.SendFish())
                return false;

            SleepChecked(333);
            return _screen.CheckDataPixel(DataColors.Casting);
        }

        private Point? FindBobber()
        {
            return null;
        }

        private bool WaitAndCatch(Point bobberPos)
        {

            // WaitForMouseAndGame(); before click to loot

            return false;
        }

        /// <summary>
        /// Run the main fishing loop.
        /// </summary>
        private void FishingLoop()
        {
            Log("Going to work.");

            while (true)
            {
                if (WaitForCombat())
                {
                    Log("Combat ended.");
                    continue;
                }

                if (!_screen.CheckDataPixel(DataColors.Alive))
                {
                    Log("Rest in pepperoni I guess.");
                    BackToHarbor();
                    ThrowIfShouldStop();
                }

                if (_settings.UseLure.Value && !_screen.CheckDataPixel(DataColors.LureActive))
                {
                    WaitForMouseAndGame();

                    Log("Applying new lure...");
                    if (!ApplyLure())
                    {
                        Log("Lure application failed! Retry...");
                        SleepChecked(2000);
                        continue;
                    }
                }

                WaitForMouseAndGame();

                if (!CastFishing())
                {
                    Log("Couldn't cast fishing! Retry...");
                    SleepChecked(2000);
                    continue;
                }

                Point? bobberPos = FindBobber();
                if (bobberPos == null)
                {
                    Log("Couldn't find bobber position! Retry...");
                    SleepChecked(4567);
                    continue;
                }

                Log("Waiting for fish to bite...");
                if (WaitAndCatch(bobberPos.Value))
                    Log("Cought a fish :)");
                else
                    Log("Failed to catch the fish :(");

                SleepChecked(1000, 4000);
            }
        }
    }
}
