using System;
using System.Collections.Generic;
using System.IO;

namespace hfst
{
    public class UnweightedTransducer : ITransducer
    {
        private TransducerHeader header;
        private TransducerAlphabet alphabet;
        private Dictionary<int, FlagDiacriticOperation> operations;
        private LetterTrie letterTrie;
        private IndexTable indexTable;
        private TransitionTable transitionTable;



        public UnweightedTransducer(BinaryReader readers, TransducerHeader h, TransducerAlphabet a)
        {
            header = h;
            alphabet = a;
            operations = alphabet.Operations;
            letterTrie = new LetterTrie();
            int i = 0;
            while (i < header.GetInputSymbolCount())
            {
                letterTrie.AddString(alphabet.KeyTable[i], i);
                i++;
            }
        
            
                indexTable = new IndexTable(readers, header.GetIndexTableSize());
                 transitionTable = new TransitionTable(readers, header.GetTargetTableSize());//Might not work..
            
        }

        private int Pivot(long i)
        {
            if (i >= HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START)
                return (int)(i - HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START);
            return (int)i;
        }

        private void TryEpsilonIndices(int index, State state)
        {
            if (indexTable.GetInput(index) == 0) TryEpsilonTransitions(Pivot(indexTable.GetTarget(index)), state);
        }

        private void TryEpsilonTransitions(int index, State state)
        {
            while (true)
            {
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
                        GetAnalyses(transitionTable.GetTarget(index), state);
                        --state.outputPointer;
                        ++index;
                        state.stateStack.Pop();
                        continue;
                    }
                }
                else if (transitionTable.GetInput(index) == 0) // epsilon transitions
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(transitionTable.GetOutput(index));
                    else state.outputString[state.outputPointer] = transitionTable.GetOutput(index);
                    ++state.outputPointer;
                    GetAnalyses(transitionTable.GetTarget(index), state);
                    --state.outputPointer;
                    ++index;
                    continue;
                }
                else break;
            }
        }

        private void FindIndex(int index, State state)
        {
            if (indexTable.GetInput(index + state.inputString[state.inputPointer - 1]) == state.inputString[state.inputPointer - 1])
                FindTransitions(Pivot(indexTable.GetTarget(index + state.inputString[state.inputPointer - 1])), state);
        }

        private void FindTransitions(int index, State state)
        {
            while (transitionTable.GetInput(index) != HfstOptimizedLookup.NO_SYMBOL_NUMBER)
            {
                if (transitionTable.GetInput(index) == state.inputString[state.inputPointer - 1])
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(transitionTable.GetOutput(index));
                    else state.outputString[state.outputPointer] = transitionTable.GetOutput(index);
                    ++state.outputPointer;
                    GetAnalyses(transitionTable.GetTarget(index), state);
                    --state.outputPointer;
                }
                else return;
                ++index;
            }
        }

        private void GetAnalyses(long idx, State state)
        {
            if (idx >= HfstOptimizedLookup.TRANSITION_TARGET_TABLE_START)
            {
                int index = Pivot(idx);
                TryEpsilonTransitions(index + 1, state);
                if (state.inputString[state.inputPointer] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) // end of input string
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
                    else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    if (transitionTable.Size() > index && transitionTable.IsFinal(index)) NoteAnalysis(state);
                    return;
                }
                ++state.inputPointer;
                FindTransitions(index + 1, state);
            }
            else
            {
                int index = Pivot(idx);
                TryEpsilonIndices(index + 1, state);
                if (state.inputString[state.inputPointer] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) // end of input string
                {
                    if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
                    else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
                    if (indexTable.IsFinal(index)) NoteAnalysis(state);
                    return;
                }
                ++state.inputPointer;
                FindIndex(index + 1, state);
            }
            --state.inputPointer;
            if (state.outputPointer == state.outputString.Count) state.outputString.Add(HfstOptimizedLookup.NO_SYMBOL_NUMBER);
            else state.outputString[state.outputPointer] = HfstOptimizedLookup.NO_SYMBOL_NUMBER;
        }

        private List<string> GetSymbols(State state)
        {
            int i = 0;
            List<string> symbols = new List<string>();
            while (i < state.outputString.Count && state.outputString[i] != HfstOptimizedLookup.NO_SYMBOL_NUMBER)
                symbols.Add(alphabet.KeyTable[state.outputString[i++]]);
            return symbols;
        }

        private void NoteAnalysis(State state)
        {
            state.displayVector.Add(new Result(GetSymbols(state), 1.0f));
        }

        public List<Result> Analyze(string input)
        {
            State state = new State(input, alphabet, letterTrie);
            if (state.inputString[0] == HfstOptimizedLookup.NO_SYMBOL_NUMBER) return new List<Result>();
            GetAnalyses(0, state);
            return state.displayVector;
        }

        public List<string> GetAlphabet()
        {
            return alphabet.KeyTable;
        }

        private bool PushState(FlagDiacriticOperation flag, State state)
        {
            int[] top = new int[alphabet.Features];
            state.stateStack.Peek().CopyTo(top, 0);
            switch (flag.Op)
            {
                case HfstOptimizedLookup.FlagDiacriticOperator.P: // positive set
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = flag.Value;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.N: // negative set
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = -1 * flag.Value;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.R: // require
                    if (flag.Value == 0) // empty require
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
                case HfstOptimizedLookup.FlagDiacriticOperator.D: // disallow
                    if (flag.Value == 0) // empty disallow
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
                case HfstOptimizedLookup.FlagDiacriticOperator.C: // clear
                    state.stateStack.Push(top);
                    state.stateStack.Peek()[flag.Feature] = 0;
                    return true;
                case HfstOptimizedLookup.FlagDiacriticOperator.U: // unification
                    if ((state.stateStack.Peek()[flag.Feature] == 0) || (state.stateStack.Peek()[flag.Feature] == flag.Value) || (state.stateStack.Peek()[flag.Feature] != flag.Value && state.stateStack.Peek()[flag.Feature] < 0))
                    {
                        state.stateStack.Push(top);
                        state.stateStack.Peek()[flag.Feature] = flag.Value;
                        return true;
                    }
                    return false;
                default:
                    return false; // compiler sanity
            }
        }
    }
}
