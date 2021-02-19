using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OliWorkshop.Threading
{
    /// <summary>
    /// Multi-signal semaphore router to manage complex flow
    /// and create redirection, rule thread to complete and more
    /// 
    /// You can pass a enum type as argument to instance that define your
    /// routes
    /// 
    /// </summary>
    /// <typeparam name="TSemaphore"></typeparam>
    public class SmartSemaphore<TSemaphore> : IDisposable
    {
        /// <summary>
        /// constructor that you can use without parameters or
        /// set the basic initial counter that for default is zero
        /// </summary>
        /// <param name="initialCount"></param>
        /// <param name="maxCount"></param>
        public SmartSemaphore(int initialCount = 0)
        {
            // get the type argument instance
            Type enumType = typeof(TSemaphore);

            // check the validation of TSemaphore
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(nameof(TSemaphore) + "should be a enum type.");
            }

           // fetch the record router
           Router = Enum.GetNames(enumType)
                .Select(key => (key, new SemaphoreSlim(initialCount)))
                .ToDictionary(x => x.key, x => x.Item2);
        }

        /// <summary>
        /// Require the semaphore parameters to initialize intances
        /// </summary>
        /// <param name="initialCount"></param>
        /// <param name="maxCount"></param>
        public SmartSemaphore(int initialCount, int maxCount)
        {
            // get the type argument instance
            Type enumType = typeof(TSemaphore);

            // check the validation of TSemaphore
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(nameof(TSemaphore) + "should be a enum type.");
            }

            // fetch the record router
            Router = Enum.GetNames(enumType)
                 .Select(key => (key, new SemaphoreSlim(initialCount, maxCount)))
                 .ToDictionary(x => x.key, x => x.Item2);
        }

        /// <summary>
        /// router reference for this class
        /// </summary>
        private Dictionary<string, SemaphoreSlim> Router { get; }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// </summary>
        /// <param name="signal"></param>
        public void Wait(TSemaphore signal)
        {
            Router[signal.ToString()].Wait();
        }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// and milliseconds timeout rule
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public bool Wait(TSemaphore signal, int millisecondsTimeout) 
        {
            return Router[signal.ToString()].Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// and milliseconds timeout rule and cancellation token
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool Wait(TSemaphore signal, int millisecondsTimeout, CancellationToken cancellationToken) 
        {
            return Router[signal.ToString()].Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// and TimeSpan timeout rule
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Wait(TSemaphore signal, TimeSpan timeout)
        {
            return Router[signal.ToString()].Wait(timeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// and TimeSpan timeout rule and cancellation token
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool Wait(TSemaphore signal, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Router[signal.ToString()].Wait(timeout, cancellationToken);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type
        /// and cancellation token
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="cancellationToken"></param>
        public void Wait(TSemaphore signal, CancellationToken cancellationToken)
        {
            Router[signal.ToString()].Wait(cancellationToken);
        }

        /// <summary>
        /// If the semaphore is locked then continue but if not then redirect to function
        /// Note: warning with loop with true value because you can build an severity problems
        /// with infinite loop
        /// <para>
        ///     This method is good to create polling flow
        /// </para>
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="loop"></param>
        /// <param name="redirection"></param>
        public void ContinueOrRedirect(TSemaphore signal, bool loop, Action redirection)
        {
            var route = Router[signal.ToString()];
            redirection:
                /// check if redirection is necesary
                if (route.CurrentCount < 1)
                {
                    redirection.Invoke();
                    if (route.CurrentCount < 1 && loop)
                    {
                        goto redirection;
                    }

                    // if loop is disable then await for route signal
                    route.Wait();
                }
                else
                {
                    // continue for this route, Wait method is necesary
                    // is possible in multi-threading conttext that your thread read
                    // from route.CurrentCount that there is one avaliable route
                    // but when your thread come here and this avaliable road is close
                    // for other thrad then that is necesary as fallback strategy
                    // wait one miliseconds if not pass then the redirection is necesary
                    bool grantless = !route.Wait(1);

                    if (grantless)
                    {
                        goto redirection;
                    }
                }
        }

        /// <summary>
        /// If the semaphore is locked then continue but if not then redirect to function
        /// Note: warning with loop with true value because you can build an severity problems
        /// with infinite loop
        /// <para>
        ///     This method is good to create polling flow
        /// </para>
        /// <para>
        ///     This method is version async of <see cref="ContinueOrRedirect(TSemaphore, bool, Action)"/>
        /// </para>
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="loop"></param>
        /// <param name="redirection"></param>
        public Task ContinueOrRedirectAsync(TSemaphore signal, bool loop, Action redirection)
        {
            var route = Router[signal.ToString()];

            // source to build the task that represent the successful operation
            var source = new TaskCompletionSource<byte>();

            // delegate this operation on other thread avaliable
            ThreadPool.QueueUserWorkItem(_ =>
            {
                redirection:
                    /// check if redirection is necesary
                    if (route.CurrentCount < 1)
                    {
                        redirection.Invoke();
                        if (route.CurrentCount < 1 && loop)
                        {
                            goto redirection;
                        }

                        // if loop is disable then await for route signal
                        route.Wait();
                    }
                    else
                    {
                        // continue for this route, Wait method is necesary
                        // is possible in multi-threading conttext that your thread read
                        // from route.CurrentCount that there is one avaliable route
                        // but when your thread come here and this avaliable road is close
                        // for other thread then that is necesary as fallback strategy
                        // wait one miliseconds if not pass then the redirection is necesary
                        bool grantless = !route.Wait(1);

                        if (grantless)
                        {
                            goto redirection;
                        }
                    }

                // set the result to complete the task
                source.SetResult(1);
            });

            // build the task to complete this redirection
            return source.Task;
        }

        /// <summary>
        /// This method create a circuit of time where the timeout parameter
        /// determine the max time to wait for a signal if this signal is not release
        /// in this timeout then the thread is redirect to an action
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="loop"></param>
        /// <param name="redirection"></param>
        public void WaitOrRedirect(TSemaphore signal, int timeout, Action redirection)
        {
            var route = Router[signal.ToString()];

            redirection:

            // get true if the thread enter for this route
            bool success = route.Wait(timeout);

            // if the current thread not enter then this is redirected
            if (!success)
            {
                redirection.Invoke();

                goto redirection;
            }
        }

        /// <summary>
        /// This method create a circuit of time where the timeout parameter
        /// determine the max time to wait for a signal if this signal is not release
        /// in this timeout then the thread is redirect to an action
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="loop"></param>
        /// <param name="redirection"></param>
        public void WaitOrRedirect(TSemaphore signal, TimeSpan timeout, Action redirection)
        {
            var route = Router[signal.ToString()];

            redirection:

            // get true if the thread enter for this route
            bool success = route.Wait(timeout);

            // if the current thread not enter then this is redirected
            if (!success)
            {
                redirection.Invoke();

                goto redirection;
            }
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// </summary>
        /// <param name="signal"></param>
        public Task WaitAsync(TSemaphore signal)
        {
            return Router[signal.ToString()].WaitAsync();
        }

        /// <summary>
        /// Awaiter for multiple-signals where all should be complete
        /// </summary>
        /// <param name="signal"></param>
        public Task WaitForAllAsync(params TSemaphore[] signals)
        {
            return Task.WhenAll(
                signals.Select(x => Router[x.ToString()].WaitAsync())
            );
        }

        /// <summary>
        /// Awaiter for multiple-signals where all should be complete
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task WaitForAllAsync(IEnumerable<TSemaphore> signals, CancellationToken token = default)
        {
            return Task.WhenAll(
                signals.Select(x => Router[x.ToString()].WaitAsync(token))
            );
        }

        /// <summary>
        /// Awaiter for multiple-signals where just one should be complete
        /// </summary>
        /// <param name="signal"></param>
        public async Task<TSemaphore> WaitForAnyAsync(params TSemaphore[] signals)
        {
            // fetch the task with signal emmited
            var first = await Task.WhenAny(
                signals.Select(
                    x => Router[x.ToString()].WaitAsync().ContinueWith(prev => {
                        return x;
                    })
                )
            );

            // return the first signal that was completed
            return await first;
        }

        /// <summary>
        /// Awaiter for multiple-signals where just one should be complete
        /// </summary>
        /// <param name="signal"></param>
        public async Task<TSemaphore> WaitForAnyAsync(IEnumerable<TSemaphore> signals, CancellationToken token = default)
        {
            // fetch the task with signal emmited
            var first = await Task.WhenAny(
               signals.Select(
                   x => Router[x.ToString()].WaitAsync(token).ContinueWith(prev => {
                       return x;
                   })
               )
            );

            // return the first signal that was completed
            return await first;
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// and TimeSpan timeout rule
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public Task<bool> WaitAsync(TSemaphore signal, int millisecondsTimeout) 
        {
            return Router[signal.ToString()].WaitAsync(millisecondsTimeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// and milliseconds timeout rule with cancellation token
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> WaitAsync(TSemaphore signal, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return Router[signal.ToString()].WaitAsync(millisecondsTimeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// and cancellation token
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task WaitAsync(TSemaphore signal, CancellationToken cancellationToken)
        {
            return Router[signal.ToString()].WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// and TimeSpan timeout rule and cancellation token
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<bool> WaitAsync(TSemaphore signal, TimeSpan timeout)
        {
            return Router[signal.ToString()].WaitAsync(timeout);
        }

        /// <summary>
        /// Simple wait for the signal semaphore type for async method version
        /// for timeout and cancellation token
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> WaitAsync(TSemaphore signal, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Router[signal.ToString()].WaitAsync(timeout, cancellationToken);
        }

        /// <summary>
        /// Open the signal
        /// </summary>
        /// <param name="signal"></param>
        public void Release(TSemaphore signal)
        {
            Router[signal.ToString()].Release();
        }

        /// <summary>
        /// Open the signal
        /// </summary>
        /// <param name="signal"></param>
        public void Release(params TSemaphore[] signals)
        {
            // iteration for signals to emit
            foreach (var item in signals)
            {
                Router[item.ToString()].Release();
            }
        }

        /// <summary>
        /// Open the signal with counter
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="count"></param>
        public void Release(TSemaphore signal, int count)
        {
            Router[signal.ToString()].Release(count);
        }

        /// <summary>
        /// Dispose implementation
        /// </summary>
        public void Dispose()
        {
            foreach (var item in Router.Values)
            {
                item.Dispose();
            }
        }
    }
}
