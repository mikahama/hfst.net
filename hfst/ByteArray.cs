using System;

namespace hfst
{
    public class ByteArray
    {
        public  sbyte[] bytes;
        private int index;
        public  int size;

        public ByteArray(int s)
        {
            size = s;
            bytes = new sbyte[size];
            index = 0;
        }

        public ByteArray(ByteArray another, int s)
        {
            size = Math.Max(s, another.Size);
            bytes = new sbyte[size];
            for (int i = 0; i < another.Size; ++i)
                bytes[i] = another.Get(i);
            index = 0;
        }

        public int Size => size;

        public sbyte Get(int i) => bytes[i];

        public sbyte[] GetBytes() => bytes;

        public short GetUByte()
        {
            short result = 0;
            result |= (short)(bytes[index] & 0xFF);
            index += 1;
            return result;
        }

        public int GetUShort()
        {
            int result = 0;
            result |= bytes[index + 1] & 0xFF;
            result <<= 8;
            result |= bytes[index] & 0xFF;
            index += 2;
            return result;
        }

        public long GetUInt()
        {
            long result = 0;
            result |= (long)(bytes[index + 3] & 0xFF);
            result <<= 8;
            result |= bytes[index + 2] & 0xFF;
            result <<= 8;
            result |= bytes[index + 1] & 0xFF;
            result <<= 8;
            result |= bytes[index] & 0xFF;
            index += 4;
            return result;
        }

        public bool GetBool()
        {
            return GetUInt() != 0;
        }

        public float GetFloat()
        {
            int bits = 0;
            bits |= bytes[index + 3] & 0xFF;
            bits <<= 8;
            bits |= bytes[index + 2] & 0xFF;
            bits <<= 8;
            bits |= bytes[index + 1] & 0xFF;
            bits <<= 8;
            bits |= bytes[index] & 0xFF;
            index += 4;
            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }
    }
}