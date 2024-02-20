namespace hfst{
    public class IndexString
    {
        public string Str { get; set; }
        public int Index { get; set; }

        public IndexString(string s)
        {
            Str = s;
            Index = 0;
        }
    }
}