using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OliWorkshop.Serializer.Blobs
{
    public static class Extensions
    {
        public static byte[] Take(this byte[] arr, int from, int length)
        {
            return arr.Skip(from).Take(length).ToArray();
        }
    }
}
