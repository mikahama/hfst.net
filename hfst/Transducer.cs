using System.Collections.Generic;

namespace hfst
{
    public interface ITransducer
    {
        List<Result> Analyze(string str);
        List<string> GetAlphabet();
    }

    public class Result
    {
        private readonly List<string> symbols;
        private readonly float weight;

        public Result(List<string> symbols, float weight)
        {
            this.symbols = symbols;
            this.weight = weight;
        }

        public List<string> Symbols
        {
            get { return symbols; }
        }

        public float Weight
        {
            get { return weight; }
        }

        public string Analysis(){
            return this.ToString();
        }

        public override string ToString()
        {
            return string.Join("", symbols);
        }
    }
}