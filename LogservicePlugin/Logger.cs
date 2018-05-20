using System;
using System.IO;
using System.Text;
using System.Timers;

namespace LogservicePlugin
{
    public class Logger : IDisposable
    {
        private StreamWriter LogStream;
        private Timer t = new Timer(5000);

        public Logger(string LogDir, string LogName)
        {
            string path = Path.Combine(LogDir, LogName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            LogStream = new StreamWriter(new FileStream(Path.Combine(path, $"{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}.log"),
                FileMode.Create, FileAccess.Write), Encoding.Unicode);
            t.Elapsed += Elapsed;
            t.Start();
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            LogStream.Flush();
        }

        public void Write(string s)
        {
            s = s.Replace("\n", "").Replace("\n\r", "");
            lock (LogStream)
            {
                LogStream.WriteLine($"[{DateTime.Now}] {s}");
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            t.Stop();
            LogStream.Flush();
            LogStream.Close();
            t.Dispose();
        }
    }
}
