using System;
using System.IO;

namespace hfst
{

    public class IndexTable
    {
        private readonly int[] ti_inputSymbols;
        private readonly long[] ti_targets;

        public int GetInput(int i)
        {
            return ti_inputSymbols[i];
        }

        public long GetTarget(int i)
        {
            return ti_targets[i];
        }

        public bool IsFinal(int i)
        {
            return (ti_inputSymbols[i] == HfstOptimizedLookup.NO_SYMBOL_NUMBER && ti_targets[i] != HfstOptimizedLookup.NO_TABLE_INDEX);
        }

        public float GetFinalWeight(int i) {
            // Assuming ti_targets is an array accessible within this context
            // and it's an array of int or a similar type that needs conversion.
            return BitConverter.ToSingle(BitConverter.GetBytes(ti_targets[i]), 0);
        }

        public IndexTable(BinaryReader input, int indicesCount)
        {
            // Assuming each index entry consists of a 2-byte unsigned short followed by a 4-byte unsigned int
            ByteArray b = new ByteArray(indicesCount * 6);
            b.bytes = MikaReader.Read(input, b.size);

            ti_inputSymbols = new int[indicesCount];
            ti_targets = new long[indicesCount];

            for (int i = 0; i < indicesCount; i++)
            {
                ti_inputSymbols[i] = b.GetUShort(); // Read an unsigned short (2 bytes)
                ti_targets[i] = b.GetUInt(); // Read an unsigned int (4 bytes)
            }
        }
    }
}