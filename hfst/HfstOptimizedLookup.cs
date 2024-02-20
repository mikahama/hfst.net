using System;
using System.Collections.Generic;
using System.IO;

namespace hfst
{
    public class HfstOptimizedLookup
    {
        public const long TRANSITION_TARGET_TABLE_START = 2147483648L; // 2^31
        public const long NO_TABLE_INDEX = 4294967295L;
        public const float INFINITE_WEIGHT = 4294967295L; // Cast of UINT_MAX to float in C++
        public const int NO_SYMBOL_NUMBER = 65535; // USHRT_MAX

        public enum FlagDiacriticOperator
        {
            P, N, R, D, C, U
        };

        public static void RunTransducer(HFST t)
        {
            Console.WriteLine("Ready for input.");
            string input;
            while ((input = Console.ReadLine()) != null)
            {
                var analyses = t.Lookup(input);
                foreach (var analysis in analyses)
                {
                    Console.WriteLine($"{input}\t{analysis}\t{analysis.Weight}");
                }
                if (analyses.Count == 0)
                {
                    Console.WriteLine($"{input}\t+?");
                }
                Console.WriteLine();
            }
        }

        public static void Main(string[] args)
        {

            FileStream transducerFile = null;
            try
            {
                transducerFile = File.OpenRead(args[0]);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"File not found: couldn't read transducer file {args[0]}.");
                Environment.Exit(1);
            }
            var hfst = new HFST(transducerFile);
            RunTransducer(hfst);
        }
    }
}