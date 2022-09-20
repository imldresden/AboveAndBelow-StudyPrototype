using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MasterNetworking.Utils
{
    public delegate void LogWrittenEventHandler(object sender, string message);

    /// <summary>
    /// A class that allows to write a consistant csv log.
    /// </summary>
    public class Logger
    {
        public event LogWrittenEventHandler LogWritten;

        private string _path;
        private string _fileName;
        private string[] _header;
        private StreamWriter _writer;
        private string _splitter;

        private string _completePath => $"{_path}\\{_fileName}";

        public Logger(string path, string fileName, string[] header, string splitter = "; ")
        {
            _path = path;
            _fileName = fileName;
            _splitter = splitter;

            _header = header;
            _header = new string[] { "day", "time" };
            _header = _header.Concat(header).ToArray();
        }

        /// <summary>
        /// Must be called to be able to write to the logger. This will open the given file and, when necessary, create the folder and the file itself.
        /// </summary>
        public void Start()
        {
            bool fileExists = File.Exists(_completePath);
            // Try to open the file.
            try
            {
                _writer = new StreamWriter(_completePath, true);
                _writer.AutoFlush = true;
            }
            // If the file couldn't be open, create the file and the directories and open it again.
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(_path);
                FileStream file = File.Create(_completePath);
                file.Close();

                _writer = new StreamWriter(_completePath, true);
                _writer.AutoFlush = true;
            }

            // If the line was freshly created, add the header at the top.
            if (!fileExists)
                _writer.WriteLine(String.Join(_splitter, _header));
        }

        /// <summary>
        /// Stops the logger and closes the stream writer.
        /// </summary>
        public void Stop()
        {
            _writer.Close();
        }

        /// <summary>
        /// Writes the received message to the log file. It will automatically add the timestamp and the date as the first column to a message.
        /// </summary>
        /// <param name="lineEntries">The list of all cloumns of the csv that should be written. This can be less then the given header entries.</param>
        public void Write(params object[] lineEntries)
        {
            DateTime now = DateTime.Now;
            Write(now, lineEntries: lineEntries);
        }

        /// <summary>
        /// Writes the received message to the log file. It will automatically add the timestamp and the date as the first column to a message.
        /// </summary>
        /// <param name="time">The time those log entries should use.</param>
        /// <param name="lineEntries">The list of all cloumns of the csv that should be written. This can be less then the given header entries.</param>
        public void Write(DateTime time, params object[] lineEntries)
        {
            List<object> entriesList = lineEntries.ToList<Object>();
            // Add the time of the message to the list.
            entriesList.Insert(0, time.ToString("yyyy-MM-dd"));
            entriesList.Insert(1, time.ToString("HH:mm:ss.fff"));

            // Add the missing columns.
            while (entriesList.Count < _header.Length)
                entriesList.Add("");

            // Write the message.
            string message = string.Join<Object>(_splitter, entriesList);
            _writer.WriteLine(message);
            LogWritten?.Invoke(this, message);
        }
    }
}
