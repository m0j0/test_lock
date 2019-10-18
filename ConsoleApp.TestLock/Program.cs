using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.TestLock
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                var test0 = new Test();
                test0.Do(0);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var test1 = new Test();
                test1.Do(1);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
    }

    public class Test
    {
        private long _counter;

        public void Do(int opt)
        {
            const int count = 10000;

            var threads = new Thread[count];

            if (opt == 0)
            {
                var lockObj = new Lock();
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        var t = lockObj.GetServiceResultAsync();
                        Interlocked.Increment(ref _counter);
                    });
                }
            }
            else
            {
                var interlock = new Interlock();

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        var t = interlock.GetServiceResultAsync();
                        Interlocked.Increment(ref _counter);
                    });
                }
            }

            var stopwatch = Stopwatch.StartNew();

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();

            Console.WriteLine(_counter);
            Console.WriteLine(stopwatch.Elapsed.ToString());
        }
    }
}