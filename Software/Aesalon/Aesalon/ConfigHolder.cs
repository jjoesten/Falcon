using System;
using System.IO;

namespace Aesalon
{
    public class ConfigHolder : BindableObject
    {
        private static ConfigHolder singleton;

        // Store config files under user's AppData/Local path to avoid needing Admin privileges
        private static readonly string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aesalon");
        private static readonly string configFileName = Path.Combine(appDataPath, "Aesalon.xml");

        private Configuration configuration = new Configuration();

        public static ConfigHolder Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new ConfigHolder();
                return singleton;
            }
        }

        public Configuration Configuration
        {
            get { return configuration; }
            set
            {
                if (configuration == value)
                    return;
                if (configuration != null)
                    configuration.Dispose();
                configuration = value;
                RaisePropertyChanged(() => Configuration);
            }
        }

        public void Save()
        {
            // TODO: Implement ConfigHolder.Save
        }

        public void Load()
        {
            // TODO: Implement ConfigHolder.Load
        }
    }
}
