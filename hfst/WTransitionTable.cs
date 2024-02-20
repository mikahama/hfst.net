using System;
using System.IO;

namespace hfst
{
    public class WTransitionTable
    {
        private readonly int[] ti_inputSymbols;
        private readonly int[] ti_outputSymbols;
        private readonly long[] ti_targets;
        private readonly float[] ti_weights;

        public WTransitionTable(BinaryReader b, int transitionCount)
        {
            ByteArray bytes = new ByteArray(transitionCount * 12);
            // Reading the full required bytes into the array
            bytes.bytes = MikaReader.Read(b, bytes.size);
            

            ti_inputSymbols = new int[transitionCount];
            ti_outputSymbols = new int[transitionCount];
            ti_targets = new long[transitionCount];
            ti_weights = new float[transitionCount];

            
                for (int i = 0; i < transitionCount; i++)
                {
                    ti_inputSymbols[i] = bytes.GetUShort(); // getUShort equivalent
                    ti_outputSymbols[i] = bytes.GetUShort(); // getUShort equivalent
                    ti_targets[i] = bytes.GetUInt(); // getUInt equivalent
                    ti_weights[i] = bytes.GetFloat();; // getFloat equivalent
                }
            
        }

        public int GetInput(int pos)
        {
            return ti_inputSymbols[pos];
        }

        public int GetOutput(int pos)
        {
            return ti_outputSymbols[pos];
        }

        public long GetTarget(int pos)
        {
            return ti_targets[pos];
        }

        public float GetWeight(int pos)
        {
            return ti_weights[pos];
        }

        public bool IsFinal(int pos)
        {
            return (ti_inputSymbols[pos] == HfstOptimizedLookup.NO_SYMBOL_NUMBER && ti_outputSymbols[pos] == HfstOptimizedLookup.NO_SYMBOL_NUMBER && ti_targets[pos] == 1);
        }

        public int Size
        {
            get { return ti_targets.Length; }
        }
    }
}
