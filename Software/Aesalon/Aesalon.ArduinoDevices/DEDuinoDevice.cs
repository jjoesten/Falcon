using F4SharedMem;
using F4SharedMem.Headers;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Aesalon.ArduinoDevices
{
    public class DEDuinoDevice
    {
        #region Statics
        public static string[] GetAvailableCOMPorts()
        {
            string[] availableCOMPorts = SerialPort.GetPortNames();
            return availableCOMPorts;
        }
        #endregion

        const int BAUDRATE = 9600;
        const int BAUDRATE_MULTIPLIER = 12;

        SerialPort connection = new SerialPort();
        bool isClosing = false;
        char serialBuffer;
        int dedLoopCount = 0;
        int pfdLoopCount = 0;
        int cmdsLoopCount = 0;

        public FlightData FlightData { get; set; }

        public string PortName { get; private set; }

        public DEDuinoDevice(string portName)
        {
            PortName = portName;
            FlightData = new FlightData();
            ConnectToDevice();
        }

        /// <summary>
        /// Opens a connection to the device on the port specified during construction.
        /// </summary>
        /// <returns></returns>
        private bool ConnectToDevice()
        {
            try
            {
                if (!connection.IsOpen)
                {
                    connection.PortName = PortName;
                    connection.RtsEnable = true;
                    connection.Handshake = Handshake.None;
                    connection.BaudRate = BAUDRATE * BAUDRATE_MULTIPLIER;
                    connection.DataReceived += Connection_DataReceived;

                    isClosing = false;

                    connection.Open();
                }
            }
            catch (Exception)
            {
                // TODO: Handle connection error
            }

            return connection.IsOpen;
        }

        public void DisconnectDevice()
        {
            if (connection.IsOpen)
            {
                try
                {
                    isClosing = true;
                    Thread.Sleep(200);  // Allow time for interrupts to finish processing
                    connection.Close();
                }
                catch (Exception)
                {
                    // TODO: Handle Disconnection error
                }
            }
        }

        private void SendLine(string line, int length)
        {
            if (line.Length < length)
                length = line.Length;

            byte[] bytesToSend = Encoding.GetEncoding(1252).GetBytes(line);

            connection.Write(bytesToSend, 0, length);
        }

        private void SendBytes(byte[] bytes, int length)
        {
            connection.Write(bytes, 0, length);
        }

        private void Connection_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!isClosing) // Don't continue if in the process of closing the connection
            {
                #region Data Processing Logic
                int bufferSize = connection.BytesToRead;
                if (bufferSize == 0)
                    return;

                char[] s = new char[bufferSize];
                for (short i = 0; i < s.Length; i++)
                    s[i] = (char)connection.ReadByte();
                #endregion

                #region Mode Logic
                char mode;
                ushort lineNum;
                if (s.Length == 1)
                {
                    if (char.IsNumber(s[0])) // Number only
                    {
                        try
                        {
                            mode = serialBuffer;
                            lineNum = ushort.Parse(s[0].ToString());
                        }
                        catch { return; }
                    }
                    else // Letter only
                    {
                        try
                        {
                            mode = s[0];    // Mode is the first character from the serial string
                            serialBuffer = mode;
                            lineNum = 255;
                        }
                        catch { return; }
                    }
                }
                else
                {
                    try
                    {
                        mode = s[0];    // Mode is the first character in the serial string
                        if (char.IsNumber(s[1]))
                            lineNum = ushort.Parse(s[1].ToString());
                        else
                            lineNum = 255;
                    }
                    catch { return; }
                }
                #endregion

                bool powerOn = CheckLight(HsiBits.Flying); // Indicates the game is in the 3D world

                switch (mode)
                {
                    case 'R':   // Received RDY call from Arduino
                        connection.DiscardInBuffer();
                        SendLine("G", 1);
                        connection.DiscardOutBuffer();
                        break;
                    case 'd':   // DED Request
                        serialBuffer = mode;
                        if (lineNum == 255)
                            break;
                        if (powerOn)
                        {
                            if (FlightData.DEDLines != null)
                                SendBytes(NormalizeLine(FlightData.DEDLines[lineNum], FlightData.Invert[lineNum]), 24);
                            else
                                SendLine(" ".PadRight(24, ' '), 24);
                        }
                        else
                        {
                            if (lineNum == 2)
                            {
                                //SendLine("FALCON NOT READY...".PadRight(24, ' '), 24);
                                SendLine(".".PadLeft(dedLoopCount, ' ').PadRight(24, ' '), 24);
                                if (++dedLoopCount > 24)
                                    dedLoopCount = 0;
                            }
                            else
                                SendLine(" ".PadRight(24, ' '), 24);
                        }
                        break;
                    case 'p':       // PFD Request
                        serialBuffer = mode;
                        if (lineNum == 255)
                            break;
                        if (powerOn)
                        {
                            if (FlightData.PFLLines != null)
                                SendBytes(NormalizeLine(FlightData.PFLLines[lineNum], FlightData.PFLInvert[lineNum]), 24);
                            else
                                SendLine(" ".PadRight(24, ' '), 24);
                        }
                        else
                        {
                            if (lineNum == 2)
                            {
                                //SendLine("FALCON NOT READY...".PadRight(24, ' '), 24);
                                SendLine("*".PadLeft(pfdLoopCount, ' ').PadRight(24, ' '), 24);
                                if (++pfdLoopCount > 24)
                                    pfdLoopCount = 0;
                            }
                            else
                                SendLine(" ".PadRight(24, ' '), 24);
                        }
                        break;
                    case 'M':       // CMDS Request
                        serialBuffer = mode;
                        if (lineNum == 255)
                            break;
                        if (powerOn)
                        {
                            SendLine(CMDSMakeLine((short)lineNum).PadRight(24, ' '), 24);
                        }
                        else
                        {
                            if (lineNum == 0)
                            {
                                //SendLine("FALCON NOT READY...".PadRight(24, ' '), 24);
                                SendLine("*".PadLeft(cmdsLoopCount, ' ').PadRight(24, ' '), 24);
                                if (++cmdsLoopCount > 24)
                                    cmdsLoopCount = 0;
                            }
                            else
                                SendLine(" ".PadRight(24, ' '), 24);
                        }
                        break;
                    case 'F':       // Fuel Flow Request
                        if (powerOn)
                            SendLine(FuelFlowConvert(FlightData.fuelFlow).PadLeft(5, '0'), 5);
                        else
                            SendLine("0".PadRight(5, '0'), 5);
                        break;
                    case 'S':       // Speedbrake Request
                        if (powerOn)
                            SendBytes(MakeSpeedBrake(), 1);
                        else
                            SendBytes(BitConverter.GetBytes('1'), 1);   // Send INOP
                        break;
                }
            }
        }

        /// <summary>
        /// This function takes two strings, LINE and INV and mashes them into a string that
        /// conforms with the font on the DEDuino display.
        /// Works for DED and PFL
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        private byte[] NormalizeLine(string disp, string inv)
        {
            char[] normLine = new char[26]; // result buffer
            for (short j = 0; j < disp.Length; j++)
            {
                if (inv[j] == 2) // Check if the corresponding position in the INV line is "lit" - indicated by char[2]
                {
                    // IF INVERTED
                    if (char.IsLetter(disp[j])) // if char is letter (always uppercase)
                        normLine[j] = char.ToLower(disp[j]); // lower it, which is the inverted in the custom font
                    else if (disp[j] == 1) // selection arrows
                        normLine[j] = (char)192;
                    else if (disp[j] == 2) // DED "*"
                        normLine[j] = (char)170;
                    else if (disp[j] == 3) // DED "_"
                        normLine[j] = (char)223;
                    else if (disp[j] == '~') // PFD arrow down
                        normLine[j] = (char)252;
                    else if (disp[j] == '^') // degree symbol (doesn't work with +128 for some reason so manuall do it
                        normLine[j] = (char)222;
                    else // for everything else just add 128 to the ASCII value
                        normLine[j] = (char)(disp[j] + 128);
                }
                else
                {
                    // NOT INVERTED
                    if(disp[j] == 1) // selector double arrow
                        normLine[j] = '@';
                    else if (disp[j] == 2) // DED "*"
                        normLine[j] = '*';
                    else if (disp[j] == 3) // DED "_"
                        normLine[j] = '_';
                    else if (disp[j] == '~') // PFD arrow down
                        normLine[j] = '|';
                    else
                        normLine[j] = disp[j];
                }
            }

            return Encoding.GetEncoding(1252).GetBytes(normLine, 0, 24);
        }

        private string CMDSMakeLine(short line)
        {
            string cmdsLine = string.Empty;
            if (CheckLight(LightBits2.Go) || CheckLight(LightBits2.NoGo))  // If either flag on the CMDS system is on
            {
                if (line == 0)
                {
                    // NOGO
                    if (CheckLight(LightBits2.NoGo))
                        cmdsLine += "NO GO";
                    else
                        cmdsLine += "".PadLeft(5, ' ');

                    cmdsLine += "".PadLeft(2, ' '); // Space between windows

                    // GO
                    if (CheckLight(LightBits2.Go))
                        cmdsLine += "GO";
                    else
                        cmdsLine += " ".PadLeft(2, ' ');

                    cmdsLine += " ".PadLeft(4, ' '); // space between windows

                    // RDY
                    if (CheckLight(LightBits2.Rdy))
                        cmdsLine += "DISPENSE RDY";
                    else
                        cmdsLine += "".PadLeft(12, ' ');
                }
                else if (line == 1)
                {
                    // AUTO DEGR
                    if (CheckLight(LightBits2.Degr))
                        cmdsLine += "AUTO DEGR";
                    else
                        cmdsLine += " ".PadLeft(9, ' ');

                    cmdsLine += " ".PadLeft(3, ' '); // space between windows

                    // CHAFF LO
                    if (CheckLight(LightBits2.ChaffLo))
                        cmdsLine += "LO";
                    else
                        cmdsLine += " ".PadLeft(2, ' ');

                    // CHAFF LOGIC
                    if (FlightData.ChaffCount > 0) // Chaff on board
                        cmdsLine += FlightData.ChaffCount.ToString().PadLeft(3, ' '); // chaff count
                    else if (FlightData.ChaffCount <= 0) // count of -1 == "out"
                        cmdsLine += "0".PadLeft(3, ' '); // chaff count 0
                    else // system is off or something
                        cmdsLine += " ".PadLeft(3, ' ');

                    cmdsLine += "".PadLeft(1, ' '); // space between windows

                    // FLARES LO
                    if (CheckLight(LightBits2.FlareLo))
                        cmdsLine += "LO";
                    else
                        cmdsLine += " ".PadLeft(2, ' ');

                    // FLARE LOGIC
                    if (FlightData.FlareCount > 0) // Flares on board
                        cmdsLine += FlightData.FlareCount.ToString().PadLeft(3, ' '); // flare count
                    else if (FlightData.FlareCount <= 0) // count of -1 == "out"
                        cmdsLine += "0".PadLeft(3, ' ');
                    else
                        cmdsLine += " ".PadLeft(3, ' ');
                }
            }
            else
            {
                // system is off, send blank line
                cmdsLine = "".PadLeft(24, ' ');
            }

            return cmdsLine;
        }

        /// <summary>
        /// Returns speedbrake indicator status
        /// 0 - CLOSED
        /// 1 - INOP
        /// 2 - OPEN
        /// </summary>
        /// <returns></returns>
        private byte[] MakeSpeedBrake()
        {
            byte[] result = new byte[1];
            if (!CheckLight(PowerBits.BusPowerEmergency))
                result[0] = 1;
            else if (CheckLight(LightBits3.SpeedBrake) && (FlightData.speedBrake > 0.0)) // OPEN
                result[0] = 2;
            else // if not INOP and not OPEN, assume CLOSED
                result[0] = 0;

            return result;
        }

        private string FuelFlowConvert(float fuelFlow)
        {
            return (Math.Round(Convert.ToDecimal(fuelFlow) / 10) * 10).ToString();
        }

        #region CheckLight
        private bool CheckLight(LightBits mask)
        {
            if ((FlightData.lightBits & (int)mask) == (int)mask)
                return true;
            else
                return false;
        }

        private bool CheckLight(F4SharedMem.Headers.LightBits2 datamask)
        {
            if ((FlightData.lightBits2 & (int)datamask) == (int)datamask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckLight(F4SharedMem.Headers.LightBits3 datamask)
        {
            if ((FlightData.lightBits3 & (int)datamask) == (int)datamask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckLight(F4SharedMem.Headers.HsiBits datamask)
        {
            if ((FlightData.hsiBits & (int)datamask) == (int)datamask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckLight(F4SharedMem.Headers.PowerBits datamask)
        {
            if ((FlightData.powerBits & (int)datamask) == (int)datamask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
