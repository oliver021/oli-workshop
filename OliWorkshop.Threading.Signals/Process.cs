using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;

namespace OliWorkshop.Threading.Signals
{
    /// <summary>
    /// The process represent an artifacts a handler signal
    /// </summary>
    public abstract class Process : CriticalFinalizerObject
    {
        /// <summary>
        /// The reference a thread
        /// </summary>
        Thread ExecutionThread = null;

        ConcurrentQueue<Signal> Penddings = new ConcurrentQueue<Signal>();

        Semaphore Semaphore = new Semaphore(0, 1);

        protected Process(ThreadPriority priority = ThreadPriority.Normal, CancellationToken cancellation = default)
        {
            ExecutionThread = new Thread(ExecutionHandle);
            ExecutionThread.Priority = priority;
            Cancellation = cancellation;
        }

        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Execution handle is method to run a internal execution
        /// </summary>
        private void ExecutionHandle()
        {
            while (!Cancellation.IsCancellationRequested)
            {
                if (Penddings.TryDequeue(out Signal signal))
                {

                }
                Semaphore.WaitOne();
            }
        }
    }
}
