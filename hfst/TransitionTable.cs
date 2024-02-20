using System;
using System.Collections.Generic;
using System.IO;

namespace hfst
{

    public class TransitionTable
    {
        private readonly int[] ti_inputSymbols;
        private readonly int[] ti_outputSymbols;
        private readonly long[] ti_targets;

        public TransitionTable(BinaryReader b, int transitionCount)
        {
            ByteArray bytes = new ByteArray(transitionCount * 8);
            // each transition entry is two unsigned shorts and an unsigned int
            bytes.bytes = MikaReader.Read(b, bytes.size);
            ti_inputSymbols = new int[transitionCount];
            ti_outputSymbols = new int[transitionCount];
            ti_targets = new long[transitionCount];
            int i = 0;
            while (i < transitionCount)
            {
                ti_inputSymbols[i] = bytes.GetUShort();
                ti_outputSymbols[i] = bytes.GetUShort();
                ti_targets[i] = bytes.GetUInt();
                i++;
            }
        }

        public bool Matches(int pos, int symbol)
        {
            if (ti_inputSymbols[pos] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) return false;
            if (symbol == HfstOptimizedLookup.NO_SYMBOL_NUMBER) return true;
            return (ti_inputSymbols[pos] == symbol);
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

        public bool IsFinal(int pos)
        {
            return (ti_inputSymbols[pos] == HfstOptimizedLookup.NO_SYMBOL_NUMBER && ti_outputSymbols[pos] == HfstOptimizedLookup.NO_SYMBOL_NUMBER && ti_targets[pos] == 1);
        }

        public int Size()
        {
            return ti_targets.Length;
        }
    }
}
