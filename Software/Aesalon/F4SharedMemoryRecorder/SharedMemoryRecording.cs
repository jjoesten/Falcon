using F4SharedMem;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Timers;

namespace F4SharedMemoryRecorder
{
    internal interface ISharedMemoryRecording : IDisposable
    {
        event EventHandler<EventArgs> RecordingStarted;
        event EventHandler<EventArgs> PlaybackStarted;
        event EventHandler<EventArgs> Stopped;
        event EventHandler<PlaybackProgressEventArgs> PlaybackProgress;
        string FileName { get; }
        ushort SampleInterval { get; }
        ulong NumSamples { get; }
        ulong CurrentSample { get; }
        bool LoopOnPlayback { get; set; }
        void Record();
        void Stop();
        void Play();

    }

    internal class SharedMemoryRecording : ISharedMemoryRecording
    {
        #region Private Class Members
        private const int DEFAULT_SAMPLING_INTERVAL_MS = 20;
        private readonly Timer recordingTimer = new Timer(DEFAULT_SAMPLING_INTERVAL_MS);
        private readonly Timer playbackTimer = new Timer(DEFAULT_SAMPLING_INTERVAL_MS);

        private readonly Reader sharedMemoryReader = new Reader();
        private readonly Writer sharedMemoryWriter = new Writer();

        private BinaryWriter gzipStreamBinaryWriter;
        private BinaryReader gzipStreamBinaryReader;
        private FileStream fileStream;
        private GZipStream gzipStream;

        private object playbackLock = new object();
        private object recordingLock = new object();
        #endregion

        #region Public Properties
        public string FileName { get; private set; }
        public ushort SampleInterval { get; private set; }
        public ulong NumSamples { get; private set; }
        public ulong CurrentSample { get; private set; }
        public bool LoopOnPlayback { get; set; }
        #endregion

        #region Events
        public event EventHandler<EventArgs> RecordingStarted;
        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<EventArgs> Stopped;
        public event EventHandler<PlaybackProgressEventArgs> PlaybackProgress;
        #endregion

        #region Construction / Destruction

        public SharedMemoryRecording(string filename)
        {
            FileName = filename;
            recordingTimer.Elapsed += RecordingTimer_Elapsed;
            playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (recordingTimer != null)
                    recordingTimer.Stop();
                if (playbackTimer != null)
                    playbackTimer.Stop();
                SimplifiedCommon.Util.DisposeObject(recordingTimer);
                SimplifiedCommon.Util.DisposeObject(playbackTimer);
            }
        }

        ~SharedMemoryRecording()
        {
            Dispose(false);
        }

        #endregion

        #region Record

        public void Record()
        {
            lock (recordingLock)
            {
                if (playbackTimer.Enabled)
                    playbackTimer.Stop();

                try
                {
                    fileStream = new FileStream(path: FileName, mode: FileMode.Create, access: FileAccess.ReadWrite, share: FileShare.None, bufferSize: 4096, options: FileOptions.SequentialScan);
                    WriterHeader();
                    gzipStream = new GZipStream(stream: fileStream, compressionLevel: CompressionLevel.Optimal, leaveOpen: true);
                    gzipStreamBinaryWriter = new BinaryWriter(output: gzipStream, encoding: Encoding.UTF8, leaveOpen: true);
                }
                catch
                {
                    Stopped(this, null);
                    return;
                }

                RecordingStarted?.Invoke(this, null);

                recordingTimer.Start();
            }
        }

        private void StopRecording()
        {
            lock (recordingLock)
            {
                recordingTimer.Stop();
                gzipStreamBinaryWriter.Flush();
                gzipStream.Flush();
                fileStream.Flush();
                SimplifiedCommon.Util.DisposeObject(gzipStreamBinaryWriter);
                SimplifiedCommon.Util.DisposeObject(gzipStream);
                WriterHeader();
                SimplifiedCommon.Util.DisposeObject(fileStream);
                gzipStreamBinaryWriter = null;
                gzipStream = null;
                fileStream = null;
            }
        }

        public void WriterHeader()
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            using (var writer = new BinaryWriter(output: fileStream, encoding: Encoding.UTF8, leaveOpen: true))
            {
                var header = new SharedMemoryRecordingHeader();
                header.Magic = new byte[4] { (byte)'S', (byte)'M', (byte)'X', (byte)'1' };
                header.NumSamples = NumSamples;
                header.SampleInterval = DEFAULT_SAMPLING_INTERVAL_MS;
                writer.Write(header.Magic, 0, header.Magic.Length);
                writer.Write(header.NumSamples);
                writer.Write(header.SampleInterval);
                writer.Flush();
            }
        }

        private void RecordingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (recordingLock)
            {
                var sample = ReadSampleFromSharedMemory();
                NumSamples++;
                CurrentSample++;
                WriteSampleToFile(sample);
            }
        }

        private SharedMemorySample ReadSampleFromSharedMemory()
        {
            var sample = new SharedMemorySample();
            sample.PrimaryFlightData = sharedMemoryReader.GetRawPrimaryFlightData();
            sample.PrimaryFlightDataLength = (ushort)(sample.PrimaryFlightData != null ? sample.PrimaryFlightData.Length : 0);
            sample.FlightData2 = sharedMemoryReader.GetRawFlightData2();
            sample.FlightData2Length = (ushort)(sample.FlightData2 != null ? sample.FlightData2.Length : 0);
            sample.OSBData = sharedMemoryReader.GetRawOSBData();
            sample.OSBDataLength = (ushort)(sample.OSBData != null ? sample.OSBData.Length : 0);
            sample.IntellivibeData = sharedMemoryReader.GetRawIntelliVibeData();
            sample.IntellivibeDataLength = (ushort)(sample.IntellivibeData != null ? sample.IntellivibeData.Length : 0);
            sample.RadioClientControlData = sharedMemoryReader.GetRawRadioClientControlData();
            sample.RadioClientControlDataLength = (ushort)(sample.RadioClientControlData != null ? sample.RadioClientControlData.Length : 0);
            sample.RadioClientStatusData = sharedMemoryReader.GetRawRadioClientStatusData();
            sample.RadioClientStatusDataLength = (ushort)(sample.RadioClientStatusData != null ? sample.RadioClientStatusData.Length : 0);
            return sample;
        }

        private void WriteSampleToFile(SharedMemorySample sample)
        {
            if (gzipStreamBinaryWriter == null)
                return;

            gzipStreamBinaryWriter.Write(sample.PrimaryFlightDataLength);
            if (sample.PrimaryFlightDataLength > 0)
                gzipStreamBinaryWriter.Write(sample.PrimaryFlightData);

            gzipStreamBinaryWriter.Write(sample.FlightData2Length);
            if (sample.FlightData2Length > 0)
                gzipStreamBinaryWriter.Write(sample.FlightData2);

            gzipStreamBinaryWriter.Write(sample.OSBDataLength);
            if (sample.OSBDataLength > 0)
                gzipStreamBinaryWriter.Write(sample.OSBData);

            gzipStreamBinaryWriter.Write(sample.IntellivibeDataLength);
            if (sample.OSBDataLength > 0)
                gzipStreamBinaryWriter.Write(sample.IntellivibeData);

            gzipStreamBinaryWriter.Write(sample.RadioClientStatusDataLength);
            if (sample.RadioClientStatusDataLength > 0)
                gzipStreamBinaryWriter.Write(sample.RadioClientStatusData);

            gzipStreamBinaryWriter.Write(sample.RadioClientControlDataLength);
            if (sample.RadioClientControlDataLength > 0)
                gzipStreamBinaryWriter.Write(sample.RadioClientControlData);

            gzipStreamBinaryWriter.Flush();
            gzipStream.Flush();
            fileStream.Flush();
        }

        #endregion

        #region Play

        public void Play()
        {
            lock (playbackLock)
            {
                try
                {
                    fileStream = new FileStream(path: FileName, mode: FileMode.Open, access: FileAccess.ReadWrite, share: FileShare.None, bufferSize: 4096, options: FileOptions.SequentialScan);
                    var header = ReadHeader();
                    NumSamples = header.NumSamples;
                    CurrentSample = 0;
                    SampleInterval = header.SampleInterval;
                    gzipStream = new GZipStream(stream: fileStream, mode: CompressionMode.Decompress, leaveOpen: true);
                    gzipStreamBinaryReader = new BinaryReader(input: gzipStream, encoding: Encoding.UTF8, leaveOpen: true);
                }
                catch (IOException)
                {
                    Stopped(this, null);
                    return;
                }

                PlaybackStarted?.Invoke(this, null);

                playbackTimer.Start();
            }
        }

        public SharedMemoryRecordingHeader ReadHeader()
        {
            var header = new SharedMemoryRecordingHeader();
            using (var reader = new BinaryReader(input: fileStream, encoding: Encoding.UTF8, leaveOpen: true))
            {
                header.Magic = reader.ReadBytes(4);
                header.NumSamples = reader.ReadUInt64();
                header.SampleInterval = reader.ReadUInt16();
            }
            return header;
        }

        private void PlaybackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (playbackLock)
            {
                if (recordingTimer.Enabled)
                {
                    playbackTimer.Stop();
                    return;
                }

                CurrentSample++;
                if (CurrentSample > NumSamples)
                {
                    if (LoopOnPlayback)
                    {
                        StopPlaying();
                        Play();
                        return;
                    }
                    else
                    {
                        Stop();
                        return;
                    }
                }

                var sample = ReadNextSampleFromFile();
                WriteSampleToSharedMemory(sample);
                ReportPlaybackProgress();
            }
        }

        private void StopPlaying()
        {
            lock (playbackLock)
            {
                playbackTimer.Stop();
                CloseFileOpenedForPlay();
            }
        }

        private void CloseFileOpenedForPlay()
        {
            SimplifiedCommon.Util.DisposeObject(gzipStreamBinaryReader);
            SimplifiedCommon.Util.DisposeObject(gzipStream);
            SimplifiedCommon.Util.DisposeObject(fileStream);
            gzipStreamBinaryReader = null;
            gzipStream = null;
            fileStream = null;
        }

        private SharedMemorySample ReadNextSampleFromFile()
        {
            var sample = new SharedMemorySample();

            sample.PrimaryFlightDataLength = gzipStreamBinaryReader.ReadUInt16();
            if (sample.PrimaryFlightDataLength > 0)            
                sample.PrimaryFlightData = gzipStreamBinaryReader.ReadBytes(sample.PrimaryFlightDataLength);
            
            sample.FlightData2Length = gzipStreamBinaryReader.ReadUInt16();
            if (sample.FlightData2Length > 0)            
                sample.FlightData2 = gzipStreamBinaryReader.ReadBytes(sample.FlightData2Length);
            
            sample.OSBDataLength = gzipStreamBinaryReader.ReadUInt16();
            if (sample.OSBDataLength > 0)            
                sample.OSBData = gzipStreamBinaryReader.ReadBytes(sample.OSBDataLength);
            
            sample.IntellivibeDataLength = gzipStreamBinaryReader.ReadUInt16();
            if (sample.IntellivibeDataLength > 0)            
                sample.IntellivibeData = gzipStreamBinaryReader.ReadBytes(sample.IntellivibeDataLength);
            
            sample.RadioClientStatusDataLength = gzipStreamBinaryReader.ReadUInt16();
            if (sample.RadioClientStatusDataLength > 0)            
                sample.RadioClientStatusData = gzipStreamBinaryReader.ReadBytes(sample.RadioClientStatusDataLength);
            
            sample.RadioClientControlDataLength = gzipStreamBinaryReader.ReadUInt16();
            if (sample.RadioClientControlDataLength > 0)            
                sample.RadioClientControlData = gzipStreamBinaryReader.ReadBytes(sample.RadioClientControlDataLength);
            
            return sample;
        }

        private void WriteSampleToSharedMemory(SharedMemorySample sample)
        {
            if (sample.PrimaryFlightData != null)
            {
                try
                {
                    sharedMemoryWriter.WritePrimaryFlightData(sample.PrimaryFlightData);
                }
                catch { }
            }
            if (sample.FlightData2 != null)
            {
                try
                {
                    sharedMemoryWriter.WriteFlightData2(sample.FlightData2);
                }
                catch { }
            }
            if (sample.OSBData != null)
            {
                try
                {
                    sharedMemoryWriter.WriteOSBData(sample.OSBData);
                }
                catch { }
            }
            if (sample.IntellivibeData != null)
            {
                try
                {
                    sharedMemoryWriter.WriteIntellivibeData(sample.IntellivibeData);
                }
                catch { }
            }
            if (sample.RadioClientStatusData != null)
            {
                try
                {
                    sharedMemoryWriter.WriteRadioClientStatusData(sample.RadioClientStatusData);
                }
                catch { }
            }
            if (sample.RadioClientControlData != null)
            {
                try
                {
                    sharedMemoryWriter.WriteRadioClientControlData(sample.RadioClientControlData);
                }
                catch { }
            }
        }

        private void ReportPlaybackProgress()
        {
            PlaybackProgress?.Invoke(this, new PlaybackProgressEventArgs { ProgressPercentage = (float)CurrentSample / (float)NumSamples });
        }



        #endregion

        #region Stop

        public void Stop()
        {
            lock (recordingLock)            
                if (recordingTimer.Enabled)
                    StopRecording();

            lock (playbackLock)
                if (playbackTimer.Enabled)
                    StopPlaying();

            Stopped?.Invoke(this, null);
            
        }

        #endregion
    }

    internal class PlaybackProgressEventArgs : EventArgs
    {
        public float ProgressPercentage { get; internal set; }
    }
}
