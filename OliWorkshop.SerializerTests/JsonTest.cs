using Newtonsoft.Json;
using NUnit.Framework;
using OliWorkshop.Serializer.Blobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using static OliWorkshop.SerializerTests.Tests;

namespace OliWorkshop.SerializerTests
{
    public class JsonTest
    {
        /// <summary>
        /// Time to test the iteration
        /// </summary>
        private const int BasicIterationAmount = 4000;

        /// <summary>
        ///  /// Output with best result for 4000 itetarion:
        ///     the time result for json => 21 miliseconds
        ///     the time result for binary => 15 miliseconds
        /// </summary>
        [Test]
        public void TestBrenchmark()
        {
            var valueTest = new Sample();

            valueTest.Value = "is basic 1 ";
            valueTest.Value2 = "is basic 2 this a basic test";
            valueTest.Value3 = "is basic 3 basic value";

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < BasicIterationAmount; i++)
            {
                JsonConvert.SerializeObject(valueTest);
            }
            stopwatch.Stop();

            var stopwatch2 = Stopwatch.StartNew();
            for (int i = 0; i < BasicIterationAmount; i++)
            {
                BlobConvert.SerializeObject(valueTest, SerializerOptions.Default);
            }
            stopwatch2.Stop();

            Console.WriteLine("the time result for json => {0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("the time result for binary => {0}", stopwatch2.ElapsedMilliseconds);
        }

        /// <summary>
        /// Test of un serialize from binary for 4000 itetarion
        /// Output with best result:
        ///     the time result for json => 37 miliseconds
        ///     the time result for binary => 2 miliseconds
        /// </summary>
        [Test]
        public void TestBrenchmark2()
        {
            var valueTest = new Sample();

            valueTest.Value = "is basic 1 ";
            valueTest.Value2 = "is basic 2 this a basic test";
            valueTest.Value3 = "is basic 3 basic value";
            string json = JsonConvert.SerializeObject(valueTest);
            byte[] blob = BlobConvert.SerializeObject(valueTest, SerializerOptions.Default);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < BasicIterationAmount; i++)
            {
                JsonConvert.DeserializeObject(json);
            }
            stopwatch.Stop();

            var stopwatch2 = Stopwatch.StartNew();

            for (int i = 0; i < BasicIterationAmount; i++)
            {
                BlobConvert.DeserializeObject<object>(blob, SerializerOptions.Default);
            }
            stopwatch2.Stop();

            Console.WriteLine("the time result for json => {0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("the time result for binary => {0}", stopwatch2.ElapsedMilliseconds);
        }

        /// <summary>
        /// Test of un serialize from binary for 4000 itetarion
        /// Output with best result:
        ///     the time result for json => 180 miliseconds
        ///     the time result for binary => 6 miliseconds
        ///     
        ///  Nota: the result is very good for binary data
        ///  also the json convert not support ip serialize
        /// </summary>
        [Test]
        public void TestBrenchmark3()
        {
            var valueTest = new Sample2();

            valueTest.Value = DateTime.Now;
            valueTest.Value2 = TimeSpan.FromMilliseconds(22);
            valueTest.ValueGuid = Guid.NewGuid();
            //valueTest.ValueIp = IPAddress.Parse("127.0.0.1"); json not support this convertion
            valueTest.ValueVersion = new Version("1.2.0");

            string json = JsonConvert.SerializeObject(valueTest);

            byte[] blob = BlobConvert.SerializeObject(valueTest, SerializerOptions.Default);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < BasicIterationAmount; i++)
            {
                JsonConvert.DeserializeObject(json);
            }

            stopwatch.Stop();

            var stopwatch2 = Stopwatch.StartNew();

            for (int i = 0; i < BasicIterationAmount; i++)
            {
                BlobConvert.DeserializeObject<object>(blob, SerializerOptions.Default);
            }
            stopwatch2.Stop();

            Console.WriteLine("the time result for json => {0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("the time result for binary => {0}", stopwatch2.ElapsedMilliseconds);
        }

        /// <summary>
        /// Test of un serialize from binary for 4000 itetarion
        /// Output with best result:
        ///     the time result for json => 54 miliseconds
        ///     the time result for binary => 32 miliseconds
        ///     
        ///  Nota: the result is very good for binary data
        ///  also the json convert not support ip serialize
        /// </summary>
        [Test]
        public void TestBrenchmark4()
        {
            var valueTest = new Sample2();

            valueTest.Value = DateTime.Now;
            valueTest.Value2 = TimeSpan.FromMilliseconds(22);
            valueTest.ValueGuid = Guid.NewGuid();
            //valueTest.ValueIp = IPAddress.Parse("127.0.0.1"); json not support this convertion
            valueTest.ValueVersion = new Version("1.2.0");

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < BasicIterationAmount; i++)
            {
                JsonConvert.SerializeObject(valueTest);
            }

            stopwatch.Stop();

            var stopwatch2 = Stopwatch.StartNew();

            for (int i = 0; i < BasicIterationAmount; i++)
            {
                BlobConvert.SerializeObject(valueTest, SerializerOptions.Default);
            }
            stopwatch2.Stop();

            Console.WriteLine("the time result for json => {0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("the time result for binary => {0}", stopwatch2.ElapsedMilliseconds);
        }
    }
}
