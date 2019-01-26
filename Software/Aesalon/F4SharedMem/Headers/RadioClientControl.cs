using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RadioClientControl
    {
        /// <summary>
        /// Socket number to use in contacting the server.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int PortNumber;

        /// <summary>
        /// String representation of server IPv4 address.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.RCC_STRING_LENGTH)]
        public byte[] Address;

        /// <summary>
        /// Plain text password for voice server access.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.RCC_STRING_LENGTH)]
        public byte[] Password;

        /// <summary>
        /// Player nickname
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.RCC_STRING_LENGTH)]
        public byte[] Nickname;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NUMBER_OF_RADIOS)]
        public RadioChannel[] Radios;

        /// <summary>
        /// Tell the client we are ready to try a connection with the current settings.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool SignalConnect;

        /// <summary>
        /// Indicate to external client that is should shut down.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool TerminateClient;

        /// <summary>
        /// True when in 3D world, false for UI state.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool FlightMode;

        /// <summary>
        /// True when external voice client should use AGC features.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool UseAGC;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NUMBER_OF_DEVICES)]
        public RadioDevice[] Devices;

        /// <summary>
        /// Number of players for whom we have data in the telemetry map.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int PlayerCount;

        /// <summary>
        /// Array of player telemetry data relative to ownship (held in entry zero)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.PLAYER_MAP_SIZE)]
        public Telemetry[] PlayerMap;
    }

    [ComVisible(true)]
    [Serializable]
    public enum Radios
    {
        UHF = 0,
        VHF,
        GUARD,
        NUMBER_OF_RADIOS,
    };

    [ComVisible(true)]
    [Serializable]
    public enum Devices
    {
        MAIN = 0,
        NUMBER_OF_DEVICES,
    };

    [ComVisible(true)]
    [Serializable]
    public struct Constants
    {
        public const int MAX_VOLUME = 0;
        public const int MIN_VOLUME = 10000;
        /// <summary>
        /// On a scale from +6dB to -40dB
        /// </summary>
        public const int Zero_dB_Raw_Volume_Default = 1304;
        public const int NAME_LEN = 20;
        public const int PLAYER_MAP_SIZE = 96;
        public const int NUMBER_OF_RADIOS = 3;
        public const int NUMBER_OF_DEVICES = 1;
        public const int RCC_STRING_LENGTH = 64;
    }

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Telemetry
    {
        /// <summary>
        /// Height above terrain in feet.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Agl;

        /// <summary>
        /// Range of remote player to ownship in nautical miles.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Range;

        /// <summary>
        /// Status information.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint Flags;

        /// <summary>
        /// Copy of player logbook name.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.NAME_LEN + 1)]
        public byte[] LogbookName;


        [MarshalAs(UnmanagedType.U1)]
        public byte padding1;

        [MarshalAs(UnmanagedType.U1)]
        public byte padding2;

        [MarshalAs(UnmanagedType.U1)]
        public byte padding3;
    };

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RadioDevice
    {
        /// <summary>
        /// INTERCOM volume.
        /// </summary>
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int IcVolume;
    }

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RadioChannel
    {
        /// <summary>
        /// 6 digit MHz frequency x1000 (i.e. no decimal places)
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Frequency;

        /// <summary>
        /// 0-15000 range, hight to low respectively.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int RxVolume;

        /// <summary>
        /// True for transmit switch activated.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool PttDepressed;

        /// <summary>
        /// True if this channel is associated with a radio that is on.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOn;

        [MarshalAs(UnmanagedType.U1)]
        public byte padding1;

        [MarshalAs(UnmanagedType.U1)]
        public byte padding2;
    }

    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum TelemetryFlags : byte
    {
        NoFlags = 0x00,
        HasPlayerLoS = 0x01,
        IsAircraft = 0x02,
    };
}
