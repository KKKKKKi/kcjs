namespace K_C_S_J.Models
{
    public class SmoothOpts
    {
        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Opt { get; set; }

        public SmoothOpts() {}

        public SmoothOpts(int start, int end, int opt)
        {
            StartIndex = start;
            EndIndex = end;
            Opt = opt;
        }
    }
}
