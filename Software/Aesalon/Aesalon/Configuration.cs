using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Aesalon
{
    public class Configuration : BindableObject, IDisposable
    {
        // NOTE: Unsure if this format version stuff is necessary...
        [XmlIgnore]
        public static Version CurrentFormatVersion { get; } = new Version(1, 1);
        [XmlAttribute("formatVersion")]
        public string FormatVersion { get; set; }

        public double ReadFalconDataTimerIntervalMS
        {
            get { return FalconConnector.Singleton.ReadFalconDataTimerInterval.TotalMilliseconds; }
            set { FalconConnector.Singleton.ReadFalconDataTimerInterval = TimeSpan.FromMilliseconds(value); }
        }
                     
        #region SetOwner
        public void SetOwner()
        {
            // TODO: SetOwner for registered devices (pokeys, gauge drivers)
        }
        #endregion

        public void Dispose()
        {
            // TODO: Implement Configuration.Dispose
            // Dispose of registered devices
        }
    }
}
