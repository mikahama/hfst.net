using System.IO;

namespace hfst
{
    /**
     * On instantiation reads the transducer's header and provides an interface to
     * it.
     */
    public class TransducerHeader
    {
        private readonly int numberOfInputSymbols;
        private readonly int numberOfSymbols;
        private readonly int sizeOfTransitionIndexTable;
        private readonly int sizeOfTransitionTargetTable;
        private readonly int numberOfStates;
        private readonly int numberOfTransitions;

        private readonly bool weighted;
        private readonly bool deterministic;
        private readonly bool inputDeterministic;
        private readonly bool minimized;
        private readonly bool cyclic;
        private readonly bool hasEpsilonEpsilonTransitions;
        private readonly bool hasInputEpsilonTransitions;
        private readonly bool hasInputEpsilonCycles;
        private readonly bool hasUnweightedInputEpsilonCycles;

        private bool hfst3;
        private readonly bool intact;

        /**
         * Read in the (56 bytes of) header information, which unfortunately is
         * mostly in little-endian unsigned form.
         */
        public TransducerHeader(BinaryReader input)
        {
            hfst3 = false;
            intact = true; // could add some checks to toggle this and check outside
            var head = new ByteArray(5);
            
            head.bytes = MikaReader.Read(input, head.size);
            if (BeginsHfst3Header(head))
            {
                SkipHfst3Header(input);
                head.bytes = MikaReader.Read(input, head.size);
                hfst3 = true;
            }
            ByteArray b = new ByteArray(head, 56);
            var bytes = MikaReader.Read(input, 51);
            Buffer.BlockCopy(bytes, 0, b.bytes, 5, 51);

            numberOfInputSymbols = b.GetUShort();
            numberOfSymbols = b.GetUShort();
            sizeOfTransitionIndexTable = (int)b.GetUInt();
            sizeOfTransitionTargetTable = (int)b.GetUInt();
            numberOfStates = (int)b.GetUInt();
            numberOfTransitions = (int)b.GetUInt();

            weighted = b.GetBool();
            deterministic = b.GetBool();
            inputDeterministic = b.GetBool();
            minimized = b.GetBool();
            cyclic = b.GetBool();
            hasEpsilonEpsilonTransitions = b.GetBool();
            hasInputEpsilonTransitions = b.GetBool();
            hasInputEpsilonCycles = b.GetBool();
            hasUnweightedInputEpsilonCycles = b.GetBool();
        }
        private bool BeginsHfst3Header(ByteArray bytes)
        {
            if (bytes.size < 5) return false;
            // HFST\0
            return (bytes.GetUByte() == 72 && bytes.GetUByte() == 70 && bytes.GetUByte() == 83 && bytes.GetUByte() == 84 && bytes.GetUByte() == 0);
        }

        private void SkipHfst3Header(BinaryReader file)
        {
            ByteArray len = new ByteArray(2);
            len.bytes = MikaReader.Read(file, 2);
            
            file.BaseStream.Seek(len.GetUShort() + 1, SeekOrigin.Current);
        }

        public int GetInputSymbolCount()
        {
            return numberOfInputSymbols;
        }

        public int GetSymbolCount()
        {
            return numberOfSymbols;
        }

        public int GetIndexTableSize()
        {
            return sizeOfTransitionIndexTable;
        }

        public int GetTargetTableSize()
        {
            return sizeOfTransitionTargetTable;
        }

        public bool IsWeighted()
        {
            return weighted;
        }

        public bool HasHfst3Header()
        {
            return hfst3;
        }

        public bool IsIntact()
        {
            return intact;
        }
    }

    public class MikaReader{
        public static sbyte[] Read(BinaryReader b, int count){
            var bytes = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = b.ReadSByte();
            }
            return bytes;
        }
    }
}