using System;
using System.Collections.Generic;
using System.IO;


namespace hfst
{
    /**
     * On instantiation reads the transducer's alphabet and provides an interface to
     * it. Flag diacritic parsing is also handled here.
     */
    public class TransducerAlphabet
    {
        public List<string> KeyTable { get; }
        public Dictionary<int, FlagDiacriticOperation> Operations { get; }
        public int Features { get; private set; }

        public TransducerAlphabet(BinaryReader reader, int numberOfSymbols)
        {
            KeyTable = new List<string>();
            Operations = new Dictionary<int, FlagDiacriticOperation>();
            var featureBucket = new Dictionary<string, int>();
            var valueBucket = new Dictionary<string, int> { { "", 0 } }; // neutral value
            Features = 0;
            int values = 1;
            int i = 0;
            int charIndex;
            List<sbyte> chars = new List<sbyte>();
            
                while (i < numberOfSymbols)
                {
                    charIndex = 0;
                    if (chars.Count == charIndex)
                        chars.Add(reader.ReadSByte());
                    else
                        chars[charIndex] = reader.ReadSByte();
                    while (chars[charIndex] != 0)
                    {
                        ++charIndex;
                        if (chars.Count == charIndex)
                            chars.Add(reader.ReadSByte());
                        else
                            chars[charIndex] = reader.ReadSByte();
                    }
                    var sbytes = chars.ToArray();
                    byte[] bytes = new byte[sbytes.Length];
                    Buffer.BlockCopy(sbytes, 0, bytes, 0, sbytes.Length);
                    string uString = System.Text.Encoding.UTF8.GetString(bytes, 0, charIndex);
                    if (uString.Length > 5 && uString[0] == '@' && uString[uString.Length - 1] == '@' && uString[2] == '.')
                    {
                        // flag diacritic identified
                        HfstOptimizedLookup.FlagDiacriticOperator op;
                        string[] parts = uString.Substring(1, uString.Length - 1).Split('.');
                        // Not a flag diacritic after all, ignore it
                        if (parts.Length < 2)
                        {
                            KeyTable.Add("");
                            i++;
                            continue;
                        }
                        string ops = parts[0];
                        string feats = parts[1];
                        string vals = parts.Length == 3 ? parts[2] : "";
                        op = Enum.TryParse<HfstOptimizedLookup.FlagDiacriticOperator>(ops, out var parsedOp) ? parsedOp : throw new InvalidOperationException("Invalid operator");
                        if (!valueBucket.ContainsKey(vals))
                        {
                            valueBucket[vals] = values++;
                        }
                        if (!featureBucket.ContainsKey(feats))
                        {
                            featureBucket[feats] = Features++;
                        }
                        Operations[i] = new FlagDiacriticOperation(op, featureBucket[feats], valueBucket[vals]);
                        KeyTable.Add("");
                        i++;
                        continue;
                    }
                    KeyTable.Add(uString);
                    i++;
                }
            
   
            KeyTable[0] = ""; // epsilon is zero
        }
    }
}