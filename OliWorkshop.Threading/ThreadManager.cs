using System;
using System.Collections.Concurrent;
using System.Threading;

namespace OliWorkshop.Threading
{
    /// <summary>
    /// This class contains a helper that can help with complex concurrent long jobs
    /// </summary>
    public class ThreadManager
    {
        /// <summary>
        /// Simple required thread amount pooling
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads)
        {
            Threads = new Thread[threads];
            ThreadsCount = threads;
            SemaphoreSlim = new SemaphoreSlim(0, threads);
        }

        int Index = 0;

        Thread[] Threads { get; }

        int ThreadsCount { get; }

        int StatusWork = 0;

        public bool Running { get; private set; }

        SemaphoreSlim SemaphoreSlim;

        ConcurrentQueue<Action> ConcurrentQueue = new ConcurrentQueue<Action>();

        CancellationTokenSource Cancellation = new CancellationTokenSource();

        /// <summary>
        /// Put in queue a action to wait its execution and running in background
        /// </summary>
        /// <param name="action"></param>
        public void EnqueueAction(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Index < ThreadsCount && !HasAvaliable() )
            {
                Threads[Index] = new Thread(ThreadRunner);
                Threads[Index].Start();
                Index++;
            }

            // set an increment to inform there is a job
            Interlocked.Increment(ref StatusWork);

            // register new jobs in queue
            ConcurrentQueue.Enqueue(action);

            // to send signal to inform to the threads that there is a jobs
            SemaphoreSlim.Release();
        }

        /// <summary>
        /// Stop all thread that been executing the queue
        /// </summary>
        public void StopQueue()
        {
            // simpel invoke the cancellation method
            Cancellation.Cancel();
        }

        /// <summary>
        /// This function is a helper to check if exist a thread avaliable to queue
        /// </summary>
        /// <returns></returns>
        private bool HasAvaliable()
        {
            for (int i = 0; i < Index; i++)
            {
                if (Threads[i] is null)
                {
                    return false;

                }
                if (Threads[i] != null &&
                    !Threads[i].IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Block the current thread until the queue 
        /// is empty or the thread manager is cancellation requested
        /// </summary>
        public void WaitWhileBussy()
        {
            check:
            if (Cancellation.Token.IsCancellationRequested || StatusWork < 1 ) {
                return;
            }
            Thread.Sleep(100);
            goto check;
        }

        /// <summary>
        /// The thread Runner is a method to set the thread propurse
        /// </summary>
        /// <param name="obj"></param>
        private void ThreadRunner(object obj)
        {
            // execution loop
            while (!Cancellation.IsCancellationRequested) {

                // wait for the queue info
                SemaphoreSlim.Wait(Cancellation.Token);

                // try get a action from the queue
                if (ConcurrentQueue.TryDequeue(out Action current))
                {
                    // check first that the cancellation is not requested
                    Cancellation.Token.ThrowIfCancellationRequested();

                    // invoke the action
                    current.Invoke();

                    // free counter by this thread
                    Interlocked.Decrement(ref StatusWork);
                }
            }
            Thread.CurrentThread.Abort();
        }
    }
}
