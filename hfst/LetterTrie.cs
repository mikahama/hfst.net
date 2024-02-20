using System;
using System.Collections.Generic;

namespace hfst
{
    public class LetterTrie
    {
        public class LetterTrieNode
        {
            private readonly Dictionary<char, int> symbols = new Dictionary<char, int>();
            private readonly Dictionary<char, LetterTrieNode> children = new Dictionary<char, LetterTrieNode>();

            public void AddString(string str, int symbolNumber)
            {
                if (str.Length > 1)
                {
                    if (!children.ContainsKey(str[0]))
                        children[str[0]] = new LetterTrieNode();
                    children[str[0]].AddString(str.Substring(1), symbolNumber);
                }
                else if (str.Length == 1)
                {
                    symbols[str[0]] = symbolNumber;
                }
            }

            public int FindKey(IndexString str)
            {
                if (str.Index >= str.Str.Length) return HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                char at_s = str.Str[str.Index];
                str.Index++;
                LetterTrieNode child;
                if (!children.TryGetValue(at_s, out child))
                {
                    int symbol;
                    if (!symbols.TryGetValue(at_s, out symbol) || symbol == 0)
                    {
                        str.Index--;
                        return HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    }
                    return symbol;
                }
                int s = child.FindKey(str);
                if (s == HfstOptimizedLookup.NO_SYMBOL_NUMBER)
                {
                    int symbol;
                    if (!symbols.TryGetValue(at_s, out symbol) || symbol == 0)
                    {
                        str.Index--;
                        return HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    }
                    return symbol;
                }
                return s;
            }

            public LetterTrieNode()
            {
            }
        }

        private readonly LetterTrieNode root;

        public LetterTrie()
        {
            root = new LetterTrieNode();
        }

        public void AddString(string str, int symbolNumber)
        {
            root.AddString(str, symbolNumber);
        }

        public int FindKey(IndexString str)
        {
            return root.FindKey(str);
        }
    }

}