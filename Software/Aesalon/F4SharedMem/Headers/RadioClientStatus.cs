using System;
using System.Runtime.InteropServices;

namespace F4SharedMem
{
    [ComVisible(true)]
    [Serializable]
    public struct RadioClientStatus
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte ClientFlags;
    }

    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum ClientFlags
    {
        /// <summary>
        /// No status indications present.
        /// </summary>
        AllClear = 0x00,
        /// <summary>
        /// Voice client program running.
        /// </summary>
        ClientActive = 0x01,
        /// <summary>
        /// Connection established with voice server.
        /// </summary>
        Connected = 0x02,
        /// <summary>
        /// Client failed to connect to voice server.
        /// </summary>
        ConnectionFail = 0x8000000,
        /// <summary>
        /// Bad IP address supplied for voice server.
        /// </summary>
        HostUnknown = 0x10000000,
        /// <summary>
        /// Password rejected by voice server.
        /// </summary>
        BadPassword = 0x20000000,
        /// <summary>
        /// No input device detected by voice client.
        /// </summary>
        NoMicrophone = 0x40000000,
        /// <summary>
        /// No output device detected by voice client
        /// </summary>
        NoSpeakers = -2147483648, //0x80000000
        /// <summary>
        /// Mask including all the error bits
        /// </summary>
        ErrorMask = -134217728, // 0xF8000000
    };
}
