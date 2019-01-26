using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum PowerBits : int
    {
        BusPowerBattery = 0x1,
        BusPowerEmergency = 0x2,
        BusPowerEssential = 0x4,
        BusPowerNonEssential = 0x8,
        MainGenerator = 0x10,
        StandbyGenerator = 0x20,
        /// <summary>
        /// True if JSF is running, can be used for magswitch
        /// </summary>
        JetFuelStarter = 0x40
    };
}
