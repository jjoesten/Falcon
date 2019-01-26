using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    /// <summary>
    /// Indicates *IF* a lamp is blinking, does not implement the actual on/off pattern logic
    /// </summary>
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum BlinkBits : int
    {
        /// <summary>
        /// Defined in HsiBits - slow flashing for outer marker
        /// </summary>
        OuterMarker = 0x01,
        /// <summary>
        /// Defined in HsiBits = fast flashing for middle marker
        /// </summary>
        MiddleMarker = 0x02,
        /// <summary>
        /// Define in LightBits2 - probe heat system is tested
        /// </summary>
        PROBEHEAT = 0x04,
        /// <summary>
        /// Defined in LightBits2 - Search function in NOT activated and a search radar is painting ownship
        /// </summary>
        AuxSrch = 0x08,
        /// <summary>
        /// Defined in LightBits2 - Missile is fired at ownship
        /// </summary>
        Launch = 0x10,
        /// <summary>
        /// Defined in LightBits2 - Priority mode is enabled but more than 5 threat emitters are detected
        /// </summary>
        PriMode = 0x20,
        /// <summary>
        /// Defined in LightBits2 - Unknown is not active but EWS detects unknown radar
        /// </summary>
        Unk = 0x40,

        // NOT YET WORKING, defined for future use
        /// <summary>
        /// Defined in LightBits3 - Non-resetting fault
        /// </summary>
        Elec_Fault = 0x80,
        /// <summary>
        /// Defined in LightBits - Monitor fault during OBOGS
        /// </summary>
        OXY_BROW = 0x100,
        /// <summary>
        /// Defined in LightBits3 - Abnormal EPU operation
        /// </summary>
        EPUOn = 0x200,
        /// <summary>
        /// Defined in LightBits3 - Slow blinking: non-critical failure
        /// </summary>
        JFSOn_Slow = 0x400,
        /// <summary>
        /// Defined in LightBits3 - Fast blinking: Critical failure 
        /// </summary>
        JFSOn_Fast = 0x800,
    }

    
}
