using System;


namespace hfst {
    public class HFST
    {
        ITransducer transducer;
        public HFST(string path)
        {
            var fs = File.OpenRead(path);
            transducer = LoadTransducer(new BinaryReader(fs));
        }
        public HFST(FileStream fs)
        {
            transducer = LoadTransducer(new BinaryReader(fs));
        }
        public HFST(BinaryReader br)
        {
            transducer = LoadTransducer(br);
        }
        private ITransducer LoadTransducer(BinaryReader reader)
        {
            TransducerHeader h = new TransducerHeader(reader);
            TransducerAlphabet a = new TransducerAlphabet(reader, h.GetSymbolCount());
            ITransducer transducer = h.IsWeighted() ? 
                new WeightedTransducer(reader, h, a) : 
                new UnweightedTransducer(reader, h, a);
            return transducer;
        }
        public List<Result> Lookup(string input)
        {
            var analyses = transducer.Analyze(input);
            return analyses;
        }
        public List<string> LookupStrings(string input)
        {
            var analyses = transducer.Analyze(input);
            var strings = new List<string>();
            foreach (var analysis in analyses)
            {
                strings.Add(analysis.ToString());
            }
            return strings;
        }
    }
}