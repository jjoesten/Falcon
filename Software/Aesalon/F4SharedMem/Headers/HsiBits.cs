using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum HsiBits : int
    {
        ToTrue = 0x1,
        IlsWarning = 0x2,
        CourseWarning = 0x4,
        Init = 0x8,
        /// <summary>
        /// NEVER SET
        /// </summary>
        TotalFlags = 0x10,
        ADI_OFF = 0x20,
        ADI_AUX = 0x40,
        ADI_GS = 0x80,
        ADI_LOC = 0x100,
        HSI_OFF = 0x200,
        BUP_ADI_OFF = 0x400,
        VVI = 0x800,
        AOA = 0x1000,
        AVTR = 0x2000,
        OuterMarker = 0x4000,
        MiddleMarker = 0x8000,
        FromTrue  = 0x10000,

        /// <summary>
        /// True if pilot is attached to an aircraft (i.e. not in UI state)
        /// NOTE: Not a lamp bit.
        /// </summary>
        Flying = -2147483648,

        /// <summary>
        /// Usedd with the MAL/IND light code to light up "everything"
        /// Please update this if you add/change bits
        /// </summary>
        AllLampHsiBitsOn = 0xE000
    };
}
