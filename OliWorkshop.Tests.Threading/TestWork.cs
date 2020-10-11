using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using  NUnit.Framework;

namespace OliWorkshop.Tests.Threading
{
    public class Tests2
    {

        [Test]
        public void Test1()
        {
            var SemaphoreSlim = new SemaphoreSlim(0);

            Parallel.Invoke(delegate {

                SemaphoreSlim.Wait();

                Console.WriteLine("The success state: 1");
            },
            delegate {
                Thread.Sleep(300);
                SemaphoreSlim.Wait();

                Console.WriteLine("The success state: 2");
            },
            delegate {
                Thread.Sleep(890);
                Console.WriteLine("The send signal 1");
                SemaphoreSlim.Release();
                Thread.Sleep(890);
                Console.WriteLine("The send signal 2");
                SemaphoreSlim.Release();

            });


        }
    }
}
