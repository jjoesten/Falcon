using Aesalon.ArduinoDevices;
using System.Collections.Generic;

namespace Aesalon
{
    public class DEDuinoEnumerator
    {
        #region Singleton
        private static DEDuinoEnumerator singleton;
        public static DEDuinoEnumerator Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new DEDuinoEnumerator();
                return singleton;
            }
        }
        #endregion

        public List<string> AvailableCOMPorts { get; private set; }

        #region Construction
        public DEDuinoEnumerator()
        {
            AvailableCOMPorts = new List<string>();
            RefreshAvailableDEDuinoList();
        }
        #endregion

        private void RefreshAvailableDEDuinoList()
        {
            AvailableCOMPorts.Clear();
            var items = DEDuinoDevice.GetAvailableCOMPorts();
            AvailableCOMPorts.AddRange(items);
        }
    }
}
