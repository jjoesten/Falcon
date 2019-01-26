using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum TacanBits : byte
    {
        /// <summary>
        /// True in this bit position if band is X
        /// </summary>
        band = 0x01,
        /// <summary>
        /// True in this bit position if domain is air to air
        /// </summary>
        mode = 0x02,
    };
}
