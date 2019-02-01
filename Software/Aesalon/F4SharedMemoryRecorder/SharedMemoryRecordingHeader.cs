using System;
using System.Runtime.InteropServices;

namespace F4SharedMemoryRecorder
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SharedMemoryRecordingHeader
    {
        public byte[] Magic { get; internal set; }
        public ulong NumSamples { get; internal set; }
        public ushort SampleInterval { get; internal set; }
    }
}