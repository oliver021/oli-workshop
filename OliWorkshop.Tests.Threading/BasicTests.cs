using NUnit.Framework;
using OliWorkshop.Threading;
using System;
using System.Threading;

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
            manager.WaitWhileBussy();

        }
    }
}