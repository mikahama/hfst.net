using System; // Required for basic .NET functionalities

namespace hfst
{
    /// <summary>
    /// A representation of one flag diacritic statement.
    /// </summary>
    public class FlagDiacriticOperation
    {
        public HfstOptimizedLookup.FlagDiacriticOperator Op { get; set; }
        public int Feature { get; set; }
        public int Value { get; set; }

        public FlagDiacriticOperation(HfstOptimizedLookup.FlagDiacriticOperator operation, int feature, int value)
        {
            Op = operation;
            Feature = feature;
            Value = value;
        }

        public FlagDiacriticOperation()
        {
            Op = HfstOptimizedLookup.FlagDiacriticOperator.P; // Assuming this is an enum in the HfstOptimizedLookup class
            Feature = HfstOptimizedLookup.NO_SYMBOL_NUMBER; // Assuming this is a constant defined in HfstOptimizedLookup
            Value = 0;
        }

        public bool IsFlag()
        {
            return Feature != HfstOptimizedLookup.NO_SYMBOL_NUMBER;
        }
    }
}