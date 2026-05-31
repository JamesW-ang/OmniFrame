namespace OmniFrame
{
    public class VisionParams
    {
        public int CannyLow { get; set; } = 50;
        public int CannyHigh { get; set; } = 150;
        public int Clahe { get; set; } = 2;
        public int MinArea { get; set; } = 100;
        public double Confidence { get; set; } = 0.8;
        public double NmsIou { get; set; } = 0.45;
    }
}
