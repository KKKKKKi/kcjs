namespace K_C_S_J.Models
{
    public class Peak
    {
        public int EdgeLeft { get; set; }

        public int EdgeRight { get; set; }

        public int Position { get; set; }

        public double Area { get; set; }

        public Peak() {}

        public Peak(int left, int right, int pos)
        {
            EdgeLeft = left;
            EdgeRight = right;
            Position = pos;
        }

        public override string ToString()
        {
            return $"{Position} - ({EdgeLeft}, {EdgeRight})";
        }
    }
}
