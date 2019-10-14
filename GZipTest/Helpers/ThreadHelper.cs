using System;
using System.Threading;

namespace GZipTest.Helpers
{
    public static class ThreadHelper
    {
        public static Thread RunGuarded(Action threadHandler, Action<Exception> exceptionHandler, int id = -1)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    threadHandler?.Invoke();
                }
                catch (Exception ex)
                {
                    exceptionHandler?.Invoke(ex);
                }
            })
            {
                Name = id.ToString(),
                IsBackground = false,
                Priority = ThreadPriority.Normal
            };
            thread.Start();
            return thread;
        }
        public static void WaitThreads(params Thread[] threads)
        {
            if (threads == null) throw new ArgumentNullException(nameof(threads));
            foreach (Thread thread in threads)
            {
                if (thread != null && thread.IsAlive) thread.Join();
            }
        }
    }
}
