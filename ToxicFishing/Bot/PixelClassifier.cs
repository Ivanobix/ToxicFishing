namespace ToxicFishing
{
    public partial class PixelClassifier
    {
        public enum ClassifierMode { Red, Blue }
        public ClassifierMode Mode { get; set; }

        public double ColourMultiplier => Mode == ClassifierMode.Red ? 0.5 : 1.5;
        public double ColourClosenessMultiplier { get; set; } = 2.0;

        public bool IsMatch(byte red, byte green, byte blue)
        {
            return Mode == ClassifierMode.Red
                ? IsBigger(red, green, blue) && AreClose(blue, green)
                : IsBigger(blue, green, red) && AreClose(red, green);
        }

        private bool IsBigger(byte primary, byte comparison1, byte comparison2)
        {
            return (primary * ColourMultiplier) > comparison1 && (primary * ColourMultiplier) > comparison2;
        }

        private bool AreClose(byte color1, byte color2)
        {
            return Math.Min(color1, color2) * ColourClosenessMultiplier > Math.Max(color1, color2) - 20;
        }
    }
}
