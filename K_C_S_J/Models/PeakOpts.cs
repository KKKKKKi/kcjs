namespace K_C_S_J.Models
{
    public class PeakOpts
    {
        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Numm { get; set; }

        public int NumH { get; set; }

        public double NumR { get; set; }

        public PeakOpts() { }

        public PeakOpts(int start, int end, int m, int h, double r)
        {
            StartIndex = start;
            EndIndex = end;
            Numm = m;
            NumH = h;
            NumR = r;
        }
    }
}
