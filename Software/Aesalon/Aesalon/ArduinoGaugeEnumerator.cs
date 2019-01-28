using Aesalon.ArduinoDevices;
using System.Collections.Generic;

namespace Aesalon
{
    public class ArduinoGaugeEnumerator
    {
        #region Singleton
        private static ArduinoGaugeEnumerator singleton;
        public static ArduinoGaugeEnumerator Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new ArduinoGaugeEnumerator();
                return singleton;
            }
        }
        #endregion

        public List<GaugeDevice> AvailableArduinoGaugeDeviceList { get; }

        public ArduinoGaugeEnumerator()
        {
            AvailableArduinoGaugeDeviceList = new List<GaugeDevice>();
            RefreshAvailableArduinoGaugeDeviceList();
        }

        private void RefreshAvailableArduinoGaugeDeviceList()
        {
            AvailableArduinoGaugeDeviceList.Clear();
            var items = GaugeDriver.GetConnectedDevices();
            AvailableArduinoGaugeDeviceList.AddRange(items);
        }
    }
}
