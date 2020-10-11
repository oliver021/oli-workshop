using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OliWorkshop.Threading
{
    /// <summary>
    /// This class allow create many model types of timer to execute action
    /// by intervals of time
    /// </summary>
    public class TimerFactory
    {
        /// <summary>
        /// Make time interval from a number of iteration
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="miliseconds"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static async Task MakeInterval(Action execution, int miliseconds, int iteration = 1)
        {
            if (iteration < 1)
            {
                throw new ArgumentException(nameof(iteration) + "can be zero as value");
            }
            while (iteration < 1)
            {
                // make a interval by task
                await Task.Delay(miliseconds);

                // invoke the execution action
                execution.Invoke();

                // decrement the iteration
                iteration--;
            }
        }

        /// <summary>
        /// Make time interval from a number of iteration
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="miliseconds"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static Task MakeIntervalSafe(Action execution, int miliseconds, int iteration = 1)
        {
            if (iteration < 1)
            {
                throw new ArgumentException(nameof(iteration) + "can be zero as value");
            }

            // create the task source
            var source = new TaskCompletionSource<byte>();

            // pooling the interval task
            ThreadPool.QueueUserWorkItem(delegate {

                // create locker manager
                // note: the max SemaphoreSlim limit should be the number iteration for safe mode
                var slim = new SemaphoreSlim(0, 1);

                again:
                    
                // this check should do
                    if (iteration < 1)
                    {
                        source.SetResult(1);
                        try
                        {
                            source.SetResult(1);
                            slim.Dispose();
                        }
                         catch (Exception)
                        {
                            // ignore
                        }
                        return;
                    }

                    // make a interval by task
                    Task.Delay(miliseconds).ContinueWith(prev => {

                        // free next iteration
                        slim.Release();

                        // invoke the execution action
                        execution.Invoke();
                    });
                    
                    // block for the new
                    slim.Wait();

                goto again;
             });

            // return the task
            return source.Task;
        }

        /// <summary>
        /// Make time interval base on cancellation token
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="miliseconds"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task MakeInterval(Action execution, int miliseconds, CancellationToken cancellation = default)
        {
            // if cancellation is requested then not make interval
            cancellation.ThrowIfCancellationRequested();

            // loop to build the interval
            while (!cancellation.IsCancellationRequested)
            {
                // make a interval by task
                await Task.Delay(miliseconds);

                // check token again
                cancellation.ThrowIfCancellationRequested();
                
                // invoke the execution action
                execution.Invoke();
            }
        }

        /// <summary>
        /// Make time interval from a number of iteration
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="miliseconds"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static Task MakeInterval(Action execution, TimeSpan time, int iteration = 1)
        {
            return MakeInterval(execution, time.Milliseconds, iteration);
        }

        /// <summary>
        /// Make time interval base on cancellation token
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="miliseconds"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static Task MakeInterval(Action execution, TimeSpan time, CancellationToken cancellation = default)
        {
            return MakeInterval(execution, time.Milliseconds, cancellation);
        }
    }
}
