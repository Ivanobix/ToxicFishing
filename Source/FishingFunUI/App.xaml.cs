using System.IO;
using System.Windows;

namespace FishingFun
{
    public partial class App : Application
    {
        public App()
        {
            _ = log4net.Config.XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));
        }
    }
}