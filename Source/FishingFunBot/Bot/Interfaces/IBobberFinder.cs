using System.Drawing;

namespace FishingFunBot.Bot.Interfaces
{
    public interface IBobberFinder
    {
        Point Find();

        void Reset();
    }
}