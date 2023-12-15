using System.Diagnostics;

namespace CSharp
{
    public class SpeedoMeter
    {
        private string _folder = "C:\\temp\\SpeedoMeter";
        private Stopwatch _stopWatch = new Stopwatch();
        private string _filePath;
        private string _distinguisher = string.Empty;
        private IDictionary<string, long> _sectionTimeStarts = new Dictionary<string, long>();
        private int _lapNumber = 0;
        private long _previousLapTimeFromStart = 0;

        public SpeedoMeter(string fileName)
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            _filePath = $"{_folder}\\{fileName}.txt";
        }

        public SpeedoMeter(string fileName, string distinguisher) : this(fileName)
        {
            if (!string.IsNullOrWhiteSpace(distinguisher))
            {
                _distinguisher = $" [{distinguisher}]";
            }
        }

        public void Start()
        {
            if (_stopWatch.IsRunning)
            {
                throw new InvalidOperationException("The benchmark is already running");
            }

            File.AppendAllText(_filePath, $"{DateTime.Now.ToString("ddd HH:mm:ss")} New benchmark started{_distinguisher}\n");
            _stopWatch.Start();
        }

        public void SectionStart(string sectionName)
        {
            ValidateStopwatchRunning();
            _stopWatch.Stop();
            if (_sectionTimeStarts.ContainsKey(sectionName))
            {
                throw new InvalidOperationException($"The section {sectionName} is already running");
            }
            _sectionTimeStarts[sectionName] = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Start();
        }

        public void SectionEnd(string sectionName)
        {
            ValidateStopwatchRunning();
            _stopWatch.Stop();
            if (!_sectionTimeStarts.ContainsKey(sectionName))
            {
                throw new InvalidOperationException($"The section {sectionName} was not started");
            }
            var sectionTime = _stopWatch.ElapsedMilliseconds - _sectionTimeStarts[sectionName];
            SaveTime(sectionName, sectionTime);
            _sectionTimeStarts.Remove(sectionName);
            _stopWatch.Start();
        }

        public void Lap() => Lap(null);

        public void Lap(string description)
        {
            ValidateStopwatchRunning();
            _stopWatch.Stop();
            var lapTime = _stopWatch.ElapsedMilliseconds - _previousLapTimeFromStart;
            var lapDescription = string.IsNullOrWhiteSpace(description) ? $"Lap {_lapNumber++}" : $"Lap {_lapNumber++} [{description}]";
            SaveTime(lapDescription, lapTime);
            _previousLapTimeFromStart = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Start();
        }

        public void End()
        {
            ValidateStopwatchRunning();
            _stopWatch.Stop();

            SaveTime("In total it ", _stopWatch.ElapsedMilliseconds);
            if (_sectionTimeStarts.Any())
            {
                var unfinishedSections = string.Join(", ", _sectionTimeStarts.Keys);
                File.AppendAllText(_filePath, $"Unfinished sections: {unfinishedSections}");
            }

            _stopWatch.Reset();
            _sectionTimeStarts.Clear();
            _lapNumber = 0;
            _previousLapTimeFromStart = 0;
        }

        private void ValidateStopwatchRunning()
        {
            if (!_stopWatch.IsRunning)
            {
                throw new InvalidOperationException("The benchmark is already running");
            }
        }

        private void SaveTime(string description, long elapsedMiliseconds)
        {
            var message = $"{DateTime.Now.ToString("ddd HH:mm:ss")}{_distinguisher} {description} took {elapsedMiliseconds / 1000}.{(elapsedMiliseconds % 1000).ToString("D3")}s\n";
            File.AppendAllText(_filePath, message);
        }
    }
}