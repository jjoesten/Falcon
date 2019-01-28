using CommandMessenger;
using CommandMessenger.TransportLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;

namespace Aesalon.ArduinoDevices
{
    public class GaugeDriver : IDisposable
    {
        enum Command
        {
            HandshakeRequest,
            HandshakeResponse,
            SetTarget,
            Status
        }

        SerialTransport serialTransport;
        CmdMessenger cmdMessenger;

        #region Static Methods and Properties

        public static int BAUD_RATE => 57600;

        public static List<GaugeDevice> GetConnectedDevices()
        {
            List<GaugeDevice> devices = new List<GaugeDevice>();

            string[] ports = SerialPort.GetPortNames();
            foreach (var portname in ports)
            {
                Debug.WriteLine("Attempting to connect to " + portname);
                var device = DetectArduinoGaugeDevice(portname);
                if (device != null)
                    devices.Add(device);
            }

            return devices;
        }

        private static GaugeDevice DetectArduinoGaugeDevice(string portname)
        {
            SerialTransport serialTransport = new SerialTransport()
            {
                CurrentSerialSettings = { PortName = portname, BaudRate = BAUD_RATE, DtrEnable = false }
            };
            CmdMessenger cmdMessenger = new CmdMessenger(serialTransport)
            {
                BoardType = BoardType.Bit16
            };

            try
            {
                cmdMessenger.StartListening();

                var command = new SendCommand((int)Command.HandshakeRequest, (int)Command.HandshakeResponse, 1000);
                var handshakeResultCommand = cmdMessenger.SendCommand(command);

                if (handshakeResultCommand.Ok)
                {
                    // Read response
                    var software = handshakeResultCommand.ReadStringArg();
                    var serialNumber = handshakeResultCommand.ReadStringArg();

                    if (software.Contains("ArduinoGauge"))
                    {
                        // Create ArduinoGaugeDevice
                        GaugeDevice device = new GaugeDevice()
                        {
                            PortName = portname,
                            SerialNumber = serialNumber
                        };

                        return device;
                    }
                    else
                    {
                        Debug.WriteLine("Connected to Arduino, but not an ArduinoGauge device.");
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Handshake FAILED");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
            finally
            {
                cmdMessenger.StopListening();
                cmdMessenger.Dispose();
                cmdMessenger = null;
                serialTransport.Dispose();
                serialTransport = null;
            }
        }

        #endregion

        #region Public Methods and Properties

        public byte StepperMotorCount => 12;
        public string SerialNumber { get; private set; }

        public void SetTarget(byte stepperMotorIndex, ushort targetSteps)
        {
            var command = new SendCommand((int)Command.SetTarget);
            command.AddArgument(stepperMotorIndex);
            command.AddArgument(targetSteps);

            cmdMessenger.SendCommand(command);
        }

        #endregion

        #region Construction

        public GaugeDriver(GaugeDevice device)
        {
            SerialNumber = device.SerialNumber;

            serialTransport = new SerialTransport()
            {
                CurrentSerialSettings = { PortName = device.PortName, BaudRate = BAUD_RATE, DtrEnable = false }
            };

            cmdMessenger = new CmdMessenger(serialTransport)
            {
                BoardType = BoardType.Bit16
            };

            cmdMessenger.StartListening();
        }

        public void Dispose()
        {
            cmdMessenger.StopListening();
            cmdMessenger.Dispose();
            serialTransport.Dispose();
            cmdMessenger = null;
            serialTransport = null;
        }

        #endregion
    }
}
