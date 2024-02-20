using System;
using System.Collections.Generic;
using System.IO;
// Assuming Hppc is a library similar to the com.carrotsearch.hppc used in Java, you may need to find a .NET equivalent or implement the required functionality.

namespace hfst
{
    public class WeightedTransducer : ITransducer
    {
        protected TransducerHeader header;
        protected TransducerAlphabet alphabet;
        protected Dictionary<int, FlagDiacriticOperation> operations;
        protected LetterTrie letterTrie;
        protected IndexTable indexTable;
        protected WTransitionTable transitionTable;

        public WeightedTransducer(BinaryReader b, TransducerHeader h, TransducerAlphabet a)
        {
            header = h;
            alphabet = a;
            operations = alphabet.Operations; // Assuming operations is exposed as a property
            letterTrie = new LetterTrie();
            int i = 0;
            while (i < header.GetInputSymbolCount())
            {
                letterTrie.AddString(alphabet.KeyTable[i], i);
                i++;
            }
            
                indexTable = new IndexTable(b, header.GetIndexTableSize());
                transitionTable = new WTransitionTable(b, header.GetTargetTableSize());//Might not work
            
            
            
        }

        private int Pivot(long i)
        {
            if (i >= HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START)
                return (int)(i - HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START);
            return (int)i;
        }

        private void TryEpsilonIndices(int index, WState state)
        {
            if (indexTable.GetInput(index) == 0) TryEpsilonTransitions(Pivot(indexTable.GetTarget(index)), state);
        }

        private void TryEpsilonTransitions(int index, WState state)
        {
            while (true)
            {
                // First test for flag
                if (operations.ContainsKey(transitionTable.GetInput(index)))
                {
                    if (!PushState(operations[transitionTable.GetInput(index)], state))
                    {
                        ++index;
                        continue;
                    }
                    else
                    {
                        if (state.outputPointer == state.outputString.Count) state.outputString.Add(transitionTable.GetOutput(index));
                        else state.outputString[state.outputPointer] = transitionTable.GetOutput(index);
                        ++state.outputPointer;
                        state.currentWeight += transitionTable.GetWeight(index);
                        GetAnalyses(transitionTable.GetTarget(index), state);
                        state.currentWeight -= transitionTable.GetWeight(index);
                        --state.outputPointer;
                        ++index;
                        state.stateStack.Pop();
                        continue;
                    }
                }
                else if (transitionTable.GetInput(index) == 0) // Epsilon transitions
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(transitionTable.GetOutput(index));
                    else state.outputString[state.outputPointer] = transitionTable.GetOutput(index);
                    ++state.outputPointer;
                    state.currentWeight += transitionTable.GetWeight(index);
                    GetAnalyses(transitionTable.GetTarget(index), state);
                    state.currentWeight -= transitionTable.GetWeight(index);
                    --state.outputPointer;
                    ++index;
                    continue;
                }
                else break;
            }
        }

        private void FindIndex(int index, WState state)
        {
            if (indexTable.GetInput(index + state.inputString[state.inputPointer - 1]) == state.inputString[state.inputPointer - 1])
                FindTransitions(Pivot(indexTable.GetTarget(index + state.inputString[state.inputPointer - 1])), state);
        }

        private void FindTransitions(int index, WState state)
        {
            while (transitionTable.GetInput(index) != HfstOptimizedLookup.NO_SYMBOL_NUMBER)
            {
                if (transitionTable.GetInput(index) == state.inputString[state.inputPointer - 1])
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(transitionTable.GetOutput(index));
                    else state.outputString[state.outputPointer] = transitionTable.GetOutput(index);
                    ++state.outputPointer;
                    state.currentWeight += transitionTable.GetWeight(index);
                    GetAnalyses(transitionTable.GetTarget(index), state);
                    state.currentWeight -= transitionTable.GetWeight(index);
                    --state.outputPointer;
                }
                else
                    return;
                ++index;
            }
        }

        private void GetAnalyses(long idx, WState state)
        {
            if (idx >= HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START)
            {
                int index = Pivot(idx);
                TryEpsilonTransitions(index + 1, state);
                if (state.inputString[state.inputPointer] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) // End of input string
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
                    else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    if (transitionTable.Size > index && transitionTable.IsFinal(index))
                    {
                        state.currentWeight += transitionTable.GetWeight(index);
                        NoteAnalysis(state);
                        state.currentWeight -= transitionTable.GetWeight(index);
                    }
                    return;
                }
                ++state.inputPointer;
                FindTransitions(index + 1, state);
            }
            else
            {
                int index = Pivot(idx);
                TryEpsilonIndices(index + 1, state);
                if (state.inputString[state.inputPointer] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) // End of input string
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
                    else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    if (indexTable.IsFinal(index))
                    {
                        state.currentWeight += indexTable.GetFinalWeight(index);
                        NoteAnalysis(state);
                        state.currentWeight -= indexTable.GetFinalWeight(index);
                    }
                    return;
                }
                ++state.inputPointer;
                FindIndex(index + 1, state);
            }
            --state.inputPointer;
            if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
            else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
        }

        private List<string> GetSymbols(WState state)
        {
            int i = 0;
            List<string> symbols = new List<string>();
            while (i < state.outputString.Count && state.outputString[i] != HfstOptimizedLookup.NO_SYMBOL_NUMBER)
                symbols.Add(alphabet.KeyTable[state.outputString[i++]]);
            return symbols;
        }

        private void NoteAnalysis(WState state)
        {
            state.displayVector.Add(new Result(GetSymbols(state), state.currentWeight));
        }

        public List<Result> Analyze(string input)
        {
            Console.WriteLine("Ready for input." + input);
            WState state = new WState(input, alphabet, letterTrie);
            if (state.inputString[0] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) return new List<Result>();
            GetAnalyses(0, state);
            return state.displayVector;
        }

        public List<string> GetAlphabet()
        {
            return alphabet.KeyTable;
        }

        private bool PushState(FlagDiacriticOperation flag, WState state)
        {
            int[] top = new int[alphabet.Features];
            Array.Copy(state.stateStack.Peek(), top, alphabet.Features);
            switch (flag.Op)
            {
                case HfstOptimizedLookup.FlagDiacriticOperator.P: // Positive set
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = flag.Value;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.N: // Negative set
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = -1 * flag.Value;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.R: // Require
                    if (flag.Value == 0) // Empty require
                    {
                        if (state.stateStack.Peek()[flag.Feature] == 0)
                            return false;
                        else
                        {
                            state.stateStack.Push(top);
                            return true;
                        }
                    }
                    else if (state.stateStack.Peek()[flag.Feature] == flag.Value)
                    {
                        state.stateStack.Push(top);
                        return true;
                    }
                    return false;
                case HfstOptimizedLookup.FlagDiacriticOperator.D: // Disallow
                    if (flag.Value == 0) // Empty disallow
                    {
                        if (state.stateStack.Peek()[flag.Feature] != 0)
                            return false;
                        else
                        {
                            state.stateStack.Push(top);
                            return true;
                        }
                    }
                    else if (state.stateStack.Peek()[flag.Feature] == flag.Value) return false;
                    state.stateStack.Push(top);
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.C: // Clear
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = 0;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.U: // Unification
                    if ((state.stateStack.Peek()[flag.Feature] == 0) || (state.stateStack.Peek()[flag.Feature] == flag.Value) || (state.stateStack.Peek()[flag.Feature] != flag.Value && state.stateStack.Peek()[flag.Feature] < 0))
                    {
                        state.stateStack.Push(top);
                        state.stateStack.Peek()[flag.Feature] = flag.Value;
                        return true;
                    }
                    return false;
                default:
                    return false; // Compiler sanity
            }
        }
    }
}