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

        ConcurrentQueue<SignalPropagation> Penddings = new ConcurrentQueue<SignalPropagation>();

        Semaphore Semaphore = new Semaphore(0, 1);

        protected Process(CancellationToken cancellation = default, ThreadPriority priority = ThreadPriority.Normal)
        {
            ExecutionThread = new Thread(ExecutionHandle);
            ExecutionThread.Priority = priority;
            Cancellation = cancellation;
        }

        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Execution handle is method to run a internal execution
        /// </summary>
        internal void ExecutionHandle()
        {
            while (!Cancellation.IsCancellationRequested)
            {
                if (Penddings.TryDequeue(out SignalPropagation signal))
                {

                }
                Semaphore.WaitOne();
            }
        }
    }
}
