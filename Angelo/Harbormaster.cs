using System.Threading;
using Angelo.Bot;
using Angelo.UI;

namespace Angelo
{
    internal class Harbormaster
    {
        private readonly MainWindow _mainWindow;
        private readonly StartButton _startButton;

        private Fisher? _fisher;

        public Harbormaster(MainWindow mw, StartButton startButton)
        {
            _mainWindow = mw;
            _startButton = startButton;
        }

        public bool StartFishing()
        {
            if (_fisher != null)
                return false;

            _mainWindow.AddLogLine("Starting to fish!");
            _startButton.SetToStop();

            _fisher = new Fisher(new LogCallback(ThreadLog), new ExitCallback(ThreadExit));
            ThreadStart ts = new(_fisher.OffToSea);
            Thread t = new(ts);
            t.Start();

            return true;
        }

        public bool IsFishing()
        {
            return _fisher != null;
        }

        public bool StopFishing()
        {
            if (_fisher != null)
            {
                _fisher.BackToHarbor();
                _startButton.SetToStart();
                _startButton.SetActive(false);
                return true;
            }
            return false;
        }

        private void ThreadExit()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.AddLogLine("Fishing ended.");
                _fisher = null;
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
    }
}
