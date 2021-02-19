using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OliWorkshop.Threading
{
    /// <summary>
    /// This class contains a helper that can help with complex concurrent long jobs
    /// and handle a complex workflow
    /// </summary>
    public class ThreadManager
    {
        /// <summary>
        /// Indicate if the queue is full capacity
        /// </summary>
        public bool HasMoreCapacity => SemaphoreSlim.CurrentCount < MaxCapacity;

        /// <summary>
        /// Indicate if the queue is running
        /// </summary>
        public bool Running => StatusWork < 1;

        /// <summary>
        /// Indicate the number the pedding actions
        /// </summary>
        public int Pendding => StatusWork;

        /// <summary>
        /// Indicate the number the threads used
        /// </summary>
        public int ThreadsUsed => Index;

        /// <summary>
        /// Index to count the avaliable threads
        /// </summary>
        int Index = 0;

        /// <summary>
        /// Array Storage of threads
        /// </summary>
        Thread[] Threads { get; }

        /// <summary>
        /// the number the max threads
        /// </summary>
        int ThreadsCount { get; }

        /// <summary>
        /// The indicator of jobs avaliables
        /// </summary>
        int StatusWork = 0;

        /// <summary>
        /// The action rate define actions per threads
        /// </summary>
        int ActionRate { get; } = 0;

        /// <summary>
        /// The default Priority to threads
        /// </summary>
        ThreadPriority Priority { get; } = ThreadPriority.Normal;

        /// <summary>
        /// The basic primitive to manage the concurrent flow
        /// </summary>
        SemaphoreSlim SemaphoreSlim;

        /// <summary>
        /// The Storage Queue of actions
        /// </summary>
        ConcurrentQueue<Action> ConcurrentQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Global Token Source to manage the cancellation
        /// </summary>
        CancellationTokenSource Cancellation = new CancellationTokenSource();

        /// <summary>
        /// Storage the finalizers action to manage flow
        /// </summary>
        ConcurrentBag<Action> Finalizers = new ConcurrentBag<Action>();

        /// <summary>
        /// The basic delegate to store the thread main execution logic
        /// </summary>
        ParameterizedThreadStart ThreadExecution;

        /// <summary>
        /// info the max capacity for queue
        /// </summary>
        public int MaxCapacity { get; }

        /// <summary>
        /// Simple required thread amount pooling and max rate of actions
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads)
        {
            Threads = new Thread[threads];
            ThreadsCount = threads;
            SemaphoreSlim = new SemaphoreSlim(0, threads);
            ThreadExecution = ThreadRunner;
            MaxCapacity = threads;
        }

        /// <summary>
        /// Simple required thread amount pooling and max rate of actions
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads, int maxActions)
        {
            Threads = new Thread[threads];
            ThreadsCount = threads;
            SemaphoreSlim = new SemaphoreSlim(0, maxActions);
            ThreadExecution = ThreadRunner;
            MaxCapacity = maxActions;
        }

        /// <summary>
        /// Simple required thread amount pooling, max rate of actions and rate action per thread 
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads, int maxActions, int actionRate) : this(threads, maxActions)
        {
            ActionRate = actionRate;
            ThreadExecution = ThreadRunnerWithRate;
        }

        /// <summary>
        /// Simple required thread amount pooling, max rate of actions and the threads priority
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads, int maxActions, ThreadPriority priority) : this(threads, maxActions)
        {
            Priority = priority;
            ThreadExecution = ThreadRunner;
        }

        /// <summary>
        /// Fully construct to set all basic parameters
        /// </summary>
        /// <param name="threads"></param>
        public ThreadManager(int threads, int maxActions, int actionRate, ThreadPriority priority) : this(threads, maxActions)
        {
            Priority = priority;
            ActionRate = actionRate;

            // set the basic thread runner base on action rate support
            if (actionRate > 0)
            {
                ThreadExecution = ThreadRunnerWithRate;
            }
            else
            {
                ThreadExecution = ThreadRunner;
            }
        }

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
                // create a new thread instance
                 var current = new Thread(ThreadExecution);
                
                // set the priority
                if (Priority != ThreadPriority.Normal)
                {
                    current.Priority = Priority;
                }

                // run the new thread
                current.Start();

                // store the new thread
                Threads[Index] = current;

                // increment the index
                Index++;
            }

            // set an increment to inform there is a job
            Interlocked.Increment(ref StatusWork);

            // register new jobs in queue
            ConcurrentQueue.Enqueue(action);

            // try release
            try
            {
                // to send signal to inform to the threads that there is a jobs
                SemaphoreSlim.Release();
            }
            catch (SemaphoreFullException)
            {
                // throw the exception to inform the problem
                throw new QueueFullException();
            }
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

            // not avaliable
            return false;
        }

        /// <summary>
        /// Block the current thread until the queue 
        /// is empty or the thread manager is cancellation requested
        /// </summary>
        public void WaitInBussy()
        {
            check:
            if (Cancellation.Token.IsCancellationRequested || StatusWork < 1 ) {
                return;
            }
            Thread.Sleep(40);
            goto check;
        }

        /// <summary>
        /// Block the current thread until the queue 
        /// is empty or the thread manager is cancellation requested
        /// </summary>
        public Task WaitInBussyAsync(CancellationToken cancellation = default)
        {
            // create a task completion source
            var source = new TaskCompletionSource<bool>();
            
            // enqueue by pooling this task
            WhenFinish(delegate {
                check:
                    // is possible also stop this task by cancellation token by argument
                    if (cancellation.IsCancellationRequested || Cancellation.Token.IsCancellationRequested || StatusWork < 1)
                    {
                        source.SetResult(true);
                    }
                    Thread.Sleep(40);
                    if (!source.Task.IsCompleted)
                    {
                        goto check;
                    }
            });

            return source.Task;
        }

        /// <summary>
        /// Add a simple hook to execute an action when the queue is empty once time
        /// then you can add more action after of once time the queue is finalize
        /// </summary>
        /// <param name="action"></param>
        private void WhenFinish(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // register a new finalizer
            Finalizers.Add(action);
        }

        /// <summary>
        /// The thread Runner is a method to set the thread propurse
        /// </summary>
        /// <param name="obj"></param>
        private void ThreadRunner(object _)
        {
            // execution loop
            try
            {
                ExecuterLoopWithoutRate();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        /// <summary>
        /// The thread Runner is a method to set the thread propurse and with rate support
        /// </summary>
        /// <param name="obj"></param>
        private void ThreadRunnerWithRate(object _)
        {
            // execution loop
            ExecuterLoopWithRate();
        }

        /// <summary>
        /// The execution loop without rate of actions
        /// </summary>
        private void ExecuterLoopWithRate()
        {
            // this array is dedicate per thread
            Action[] bagArray = new Action[ActionRate];

            // main execution that implement rate support
            while (!Cancellation.IsCancellationRequested)
            {
                // declare the index integer
                int index = 0;

                // wait for the queue info
                SemaphoreSlim.Wait(Cancellation.Token);

                // recollector loop
                while (index < ActionRate && ConcurrentQueue.TryDequeue(out Action current))
                {
                    bagArray[index] = current;

                    // increment local counter to track the rate limit
                    index++;
                }

                // execution stored actions
                for (int i = 0; i < index; i++)
                {
                    // run action by class method dedicate
                    RunAction(bagArray[i]);
                }
            }
        }

        /// <summary>
        /// The execution loop with rate of actions
        /// </summary>
        private void ExecuterLoopWithoutRate()
        {
            while (!Cancellation.IsCancellationRequested)
            {
                // wait for the queue info
                SemaphoreSlim.Wait(Cancellation.Token);

                // try get a action from the queue
                if (ConcurrentQueue.TryDequeue(out Action current))
                {
                    // run action by class method dedicate
                    RunAction(current);

                    if (StatusWork < 1)
                    {
                        ExecuteFinalizers();
                    }
                }
            }
        }

        /// <summary>
        /// Execute the action passed as argument
        /// </summary>
        /// <param name="current"></param>
        private void RunAction(Action current)
        {
            // check first that the cancellation is not requested
            Cancellation.Token.ThrowIfCancellationRequested();

            // control above the action execution
            try
            {
                // invoke the action
                current.Invoke();
            }
            catch (Exception)
            { }
            finally
            {
                // free counter by this thread
                Interlocked.Decrement(ref StatusWork);
            }
        }

        /// <summary>
        /// This is a helper to execute all action when
        /// </summary>
        private void ExecuteFinalizers()
        {
            foreach (var action in Finalizers)
            {
                action.Invoke();
            }

            // remove all finalizers
            Finalizers.Clear();
        }
    }
}
