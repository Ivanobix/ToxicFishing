namespace ToxicFishing
{
    public partial class PixelClassifier
    {
        public enum ClassifierMode { Red, Blue }
        public ClassifierMode Mode { get; set; } = ClassifierMode.Red;

        public double ColourMultiplier { get; set; } = 0.5;
        public double ColourClosenessMultiplier { get; set; } = 2.0;

        public bool IsMatch(byte red, byte green, byte blue)
        {
            return Mode == ClassifierMode.Red
                ? isBigger(red, green, blue) && areClose(blue, green)
                : isBigger(blue, green, red) && areClose(red, green);
        }

        public void SetConfiguration(bool isWowClasic)
        {
            if (isWowClasic)
            {
                Console.WriteLine("Wow Classic configuration");
                ColourMultiplier = 1;
                ColourClosenessMultiplier = 1;
            }
            else
            {
                Console.WriteLine("Wow Standard configuration");
            }
        }

        private bool isBigger(byte primary, byte comparison1, byte comparison2)
        {
            return (primary * ColourMultiplier) > comparison1 && (primary * ColourMultiplier) > comparison2;
        }

        private bool areClose(byte color1, byte color2)
        {
            return Math.Min(color1, color2) * ColourClosenessMultiplier > Math.Max(color1, color2) - 20;
        }
    }
}
