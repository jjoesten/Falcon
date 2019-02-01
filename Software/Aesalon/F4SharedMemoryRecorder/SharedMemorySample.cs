using System;
using System.Runtime.InteropServices;

namespace F4SharedMemoryRecorder
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SharedMemorySample
    {
        public ushort PrimaryFlightDataLength { get; internal set; }
        public byte[] PrimaryFlightData { get; internal set; }
        public ushort FlightData2Length { get; internal set; }
        public byte[] FlightData2 { get; internal set; }
        public ushort OSBDataLength { get; internal set; }
        public byte[] OSBData { get; internal set; }
        public ushort IntellivibeDataLength { get; internal set; }
        public byte[] IntellivibeData { get; internal set; }
        public ushort RadioClientControlDataLength { get; internal set; }
        public byte[] RadioClientControlData { get; internal set; }
        public ushort RadioClientStatusDataLength { get; internal set; }
        public byte[] RadioClientStatusData { get; internal set; }
    }
}