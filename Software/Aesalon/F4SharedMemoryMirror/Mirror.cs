using F4SharedMem;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Serialization;

namespace F4SharedMemoryMirror
{
    public enum NetworkingMode
    {
        Client,
        Server
    }

    public sealed class Mirror : IDisposable
    {
        #region Singleton
        public static Mirror Singleton { get; } = new Mirror();

        // Explicit static constructor to tell compiler not to mark as beforefieldint
        static Mirror() { }

        private Mirror() { }
        #endregion

        private volatile bool disposed;
        private volatile bool running;

        private CancellationTokenSource wToken;
        private ActionBlock<DateTimeOffset> task;
        private SharedMemoryMirrorClient client;

        private readonly Writer writer = new Writer();
        private Reader reader;

        #region Polling Frequency
        private TimeSpan pollingFrequencyMS = TimeSpan.FromMilliseconds(100);
        public TimeSpan PollingFrequencyMS
        {
            get { return pollingFrequencyMS; }
            set
            {
                if (pollingFrequencyMS == value)
                    return;
                if (value.TotalMilliseconds < 0 || value.TotalMilliseconds > Int32.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(PollingFrequencyMS));

                pollingFrequencyMS = value;

                // TODO: Change timer interval (if necessary...?)
            }
        }
        #endregion

        public NetworkingMode NetworkingMode { get; set; }
        public int ServerPort { get; set; }
        public string ServerIP { get; set; }
        public ThreadPriority ThreadPriority { get; set; }
        public bool IsRunning { get { return running; } }

        ITargetBlock<DateTimeOffset> PollingTask(Action<DateTimeOffset> action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            // Declare block variable, it needs to be captured
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so the declaration and assignment need to be separate.
            // Async so you can wait easily when the delay comes.
            block = new ActionBlock<DateTimeOffset>(async now =>
            {
                // Perform action
                action(now);

                // Wait
                await Task.Delay(PollingFrequencyMS, cancellationToken).ConfigureAwait(false);

                // Post the action back to the block
                block.Post(DateTimeOffset.Now);
            }, new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });

            // Return block
            return block;
        }

        public void StartMirroring()
        {
            switch (NetworkingMode)
            {
                case NetworkingMode.Client:
                    RunClient();
                    break;
                case NetworkingMode.Server:
                    RunServer();
                    break;
                default:
                    break;
            }
        }

        public void StopMirroring()
        {
            running = false;

            using (wToken)
            {
                wToken.Cancel();
            }

            if (NetworkingMode == NetworkingMode.Server)
                SharedMemoryMirrorServer.TearDownService(ServerPort);

            wToken = null;
            task = null;
        }



        private void RunClient()
        {
            if (running)
                throw new InvalidOperationException();

            running = true;            

            try
            {
                var serverIPAddress = IPAddress.Parse(ServerIP);
                var endpoint = new IPEndPoint(serverIPAddress, ServerPort);
                client = new SharedMemoryMirrorClient(endpoint, "F4SharedMemoryMirrorService");
            }
            catch (RemotingException)
            {
                client = null;
            }

            // Create cancellation token
            wToken = new CancellationTokenSource();

            // Set the task
            task = (ActionBlock<DateTimeOffset>)PollingTask(now => ClientWork(), wToken.Token);

            // Start task, post the time
            task.Post(DateTimeOffset.Now);
        }

        private void ClientWork()
        {
            // TODO: REMOVE
            Debug.WriteLine("Do Client Work");
            try
            {
                byte[] primaryFlightData = null;
                byte[] flightData2 = null;
                byte[] osbData = null;
                byte[] intellivibeData = null;
                byte[] radioClientControlData = null;
                byte[] radioClientStatusData = null;

                if (client != null)
                {
                    try
                    {
                        primaryFlightData = client.GetPrimaryFlightData();
                        flightData2 = client.GetFlightData2();
                        osbData = client.GetOSBData();
                        intellivibeData = client.GetIntellivibeData();
                        radioClientControlData = client.GetRadioClientControlData();
                        radioClientStatusData = client.GetRadioClientStatusData();
                    }
                    catch (RemotingException e)
                    {
                        Debug.WriteLine(e);
                    }

                    writer.WritePrimaryFlightData(primaryFlightData);
                    writer.WriteFlightData2(flightData2);
                    writer.WriteOSBData(osbData);
                    writer.WriteIntellivibeData(intellivibeData);
                    writer.WriteRadioClientControlData(radioClientControlData);
                    writer.WriteRadioClientStatusData(radioClientStatusData);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void RunServer()
        {
            if (running)
                throw new InvalidOperationException();

            running = true;

            reader = new Reader();

            try
            {
               // SharedMemoryMirrorServer.TearDownService(ServerPort);
            }
            catch (RemotingException) { }

            try
            {
                SharedMemoryMirrorServer.CreateService("F4SharedMemoryMirrorService", ServerPort);
            }
            catch (RemotingException) { }

            // Create cancellation token
            wToken = new CancellationTokenSource();

            // Set task
            task = (ActionBlock<DateTimeOffset>)PollingTask(now => ServerWork(), wToken.Token);

            // Start task
            task.Post(DateTimeOffset.Now);
        }

        private void ServerWork()
        {
            // TODO: REMOVE
            Debug.WriteLine("Do Server Work");
            try
            {
                var primaryFlightData = reader.GetRawPrimaryFlightData();
                var flightData2 = reader.GetRawFlightData2();
                var osbData = reader.GetRawOSBData();
                var intellivibeData = reader.GetRawIntelliVibeData();
                var radioClientControlData = reader.GetRawRadioClientControlData();
                var radioClientStatusData = reader.GetRawRadioClientStatusData();

                SharedMemoryMirrorServer.SetPrimaryFlightData(primaryFlightData);
                SharedMemoryMirrorServer.SetFlightData2(flightData2);
                SharedMemoryMirrorServer.SetOSBData(osbData);
                SharedMemoryMirrorServer.SetIntellivibeData(intellivibeData);
                SharedMemoryMirrorServer.SetRadioClientControlData(radioClientControlData);
                SharedMemoryMirrorServer.SetRadioClientStatusData(radioClientStatusData);
            }
            catch (RemotingException e)
            {
                Debug.WriteLine(e);
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (reader != null)
                    {
                        reader.Dispose();                       
                    }
                    if (writer != null)
                    {
                        writer.Dispose();
                    }
                    if (wToken != null)
                    {
                        wToken.Dispose();
                    }
                }

                disposed = true;
            }
        }
        #endregion
    }
}
