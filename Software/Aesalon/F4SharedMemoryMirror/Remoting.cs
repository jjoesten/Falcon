using System;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace F4SharedMemoryMirror
{
    public interface ISharedMemoryMirror
    {
        byte[] GetPrimaryFlightData();
        byte[] GetFlightData2();
        byte[] GetOSBData();
        byte[] GetIntellivibeData();
        byte[] GetRadioClientControlData();
        byte[] GetRadioClientStatusData();
    }

    public class SharedMemoryMirrorClient : ISharedMemoryMirror
    {
        private readonly ISharedMemoryMirror server;

        public SharedMemoryMirrorClient(IPEndPoint endpoint, string serviceName)
        {
            //IDictionary prop = new Hashtable
            //{
            //    ["port"] = endpoint.Port,
            //    ["machineName"] = endpoint.Address.ToString(),
            //    ["priority"] = 100
            //};
            TcpClientChannel chan = new TcpClientChannel();
            try
            {
                ChannelServices.RegisterChannel(chan, false);
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

                // create instance of remote object
                server = (SharedMemoryMirrorServer)Activator.GetObject(
                    typeof(SharedMemoryMirrorServer),
                    string.Format("{0}:{1}/{2}", endpoint.Address, endpoint.Port.ToString(CultureInfo.InvariantCulture), serviceName));
            }
            catch (Exception) { }

        }

        #region ISharedMemoryMirror
        public byte[] GetFlightData2()
        {
            return server.GetFlightData2();
        }

        public byte[] GetIntellivibeData()
        {
            return server.GetIntellivibeData();
        }

        public byte[] GetOSBData()
        {
            return server.GetOSBData();
        }

        public byte[] GetPrimaryFlightData()
        {
            return server.GetPrimaryFlightData();
        }

        public byte[] GetRadioClientControlData()
        {
            return server.GetRadioClientControlData();
        }

        public byte[] GetRadioClientStatusData()
        {
            return server.GetRadioClientStatusData();
        }
        #endregion
    }

    public class SharedMemoryMirrorServer : MarshalByRefObject, ISharedMemoryMirror
    {
        private static byte[] primaryFlightData;
        private static byte[] flightData2;
        private static byte[] osbData;
        private static byte[] intellivibeData;
        private static byte[] radioClientControlData;
        private static byte[] radioClientStatusData;

        private SharedMemoryMirrorServer()
        { }

        #region ISharedMemoryMirror
        public byte[] GetFlightData2()
        {
            return flightData2;
        }

        public byte[] GetIntellivibeData()
        {
            return intellivibeData;
        }

        public byte[] GetOSBData()
        {
            return osbData;
        }

        public byte[] GetPrimaryFlightData()
        {
            return primaryFlightData;
        }

        public byte[] GetRadioClientControlData()
        {
            return radioClientControlData;
        }

        public byte[] GetRadioClientStatusData()
        {
            return radioClientStatusData;
        }
        #endregion

        internal static void CreateService(string serviceName, int port)
        {
            IDictionary prop = new Hashtable
            {
                ["port"] = port,
                ["priority"] = 100
            };
            TcpServerChannel channel;

            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                channel = new TcpServerChannel(prop, null, null);
                ChannelServices.RegisterChannel(channel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(SharedMemoryMirrorServer),
                    serviceName,
                    WellKnownObjectMode.Singleton);
            }
            catch (Exception)
            {
            }
        }

        internal static void TearDownService(int port)
        {
            IDictionary prop = new Hashtable();
            prop["port"] = port;
            TcpServerChannel channel = null;

            try
            {
                channel = new TcpServerChannel(prop, null, null);
                ChannelServices.UnregisterChannel(channel);
            }
            catch (Exception)
            {
            }
        }

        public static void SetPrimaryFlightData(byte[] data)
        {
            primaryFlightData = data;
        }

        public static void SetFlightData2(byte[] data)
        {
            flightData2 = data;
        }

        public static void SetOSBData(byte[] data)
        {
            osbData = data;
        }
        public static void SetIntellivibeData(byte[] data)
        {
            intellivibeData = data;
        }
        public static void SetRadioClientControlData(byte[] data)
        {
            radioClientControlData = data;
        }
        public static void SetRadioClientStatusData(byte[] data)
        {
            radioClientStatusData = data;
        }
    }
}