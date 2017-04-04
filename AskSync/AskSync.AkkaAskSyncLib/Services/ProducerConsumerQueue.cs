using System;
using System.Collections.Generic;
using System.Threading;

namespace AskSync.AkkaAskSyncLib.Services
{
    /*

        todo try this
       static void Main()
{
using (ProducerConsumerQueue q = new ProducerConsumerQueue())
{
  q.EnqueueTask ("Hello");
  for (int i = 0; i < 10; i++) q.EnqueueTask ("Say " + i);
  q.EnqueueTask ("Goodbye!");
}

// Exiting the using statement calls q's Dispose method, which
// enqueues a null task and waits until the consumer finishes.
}
       */

    public class ProducerConsumerQueue : IDisposable
    {
        private readonly object _locker = new object();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private readonly Thread _worker;
        private readonly Queue<string> _tasks = new Queue<string>();

        public ProducerConsumerQueue()
        {
            _worker = new Thread(Work);
            _worker.Start();
        }

        public void Dispose()
        {
            EnqueueTask(null); // Signal the consumer to exit.
            _worker.Join(); // Wait for the consumer's thread to finish.
            _wh.Close(); // Release any OS resources.
        }

        public void EnqueueTask(string task)
        {
            lock (_locker) _tasks.Enqueue(task);
            _wh.Set();
        }

        private void Work()
        {
            while (true)
            {
                string task = null;
                lock (_locker)
                    if (_tasks.Count > 0)
                    {
                        task = _tasks.Dequeue();
                        if (task == null) return;
                    }
                if (task != null)
                {
                    Console.WriteLine("Performing task: " + task);
                    Thread.Sleep(1000); // simulate work...
                }
                else
                    _wh.WaitOne(); // No more tasks - wait for a signal
            }
        }
    }
}