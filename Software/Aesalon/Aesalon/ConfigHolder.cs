using System;
using System.IO;
using System.Xml.Serialization;

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
            Configuration.FormatVersion = Configuration.CurrentFormatVersion.ToString();

            // Create Aesalon directory under AppData/Local path if it doesn't exist
            Directory.CreateDirectory(appDataPath);

            XmlSerializer xs = new XmlSerializer(typeof(Configuration));

            using (Stream file = File.Create(configFileName))
            {
                xs.Serialize(file, Configuration);
            }
        }

        public void Load()
        {
            if (!File.Exists(configFileName))
                return;

            XmlSerializer xs = new XmlSerializer(typeof(Configuration));

            using (Stream file = File.OpenRead(configFileName))
            {
                Configuration newConfiguration = (Configuration)xs.Deserialize(file);
                newConfiguration.SetOwner();
                Configuration = newConfiguration;
            }
        }
    }
}
