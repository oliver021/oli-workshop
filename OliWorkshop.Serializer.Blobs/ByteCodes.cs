using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Serializer.Blobs
{
    public static class ByteCodes
    {
        public const byte Null = 0;
        public const byte Boolean = 1;
        public const byte NumberTwoBytes = 2;
        public const byte NumberFourBytes = 3;
        public const byte NumberEigthBytes = 4;
        public const byte String = 5;
        public const byte Parseable = 6;
        public const byte Enumerable = 7;
        public const byte EndEnumerable = 12;
        public const byte Array = 8;
        public const byte Object = 9;
        public const byte EndObject = 13;
        public const byte PartionAll = 10;
        public const byte PartionField = 10;
        public const byte PartionProperties = 10;
        public const byte EndPartition = 11;
        public const byte UTF8 = 14;
        public const byte UTF16 = 15;
        public const byte UTF32 = 16;
        public const byte ASSIC = 17;
        public const byte SimpleArrayBytes = 18;
        public const byte NumberUTwoBytes = 19;
        public const byte NumberUFourBytes = 20;
        public const byte NumberUEigthBytes = 21;
        public const byte NumberDouble = 22;
        public const byte NumberFloat = 22;
    }
}
