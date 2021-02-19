using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Serializer.Blobs
{
    /// <summary>
    /// The array tracker allow count the record of index
    /// to check the buffer storage
    /// </summary>
    public class ArrayTracker
    {
        public byte[] buffer;

        public int capacity;

        public int record;

        /// <summary>
        /// Requiere a rate resize
        /// </summary>
        /// <param name="rate"></param>
        public ArrayTracker(int rate)
        {
            Rate = rate;
            capacity = Rate;
            buffer = new byte[Rate];
            record = 0;
        }

        /// <summary>
        /// The main Rate to resize the buffer when this need more memory usage
        /// </summary>
        public int Rate { get; }

        /// <summary>
        /// Write a simple byte
        /// </summary>
        /// <param name="code"></param>
        public void WriteOnce(byte code) {
            if (capacity < (1 + record))
            {
                Array.Resize(ref buffer, Rate);
                capacity += Rate;
            }

            buffer[record] = code;
            record++;

        }

        /// <summary>
        /// Bytes array to append at end the buffer
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pack"></param>
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

        /// <summary>
        /// Bytes array to append at end the buffer with bytes prefix
        /// like length info or subtypes specifications
        /// </summary>
        /// <param name="code"></param>
        /// <param name="prefix"></param>
        /// <param name="pack"></param>
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
