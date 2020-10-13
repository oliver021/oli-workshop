using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Serializer.Blobs
{
    public class ArrayTracker
    {
        public byte[] buffer;

        public int capacity;

        public int record;

        public ArrayTracker(int rate)
        {
            Rate = rate;
            capacity = Rate;
            buffer = new byte[Rate];
            record = 0;
        }

        public int Rate { get; }


        public void WriteOnce(byte code) {
            if (capacity < (1 + record))
            {
                Array.Resize(ref buffer, Rate);
                capacity += Rate;
            }

            buffer[record] = code;
            record++;

        }

        public void WriteBuffer(byte code, byte[] pack)
        {
            var nextWrite = 1 + pack.Length;

            if (capacity < (nextWrite+record))
            {
                Array.Resize(ref buffer, Rate);
                capacity += Rate;
            }

            buffer[record] = code;
            record += 1;
            Array.Copy(pack, 0, buffer, record, pack.Length);
            record += pack.Length;
        }

        public void WriteBuffer(byte code, byte[] prefix, byte[] pack)
        {
            var nextWrite = 1 + pack.Length + prefix.Length;

            if (capacity < (nextWrite + record))
            {
                Array.Resize(ref buffer, Rate);
                capacity += Rate;
            }

            buffer[record] = code;
            record += 1;
            Array.Copy(prefix, 0, buffer, record, prefix.Length);
            record += prefix.Length;
            Array.Copy(pack, 0, buffer, record, pack.Length);
            record += pack.Length;
        }
    }
}
