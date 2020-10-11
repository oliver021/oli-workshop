using NUnit.Framework;
using OliWorkshop.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OliWorkshop.Tests.Threading
{
    public class Tests
    {

        [Test]
        public void ThreadManagerTest()
        {
            // set the max concurrent threads
            var manager = new ThreadManager(4);

            manager.EnqueueAction(delegate { Thread.Sleep(680); Console.WriteLine("action 2"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Console.WriteLine("action 1"); });

            // block this thread to test and that allow the queue to finish
            manager.WaitInBussy();
        }

        [Test]
        public async Task ThreadManagerTestAsync()
        {
            /// this test is similar to <see cref="ThreadManagerTest"/> but this is async version
            // set the max concurrent threads
            var manager = new ThreadManager(4);

            manager.EnqueueAction(delegate { Thread.Sleep(680); Console.WriteLine("action 2"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Console.WriteLine("action 1"); });

            // block this thread to test and that allow the queue to finish
            await manager.WaitInBussyAsync();

            Console.WriteLine("excute here when all action is finished");
        }

        [Test]
        public void TestCancellation()
        {
            /// this test is similar to <see cref="ThreadManagerTest"/> but this is async version
            // set the max concurrent threads
            var manager = new ThreadManager(4, 20);

            manager.EnqueueAction(delegate { Thread.Sleep(680); Console.WriteLine("action 2"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
            manager.EnqueueAction(delegate { Console.WriteLine("action 1"); });

            // stopping the queue process
            manager.StopQueue();

            // block this thread to test and that allow the queue to finish
            manager.WaitInBussy();

            Console.WriteLine("excute here when all action is finished");
        }

        [Test]
        public void ThreadManagerWithRateTest()
        {
            // set the max concurrent threads
            var manager = new ThreadManager(4, 20, 2);

            manager.EnqueueAction(delegate { 
                Thread.Sleep(200);
                Console.WriteLine("action 2 " + " current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            manager.EnqueueAction(delegate { 
                Thread.Sleep(300);
                Console.WriteLine("action 3-1 "+"current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            manager.EnqueueAction(delegate {
                Thread.Sleep(300);
                Console.WriteLine("action 3-2 "+"current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            manager.EnqueueAction(delegate {
                Thread.Sleep(300);
                Console.WriteLine("action 3-3 "+"current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            manager.EnqueueAction(delegate {
                Thread.Sleep(300);
                Console.WriteLine("action 3-4 "+"current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            manager.EnqueueAction(delegate { 
                Console.WriteLine("action 1"+"current thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            });

            // block this thread to test and that allow the queue to finish
            manager.WaitInBussy();
        }

        [Test]
        public void TestExceptionNotification()
        {
            /// this test is similar to <see cref="ThreadManagerTest"/> but this is async version
            // set the max concurrent threads
            var manager = new ThreadManager(4, 5);

            // should throw the exception for overload the queue
            Assert.Catch<QueueFullException>(delegate {

                manager.EnqueueAction(delegate { Thread.Sleep(8680); Console.WriteLine("action 2"); });
                manager.EnqueueAction(delegate { Thread.Sleep(8680); Console.WriteLine("action 2"); });
                manager.EnqueueAction(delegate { Thread.Sleep(8680); Console.WriteLine("action 2"); });
                manager.EnqueueAction(delegate { Thread.Sleep(2980); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(2980); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(2980); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(6680); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(2980); Console.WriteLine("action 3"); });
                manager.EnqueueAction(delegate { Thread.Sleep(1980); Console.WriteLine("action 1"); });

            }, "exception test!!");
        }

        [Test]
        public void TestProperties()
        {
            // set the max concurrent threads
            var manager = new ThreadManager(4);

            manager.EnqueueAction(delegate { Thread.Sleep(4680); Console.WriteLine("action 2"); });

            // block this thread to test and that allow the queue to finish
            manager.WaitInBussy();
        }
    }
}