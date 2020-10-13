using NUnit.Framework;
using OliWorkshop.Serializer.Blobs;
using System;
using System.Net;

namespace OliWorkshop.SerializerTests
{
    public class Tests
    {
        public class Sample {

            public string Value;
            public string Value2;
            public string Value3;

        }

        public class Sample2
        {
            public DateTime Value;
            public TimeSpan Value2;
            public Boolean ValueBool = true;
            public IPAddress ValueIp = null;
            public Guid ValueGuid;
            public Version ValueVersion;

        }

        [Test]
        public void TestWithString()
        {
            var valueTest = new Sample();

            valueTest.Value = "is basic 1";
            valueTest.Value2 = "is basic 2";
            valueTest.Value3 = "is basic 3";

            var blob = BlobConvert.SerializeObject(valueTest, SerializerOptions.Default) ;

            var finalValue = BlobConvert.DeserializeObject<Sample>(blob, SerializerOptions.Default);

            Console.WriteLine("result 1: {0}", finalValue.Value);
            Console.WriteLine("result 2: {0}", finalValue.Value2);
        }

        [Test]
        public void TestWithParse()
        {
            var valueTest = new Sample2();

            valueTest.Value = DateTime.Now;
            valueTest.Value2 = TimeSpan.FromMilliseconds(22);
            valueTest.ValueGuid = Guid.NewGuid();
            valueTest.ValueIp = IPAddress.Parse("127.0.0.1");
            valueTest.ValueVersion = new Version("1.2.0");

            var blob = BlobConvert.SerializeObject(valueTest, SerializerOptions.Default);

            var finalValue = BlobConvert.DeserializeObject<Sample2>(blob, SerializerOptions.Default);

            Assert.IsTrue(finalValue.ValueBool);
            Assert.AreEqual(valueTest.Value.Second, finalValue.Value.Second); // exists impresition for miliseconds
            Assert.AreEqual(valueTest.Value2, finalValue.Value2);             // but this is good for this case
            Assert.AreEqual(valueTest.ValueGuid, finalValue.ValueGuid);
            Assert.AreEqual(valueTest.ValueIp, finalValue.ValueIp);
            Assert.AreEqual(valueTest.ValueVersion, finalValue.ValueVersion);
        }
    }
}