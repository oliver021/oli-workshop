using NUnit.Framework;
using OliWorkshop.Threading.Reactive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OliWorkshop.Tests.Threading
{
    public class TestingBroadcast
    {
        private const string TestChannel = "test-channel";

        [Test]
        public Task TestSimple()
        {
            // initalize with max concurrent the 8 messages
            var broadcasting = new BroadcastConcurrent(8, 1000);

            broadcasting.Subscribe(TestChannel, async m => {
                // emulate a job
                await Task.Delay(100);
                // print a message to inform the task is successful
                Console.WriteLine("print the message: {0}", Encoding.UTF8.GetString(m.GetContentInBytes()));
            });

            // post 8 messages
            broadcasting.PostString(TestChannel, "hello in real time");
            broadcasting.PostString(TestChannel, "hello in real time 2");
            broadcasting.PostString(TestChannel, "hello in real time 3");
            broadcasting.PostString(TestChannel, "hello in real time 4");
            broadcasting.PostString(TestChannel, "hello in real time 5");
            broadcasting.PostString(TestChannel, "hello in real time 6");
            broadcasting.PostString(TestChannel, "hello in real time 7");
            broadcasting.PostString(TestChannel, "hello in real time 8");

            // wait for 110 miliseconds
            return Task.Delay(110);
        }

        [Test]
        public Task TestCancellation()
        {
            // basic index iteration
            int i = 1;

            // initalize with max concurrent the 8 messages
            var broadcasting = new BroadcastConcurrent(1, 1000);

            broadcasting.Subscribe(TestChannel, async m => {
                
                // emulate a job
                await Task.Delay(i*250);

                // print a message to inform the task is successful
                Console.WriteLine("print the message: {0}",m.GetString());
                
                // increment time awating
                i++;
            });

            // post 8 messages
            broadcasting.PostString(TestChannel, "hello in real time 1");
            broadcasting.PostString(TestChannel, "hello in real time 2");
            broadcasting.PostString(TestChannel, "hello in real time 3");
            broadcasting.PostString(TestChannel, "hello in real time 4");
            broadcasting.PostString(TestChannel, "not should be show");

            Task.Delay(1000).ContinueWith(prev => {

                // close the broadcasting
                broadcasting.Close(); 
            });

            // wait for 110 miliseconds
            return broadcasting.Process;
        }
    }
}
