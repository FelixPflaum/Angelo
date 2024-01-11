using Angelo.Bot;
using Angelo.Bot.Bobber;
using Angelo.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Angelo
{
    internal class Harbormaster
    {
        private readonly MainWindow _mainWindow;
        private readonly StartButton _startButton;

        private event Fisher.SplashDetectionUpdate? _splashEvent;
        private event Fisher.BobberDetectionEvent? _bobberEvent;
        private Fisher? _fisher;

        public Harbormaster(MainWindow mw, StartButton startButton)
        {
            _mainWindow = mw;
            _startButton = startButton;
        }

        /// <summary>
        /// Commence fishing operation.
        /// </summary>
        /// <returns>true if fishing was started, false if it was already running.</returns>
        public bool StartFishing()
        {
            if (_fisher != null)
                return false;

            _mainWindow.AddLogLine("Starting to fish!");
            _startButton.SetToStop();

            _fisher = new Fisher();
            ThreadStart ts = new(_fisher.OffToSea);
            Thread t = new(ts);

            _fisher.LogEvent += ThreadLog;
            _fisher.ExitEvent += ThreadExit;
            _fisher.SplashEvent += ThreadSplash;

            if (_bobberEvent != null)
                _fisher.BobberEvent += ThreadBobber;

            t.Start();

            return true;
        }

        /// <summary>
        /// Check if fishing thread is running.
        /// </summary>
        /// <returns>true if fishing process is running.</returns>
        public bool IsFishing()
        {
            return _fisher != null;
        }

        /// <summary>
        /// Stop fishing thread if it is running.
        /// </summary>
        /// <returns>true if fishing process was stopped, false if it wasn't running.</returns>
        public bool StopFishing()
        {
            if (_fisher != null)
            {
                _fisher.BackToHarbor();
                _startButton.SetActive(false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Register callback for bobber detection event.
        /// Callback will run on the UI thread.
        /// </summary>
        /// <param name="bobberEventHandler"></param>
        public void RegisterEvent(Fisher.BobberDetectionEvent bobberEventHandler)
        {
            _bobberEvent += bobberEventHandler;
            if (_fisher != null && _bobberEvent.GetInvocationList().Length == 1)
                _fisher.BobberEvent += ThreadBobber;
        }

        /// <summary>
        /// Register callback for splash detection update event.
        /// Callback will run on the UI thread.
        /// </summary>
        /// <param name="splashEventHandler"></param>
        public void RegisterEvent(Fisher.SplashDetectionUpdate splashEventHandler)
        {
            _splashEvent += splashEventHandler;
            if (_fisher != null && _splashEvent.GetInvocationList().Length == 1)
                _fisher.SplashEvent += ThreadSplash;
        }

        /// <summary>
        /// Uregister a previously registered event handler.
        /// </summary>
        public void UnregisterEvent(Fisher.BobberDetectionEvent bobberEventHandler)
        {
            _bobberEvent -= bobberEventHandler;
            if (_fisher != null && _bobberEvent == null)
                _fisher.BobberEvent -= ThreadBobber;
        }

        /// <inheritdoc cref="UnregisterEvent(Fisher.BobberDetectionEvent)"/>
        public void UnregisterEvent(Fisher.SplashDetectionUpdate splashEventHandler)
        {
            _splashEvent -= splashEventHandler;
            if (_fisher != null && _splashEvent == null)
                _fisher.SplashEvent -= ThreadSplash;
        }

        private void ThreadExit()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.AddLogLine("Fishing ended.");
                if (_fisher != null)
                {
                    _fisher.LogEvent -= ThreadLog;
                    _fisher.ExitEvent -= ThreadExit;
                    _fisher.SplashEvent -= ThreadSplash;
                    _fisher.BobberEvent -= ThreadBobber;
                    _fisher = null;
                }
                _startButton.SetToStart();
                _startButton.SetActive(true);
            });
        }

        private void ThreadLog(string line)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.AddLogLine(line);
            });
        }

        private void ThreadSplash(int pixelsFound, int threshold, int maxFound)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _splashEvent?.Invoke(pixelsFound, threshold, maxFound);
            });
        }

        private void ThreadBobber(Bitmap bmp, Rectangle checkRegion, List<FloodCountResult> positions)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _bobberEvent?.Invoke(bmp, checkRegion, positions);
            });
        }
    }
}
