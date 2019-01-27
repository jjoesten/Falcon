using PoKeysDevice_DLL;
using System.Collections.Generic;

namespace Aesalon
{
    public class PoKeysEnumerator
    {
        private static PoKeysEnumerator singleton;
        public static PoKeysEnumerator Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new PoKeysEnumerator();
                return singleton;
            }
        }

        public List<AvailablePoKeys> AvailablePoKeysList { get; }

        public readonly PoKeysDevice PoKeysDevice = new PoKeysDevice();

        public PoKeysEnumerator()
        {
            AvailablePoKeysList = new List<AvailablePoKeys>();
            RefreshAvailablePoKeysList();
        }

        private void RefreshAvailablePoKeysList()
        {
            AvailablePoKeysList.Clear();

            int nbPokeys = PoKeysDevice.EnumerateDevices();

            for (int pokeysIndex = 0; pokeysIndex < nbPokeys; pokeysIndex++)
            {
                if (PoKeysDevice.ConnectToDevice(pokeysIndex))
                {
                    int pokeysSerial = 0;
                    int firmwareMajor = 0;
                    int firmwareMinor = 0;
                    PoKeysDevice.GetDeviceIDEx(ref pokeysSerial, ref firmwareMajor, ref firmwareMinor);

                    string pokeysName = PoKeysDevice.GetDeviceName();

                    byte pokeysUserId = 0;
                    bool pokeysUserIdOk = PoKeysDevice.GetUserID(ref pokeysUserId);

                    AvailablePoKeys availablePoKeys = new AvailablePoKeys(pokeysSerial, pokeysName, pokeysUserIdOk ? pokeysUserId : (byte?)null, pokeysIndex);
                    AvailablePoKeysList.Add(availablePoKeys);

                    PoKeysDevice.DisconnectDevice();
                }
            }
        }
    }
}