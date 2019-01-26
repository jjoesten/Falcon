using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FlightData2
    {
        public const int RWRINFO_SIZE = 512;
        public const int MAX_CALLSIGNS = 32;

        // VERSION 1
        /// <summary>
        /// Ownship engine nozzle2 percent open (0-100)
        /// </summary>
        public float nozzlePos2;
        /// <summary>
        /// Ownship engine rpm2 percent (0-103)
        /// </summary>
        public float rmp2;
        /// <summary>
        /// Ownship Forward Turbine Inlet Temp2 (Degrees C)
        /// </summary>
        public float ftit2;
        /// <summary>
        /// Ownship Oil Pressure2 percent (0-100)
        /// </summary>
        public float oilPressure2;
        /// <summary>
        /// Current mode selected for HSI/eHSI (added in BMS4)
        /// </summary>
        public byte navMode;
        /// <summary>
        /// Ownship barometric altitude given by AAU (depends on calibration)
        /// </summary>
        public float aauz;
        /// <summary>
        /// Tacan band/mode settings for UFC and AUX COMM
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TacanSources.NUMBER_OF_SOURCES)]
        public byte[] tacanInfo;

        // VERSION 2
        /// <summary>
        /// Barometric altitude calibration (depends on cal type)
        /// </summary>
        public int AltCalReading;
        /// <summary>
        /// Various altimeter bits, see <see cref="AltBits"/> enum for details.
        /// </summary>
        public int altBits;
        /// <summary>
        /// Ownship power bus / generator states, see <see cref="PowerBits"/> enum for details.
        /// </summary>
        public int powerBits;
        /// <summary>
        /// Cockpit indicator lights blink status, see <see cref="BlinkBits"/> enum for details.
        /// NOTE: these bits indicate only if a lamp is blinking, in addition to the actual on/off bits.
        /// Its up to the external program to implement the blinking behavior.
        /// </summary>
        public int blinkBits;
        /// <summary>
        /// Ownship CMDS mode state, see <see cref="CmdsModes"/> enum for details
        /// </summary>
        public int cmdsMode;
        /// <summary>
        /// Backup (BUP) UFH channel preset
        /// </summary>
        public int BupUhfPreset;

        // VERSION 3
        /// <summary>
        /// BUP UHF channel frequency
        /// </summary>
        public int BupUhfFreq;
        /// <summary>
        /// Ownship cabin altitude
        /// </summary>
        public float cabinAlt;
        /// <summary>
        /// Ownship Hydraulic Pressure A
        /// </summary>
        public float hydPressureA;
        /// <summary>
        /// Ownship Hydraulic Pressure B
        /// </summary>
        public float hydPressureB;
        /// <summary>
        /// Current time in seconds (max 60 * 60 * 24)
        /// </summary>
        public int currentTime;
        /// <summary>
        /// Ownship ACD index number, i.e. which aircraft type are we flying.
        /// </summary>
        public short vehicleACD;
        /// <summary>
        /// Version of FlightData2 mem area
        /// </summary>
        public int VersionNum2;

        // VERSION 4
        /// <summary>
        /// Ownship fuel flow2 (Lbs/Hour)
        /// </summary>
        public float fuelFlow2;

        // VERSION 5/6
        /// <summary>
        /// [512] New RWR info
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = RWRINFO_SIZE)]
        public byte[] RwrInfo;
        /// <summary>
        /// Ownship LEF position
        /// </summary>
        public float leftPos;
        /// <summary>
        /// Ownship TEF position
        /// </summary>
        public float rightPos;

        // VERSION 6
        /// <summary>
        /// Ownship VTOL exhaust angle
        /// </summary>
        public float vtolPos;

        // VERSION 9
        /// <summary>
        /// Number of pilots in a MP session
        /// </summary>
        public byte pilotsOnline;

        /// <summary>
        /// List of pilots callsigns connected to an MP session
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CALLSIGNS)]
        public Callsign_LineOfText[] pilotCallsign;

        /// <summary>
        /// Status of the MP pilots, see <see cref="FlyStates"/> enum.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CALLSIGNS)]
        public byte[] pilotsStatus;
    }
}
