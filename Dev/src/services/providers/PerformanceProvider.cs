#undef PERF_LOG

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// </summary>
    public class PerformanceProvider
    {
        private static Thread _WorkerThread = null;
        private static ConcurrentQueue<string> _Queue = null;

        private static int _Count = 0;
        private static FileStream _PerfLogFile = null;
        private static StreamWriter _PerfLog = null;
        //private Dictionary<string, long> _Timming = null;
        private List<string> _Timming = null;
        private Stopwatch sw = new Stopwatch();
        private long _PrevElapsed = 0;

        /// <summary>
        /// </summary>
        public PerformanceProvider() {
            //_Timming = new Dictionary<string, long>();
            _Timming = new List<string>();
        }

        /// <summary>
        /// Init the performance log.
        /// </summary>
        /// <param name="rootPath"></param>
        public static void Init(string rootPath)
        {
#if PERF_LOG
            try
            {
                //Console.WriteLine("Services.PerformanceProvider.Init...");

                // Open the output log file...
                if (rootPath != null && _PerfLog == null)
                {
                    if ((_PerfLog = new System.IO.StreamWriter((_PerfLogFile = System.IO.File.Create(rootPath + "\\logs\\performance.log")))) != null)
                    {
                        _PerfLog.WriteLine("\r\n============================================\r\n");
                    }
                }

                // Start the worker thread...
                if (_WorkerThread == null)
                {
                    if ((_WorkerThread = new Thread(_DoWork)) != null)
                    {
                        // Create the queue
                        if ((_Queue = new ConcurrentQueue<string>()) != null)
                        {
                            // Start the thread...
                            _WorkerThread?.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Services.PerformanceProvider.Init:Exception:" + e.Message);
            }
#endif
        }

        /// <summary>
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                return sw?.ElapsedMilliseconds ?? -1;
            }
        }

        /// <summary>
        /// Start performance log.
        /// </summary>
        public void Start()
        {
#if PERF_LOG
            sw?.Start();
            _PrevElapsed = sw?.ElapsedMilliseconds ?? -1;
#endif
        }

        /// <summary>
        /// Add a performance log.
        /// </summary>
        /// <param name="function"></param>
        public void Add(string function)
        {
#if PERF_LOG
            try
            {
                long elapsed = sw?.ElapsedMilliseconds ?? -1;
                if (elapsed != -1)
                {
                    //_Timming?.Add("PostAuthorizationHandler::Handle", elapsed);
                    _Timming?.Add($"{function}::{elapsed}ms(+{elapsed - _PrevElapsed}ms)");
                    _PrevElapsed = elapsed;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Services.PerformanceProvider.Add:Exception:" + e.Message);
            }
#endif
        }

        /// <summary>
        /// Write performance logs to file.
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
#if PERF_LOG
            try
            {
                StringBuilder strBld = null;
                // Trace performance...
                if (_Queue/*_PerfLog*/ != null && _Timming != null && (strBld = new StringBuilder()) != null)
                {
                    strBld.Append($"{path} :{sw.ElapsedMilliseconds}ms\r\n");
                    //foreach (KeyValuePair<string, long> perf in _Timming)
                    //{
                    //    strBld.Append($"\t\t{perf.Key }:{perf.Value}\r\n");
                    //}
                    foreach (string perf in _Timming)
                    {
                        strBld.Append($"  {perf}\r\n");
                    }
                    strBld.Append($"  --------\r\n  {sw.ElapsedMilliseconds}ms\r\n");
                    //_PerfLog.WriteLine(strBld.ToString());
                    _Queue.Enqueue(strBld.ToString());
                }
                if (++_Count % 200 == 0)
                {
                    //Console.WriteLine($"{_Count} requests server.");
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Services.PerformanceProvider.Write:Exception:" + e.Message);
            }
#endif
        }

        /// <summary>
        /// Flush performance log file.
        /// </summary>
        public void Flush()
        {
#if PERF_LOG
            try
            {
                ////_PerfLog?.Flush();
                //_PerfLogFile?.Flush();
                _Queue.Enqueue("flush");
            }
            catch (Exception e)
            {
                //Console.WriteLine("Services.PerformanceProvider.Flush:Exception:" + e.Message);
            }
#endif
        }

        /// <summary>
        /// </summary>
        private static void _DoWork()
        {
#if PERF_LOG
            try
            {
                //Console.WriteLine("Services.PerformanceProvider._DoWork...");
                if (_Queue != null)
                {
                    while (true/*!_shouldStop*/)
                    {
                        string perf = null;
                        if (_Queue.TryDequeue(out perf) == true)
                        {
                            if (perf == "flush")
                            {
                                _PerfLogFile?.Flush();
                            }
                            //else if (perf == "close")
                            //{
                            //    //_PerfLog?.Dispose();
                            //}
                            else
                            {
                                _PerfLog?.WriteLine(perf);
                            }
                        }
                        else
                        {
                            Thread.Sleep(1 * 1000);
                        }
                        //Console.WriteLine("worker thread: working...");
                    }
                }
                //Console.WriteLine("worker thread: terminating gracefully.");
            }
            catch (Exception e)
            {
                //Console.WriteLine("Services.PerformanceProvider._DoWork:Exception:" + e.Message);
            }
#endif
        }
    }
}
