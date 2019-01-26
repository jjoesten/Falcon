using F4SharedMem;
using F4SharedMem.Headers;
using SimplifiedCommon.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Aesalon
{
    internal class FalconConnector
    {
        #region Singleton
        // TODO: Rewrite Singleton Property
        private static FalconConnector singleton;
        public static FalconConnector Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new FalconConnector();
                return singleton;
            }
        }

        #endregion

        #region Construction

        public FalconConnector()
        {
            LightList = new List<FalconLight>();
            InitLightList();

            GaugeList = new List<FalconGauge>();
            InitGaugeList();

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = detectFalconTimerInterval;
        }

        #endregion

        #region ReadFalconDataTimerInterval
        private TimeSpan readFalconDataTimerInterval = TimeSpan.FromMilliseconds(100);
        public TimeSpan ReadFalconDataTimerInterval
        {
            get { return readFalconDataTimerInterval; }
            set
            {
                if (readFalconDataTimerInterval == value)
                    return;

                if (value.TotalMilliseconds < 0 || value.TotalMilliseconds > Int32.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(ReadFalconDataTimerInterval));

                readFalconDataTimerInterval = value;

                if (reader != null)
                    timer.Interval = readFalconDataTimerInterval;
            }
        }
        #endregion

        #region Timer

        private DispatcherTimer timer;
        private readonly TimeSpan detectFalconTimerInterval = TimeSpan.FromSeconds(5);
        private Reader reader;
        private FlightData oldFlightData;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (reader == null)
            {
                if (GetFalconWindowHandle() != IntPtr.Zero)
                {
                    reader = new Reader();
                    if (!reader.IsFalconRunning)
                    {
                        reader.Dispose();
                        reader = null;
                    }
                    else
                    {
                        // Falcon Started
                        RaiseFalconStarted();
                        timer.Interval = readFalconDataTimerInterval;
                    }
                }
            }
            else
            {
                if (GetFalconWindowHandle() == IntPtr.Zero)
                {
                    // Falcon Stopped
                    reader.Dispose();
                    reader = null;
                    RaiseFalconStarted();
                    timer.Interval = detectFalconTimerInterval;
                }
            }

            FlightData newFlightData = null;

            if (reader != null)
                newFlightData = reader.GetCurrentData();

            if (newFlightData == null)
            {
                if (oldFlightData != null)
                {
                    RaiseFlightDataChanged(oldFlightData, newFlightData);
                    RaiseFlightDataLightsChanged(oldFlightData, newFlightData);
                }
            }
            else
            {
                if (oldFlightData == null)
                {
                    RaiseFlightDataChanged(oldFlightData, newFlightData);
                    RaiseFlightDataLightsChanged(oldFlightData, newFlightData);
                }
                else
                {
                    RaiseFlightDataChanged(oldFlightData, newFlightData);

                    bool lightChanged = (oldFlightData.lightBits ^ newFlightData.lightBits) != 0;
                    lightChanged |= (oldFlightData.lightBits2 ^ newFlightData.lightBits2) != 0;
                    lightChanged |= (oldFlightData.lightBits3 ^ newFlightData.lightBits3) != 0;
                    lightChanged |= (oldFlightData.hsiBits ^ newFlightData.hsiBits) != 0;
                    lightChanged |= oldFlightData.speedBrake != newFlightData.speedBrake;
                    lightChanged |= oldFlightData.rpm != newFlightData.rpm;
                    lightChanged |= (oldFlightData.blinkBits ^ newFlightData.blinkBits) != 0;
                    lightChanged |= (oldFlightData.powerBits ^ newFlightData.powerBits) != 0;
                    lightChanged |= oldFlightData.tacanInfo != newFlightData.tacanInfo;
                    lightChanged |= oldFlightData.navMode != newFlightData.navMode;
                    lightChanged |= (oldFlightData.cmdsMode ^ newFlightData.cmdsMode) != 0;
                    lightChanged |= (oldFlightData.altBits ^ newFlightData.altBits) != 0;

                    if (lightChanged)
                        RaiseFlightDataLightsChanged(oldFlightData, newFlightData);
                }
            }

            oldFlightData = newFlightData;
        }

        #endregion

        #region Events

        public event EventHandler FalconStarted;
        private void RaiseFalconStarted() => FalconStarted?.Invoke(this, EventArgs.Empty);

        public event EventHandler FalconStopped;
        private void RaiseFalconStopped() => FalconStopped?.Invoke(this, EventArgs.Empty);

        public event EventHandler<FlightDataChangedEventArgs> FlightDataChanged;
        private void RaiseFlightDataChanged(FlightData oldFlightData, FlightData newFlightData)
            => FlightDataChanged?.Invoke(this, new FlightDataChangedEventArgs(oldFlightData, newFlightData));

        public event EventHandler<FlightDataChangedEventArgs> FlightDataLightsChanged;
        private void RaiseFlightDataLightsChanged(FlightData oldFlightData, FlightData newFlightData)
            => FlightDataLightsChanged?.Invoke(this, new FlightDataChangedEventArgs(oldFlightData, newFlightData));

        #endregion

        #region Static Falcon Functions
        public static IntPtr GetFalconWindowHandle()
        {
            IntPtr hWnd = NativeMethods.FindWindow("FalconDisplay", null);
            return hWnd;
        }
        #endregion

        #region Public Methods

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        #endregion

        #region Panels

        const string aoaIndexer = "HUD: AOA Indexer";
        const string airRefuel = "HUD: Air Refuel";
        const string leftEyebrow = "L. WARNING LIGHTS";
        const string rightEyebrow = "R. WARNING LIGHTS";
        const string threatWarningPrime = "TWP";
        const string miscArmament = "MISC";
        const string landingGear = "LANDING GEAR";
        const string centerConsole = "CENTER CONSOLE";
        const string threatWarningAux = "TWA";
        const string chaffFlare = "CMDS";
        const string cautionLightPanel = "CAUTION";
        const string enginePanel = "ENG & JET START";
        const string epuPanel = "EPU";
        const string elecPanel = "ELEC";
        const string avtrPanel = "AVTR";
        const string flightControlPanel = "FLT CONTROL";
        const string testPanel = "TEST";
        const string auxCommPanel = "AUX COMM";
        const string others = "Others";

        const string leftAux = "L. AUX CONSOLE";
        const string powerDistribution = "POWER DISTRIBUTION";
        const string realMagSwitches = "REAL MAGNETIC SWITCHES";

        #endregion // Panels

        #region LightList

        public List<FalconLight> LightList { get; private set; }
        private void InitLightList()
        {
            AddToLightList(new FalconLightBit1(leftEyebrow, "MASTER CAUTION", LightBits.MasterCaution));
            AddToLightList(new FalconLightBit1(leftEyebrow, "TF-FAIL", LightBits.TF));
            AddToLightList(new FalconLightBit1(rightEyebrow, "OXY LOW (R. Warning)", LightBits.OXY_BROW));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "EQUIP HOT", LightBits.EQUIP_HOT));
            AddToLightList(new FalconLightBit1(rightEyebrow, "ENG FIRE", LightBits.ENG_FIRE));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "STORES CONFIG", LightBits.CONFIG));
            AddToLightList(new FalconLightBit1(rightEyebrow, "HYD/OIL PRESS", LightBits.HYD));
            AddToLightList(new FalconLightBit1(testPanel, "FLCS TEST", LightBits.Flcs_ABCD));
            AddToLightList(new FalconLightBit1(rightEyebrow, "FLCS/DUAL", LightBits.FLCS));
            AddToLightList(new FalconLightBit1(rightEyebrow, "CANOPY", LightBits.CAN));
            AddToLightList(new FalconLightBit1(rightEyebrow, "TO/LDG CONFIG", LightBits.T_L_CFG));
            AddToLightList(new FalconLightBit1(aoaIndexer, "AOA Above", LightBits.AOAAbove));
            AddToLightList(new FalconLightBit1(aoaIndexer, "AOA On", LightBits.AOAOn));
            AddToLightList(new FalconLightBit1(aoaIndexer, "AOA Below", LightBits.AOABelow));
            AddToLightList(new FalconLightBit1(airRefuel, "RDY", LightBits.RefuelRDY));
            AddToLightList(new FalconLightBit1(airRefuel, "AR/NWS", LightBits.RefuelAR));
            AddToLightList(new FalconLightBit1(airRefuel, "DISC", LightBits.RefuelDSC));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "FLCS FAULT", LightBits.FltControlSys));
            AddToLightList(new FalconLightBit1(others, "LE FLAPS", LightBits.LEFlaps));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "ENGINE FAULT", LightBits.EngineFault));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "OVERHEAT", LightBits.Overheat));
            AddToLightList(new FalconLightBit1(others, "FUEL LOW", LightBits.FuelLow));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "AVIONICS FAULT", LightBits.Avionics));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "RADAR ALT", LightBits.RadarAlt));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "IFF", LightBits.IFF));
            AddToLightList(new FalconLightBit1(others, "ECM", LightBits.ECM));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "HOOK", LightBits.Hook));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "NWS FAIL", LightBits.NWSFail));
            AddToLightList(new FalconLightBit1(cautionLightPanel, "CABIN PRESS", LightBits.CabinPress));
            AddToLightList(new FalconLightBit1(miscArmament, "TFR STBY", LightBits.TFR_STBY));
            AddToLightList(new FalconLightBit2(threatWarningPrime, "HANDOFF", LightBits2.HandOff));
            AddToLightList(new FalconLightBit2(threatWarningPrime, "NAVAL", LightBits2.Naval));
            AddToLightList(new FalconLightBit2(threatWarningPrime, "TGT SEP", LightBits2.TgtSep));
            AddToLightList(new FalconLightBit2(chaffFlare, "GO", LightBits2.Go));
            AddToLightList(new FalconLightBit2(chaffFlare, "NO GO", LightBits2.NoGo));
            AddToLightList(new FalconLightBit2(chaffFlare, "AUTO DEGR", LightBits2.Degr));
            AddToLightList(new FalconLightBit2(chaffFlare, "DISPENSE RDY", LightBits2.Rdy));
            AddToLightList(new FalconLightBit2(chaffFlare, "BINGO CHAFF", LightBits2.ChaffLo));
            AddToLightList(new FalconLightBit2(chaffFlare, "BINGO FLARE", LightBits2.FlareLo));

            //
            // NEW BLINKING LAMPS
            //
            AddToLightList(new FalconBlinkingLamp(threatWarningPrime, "LAUNCH",
                flightData => (flightData.lightBits2 & (int)LightBits2.Launch) != 0, BlinkBits.Launch, 260));
            AddToLightList(new FalconBlinkingLamp(threatWarningAux, "SEARCH",
                flightData => (flightData.lightBits2 & (int)LightBits2.AuxSrch) != 0, BlinkBits.AuxSrch, 250));
            AddToLightList(new FalconBlinkingLamp(threatWarningPrime, "PRIORITY",
                flightData => (flightData.lightBits2 & (int)LightBits2.PriMode) != 0, BlinkBits.PriMode, 250));
            AddToLightList(new FalconBlinkingLamp(threatWarningPrime, "UNKNOWN",
                flightData => (flightData.lightBits2 & (int)LightBits2.Unk) != 0, BlinkBits.Unk, 250));
            AddToLightList(new FalconBlinkingLamp(centerConsole, "OUTER MARKER",
                flightData => (flightData.hsiBits & (int)HsiBits.OuterMarker) != 0, BlinkBits.OuterMarker, 500));
            AddToLightList(new FalconBlinkingLamp(centerConsole, "MIDDLE MARKER",
                flightData => (flightData.hsiBits & (int)HsiBits.MiddleMarker) != 0, BlinkBits.MiddleMarker, 250));
            AddToLightList(new FalconBlinkingLamp(cautionLightPanel, "PROBE HEAT",
                flightData => (flightData.lightBits2 & (int)LightBits2.PROBEHEAT) != 0, BlinkBits.PROBEHEAT, 100));

            AddToLightList(new FalconTWPOpenLight(threatWarningPrime, "OPEN", flightData => (flightData.lightBits2 & (int)LightBits2.PriMode) != 0));
            AddToLightList(new FalconLightBit2(threatWarningAux, "ACTIVITY", LightBits2.AuxAct));
            AddToLightList(new FalconLightBit2(threatWarningAux, "LOW ALTITUDE", LightBits2.AuxLow));
            AddToLightList(new FalconLightBit2(threatWarningAux, "SYSTEM POWER", LightBits2.AuxPwr));
            AddToLightList(new FalconLightBit2(miscArmament, "ECM PWR", LightBits2.EcmPwr));
            AddToLightList(new FalconLightBit2(miscArmament, "ECM FAIL", LightBits2.EcmFail));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "FWD FUEL LOW", LightBits2.FwdFuelLow));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "AFT FUEL LOW", LightBits2.AftFuelLow));
            AddToLightList(new FalconLightBit2(epuPanel, "EPU ON", LightBits2.EPUOn));
            AddToLightList(new FalconLightBit2(enginePanel, "JFS RUN", LightBits2.JFSOn));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "SEC", LightBits2.SEC));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "OXY LOW (Caution)", LightBits2.OXY_LOW));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "SEAT ARM", LightBits2.SEAT_ARM));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "BUC", LightBits2.BUC));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "FUEL OIL HOT", LightBits2.FUEL_OIL_HOT));
            AddToLightList(new FalconLightBit2(cautionLightPanel, "ANTI SKID", LightBits2.ANTI_SKID));
            AddToLightList(new FalconLightBit2(miscArmament, "TFR ENGAGED", LightBits2.TFR_ENGAGED));
            AddToLightList(new FalconLightBit2(landingGear, "GEAR HANDLE", LightBits2.GEARHANDLE));
            AddToLightList(new FalconLightBit2(rightEyebrow, "ENGINE", LightBits2.ENGINE));
            AddToLightList(new FalconLightBit3(elecPanel, "FLCS PMG", LightBits3.FlcsPmg));
            AddToLightList(new FalconLightBit3(elecPanel, "MAIN GEN", LightBits3.MainGen));
            AddToLightList(new FalconLightBit3(elecPanel, "STBY GEN", LightBits3.StbyGen));
            AddToLightList(new FalconLightBit3(elecPanel, "EPU GEN", LightBits3.EpuGen));
            AddToLightList(new FalconLightBit3(elecPanel, "EPU PMG", LightBits3.EpuPmg));
            AddToLightList(new FalconLightBit3(elecPanel, "TO FLCS", LightBits3.ToFlcs));
            AddToLightList(new FalconLightBit3(elecPanel, "FLCS RLY", LightBits3.FlcsRly));
            AddToLightList(new FalconLightBit3(elecPanel, "BAT FAIL", LightBits3.BatFail));
            AddToLightList(new FalconLightBit3(epuPanel, "HYDRAZINE", LightBits3.Hydrazine));
            AddToLightList(new FalconLightBit3(epuPanel, "AIR", LightBits3.Air));
            AddToLightList(new FalconLightBit3(cautionLightPanel, "ELEC SYS", LightBits3.Elec_Fault));
            AddToLightList(new FalconLightBit3(others, "LEF FAULT", LightBits3.Lef_Fault));

            AddToLightList(new FalconLightBit3(powerDistribution, "POWER OFF", LightBits3.Power_Off));
            AddToLightList(new FalconLightBit3(landingGear, "SPEEDBRAKE", LightBits3.SpeedBrake));
            AddToLightList(new FalconLightBit3(threatWarningPrime, "SYS TEST", LightBits3.SysTest));
            AddToLightList(new FalconLightBit3(leftEyebrow, "MASTER CAUTION ANNOUNCED", LightBits3.MCAnnounced));
            AddToLightList(new FalconLightBit3(others, "MAIN GEAR WOW", LightBits3.MLGWOW));
            AddToLightList(new FalconLightBit3(others, "NOSE GEAR WOW", LightBits3.NLGWOW));
            AddToLightList(new FalconLightBit3(others, "ATF NOT ENGAGED", LightBits3.ATF_Not_Engaged));
            AddToLightList(new FalconHsiBits(centerConsole, "HSI TO FLAG", HsiBits.ToTrue));
            AddToLightList(new FalconHsiBits(centerConsole, "HSI FROM FLAG", HsiBits.FromTrue));
            AddToLightList(new FalconHsiBits(centerConsole, "HSI ILS WARN FLAG", HsiBits.IlsWarning));
            AddToLightList(new FalconHsiBits(centerConsole, "HSI CRS WARN FLAG", HsiBits.CourseWarning));
            AddToLightList(new FalconHsiBits(centerConsole, "HSI OFF FLAG", HsiBits.HSI_OFF));
            AddToLightList(new FalconHsiBits(centerConsole, "ADI OFF FLAG", HsiBits.ADI_OFF));
            AddToLightList(new FalconHsiBits(centerConsole, "ADI AUX FLAG", HsiBits.ADI_AUX));
            AddToLightList(new FalconHsiBits(centerConsole, "ADI GS FLAG", HsiBits.ADI_GS));
            AddToLightList(new FalconHsiBits(centerConsole, "ADI LOC FLAG", HsiBits.ADI_LOC));
            AddToLightList(new FalconHsiBits(centerConsole, "BUP ADI OFF FLAG", HsiBits.BUP_ADI_OFF));
            AddToLightList(new FalconHsiBits(centerConsole, "VVI OFF FLAG", HsiBits.VVI));
            AddToLightList(new FalconHsiBits(centerConsole, "AOA OFF FLAG", HsiBits.AOA));
            AddToLightList(new FalconHsiBits(others, "IN THE PIT", HsiBits.Flying));
            AddToLightList(new FalconTacanBits(auxCommPanel, "AUX TACAN BAND X", TacanBits.band, (int)TacanSources.AUX));
            AddToLightList(new FalconTacanBits(auxCommPanel, "AUX TACAN A-A", TacanBits.mode, (int)TacanSources.AUX));
            AddToLightList(new FalconTacanBits(auxCommPanel, "UFC TACAN BAND X", TacanBits.band, (int)TacanSources.UFC));
            AddToLightList(new FalconTacanBits(auxCommPanel, "UFC TACAN A-A", TacanBits.mode, (int)TacanSources.UFC));
            AddToLightList(new FalconDataBits(centerConsole, "ALTIMETER in Hg", flightdata => (flightdata.altBits & (int)AltBits.CalType) == 1));
            AddToLightList(new FalconDataBits(centerConsole, "ALT PNEU FLAG", flightdata => (flightdata.altBits & (int)AltBits.PneuFlag) == 1));
            AddToLightList(new FalconDataBits(centerConsole, "NAV MODE TACAN", flightdata => (flightdata.navMode == (byte)NavModes.TACAN)));
            AddToLightList(new FalconDataBits(centerConsole, "NAV MODE NAV", flightdata => (flightdata.navMode == (byte)NavModes.NAV)));
            AddToLightList(new FalconDataBits(centerConsole, "NAV MODE ILS-TACAN", flightdata => (flightdata.navMode == (byte)NavModes.ILS_TACAN)));
            AddToLightList(new FalconDataBits(centerConsole, "NAV MODE ILS-NAV", flightdata => (flightdata.navMode == (byte)NavModes.ILS_NAV)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE OFF ", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsOFF)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE STBY", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsSTBY)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE MAN", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsMAN)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE AUTO", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsAUTO)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE BYP", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsBYP)));
            AddToLightList(new FalconDataBits(chaffFlare, "CMDS MODE SEMI", flightdata => (flightdata.cmdsMode == (int)CmdsModes.CmdsSEMI)));


            AddToLightList(new FalconLightBit3(flightControlPanel, "FLCS BIT RUN", LightBits3.FlcsBitRun));
            AddToLightList(new FalconLightBit3(flightControlPanel, "FLCS BIT FAIL", LightBits3.FlcsBitFail));
            AddToLightList(new FalconLightBit3(rightEyebrow, "DBU ON", LightBits3.DbuWarn));
            AddToLightList(new FalconLightBit3(cautionLightPanel, "C ADC", LightBits3.cadc));

            //
            // Real Mag Switch Enhancements
            //
            AddToLightList(new FalconLightBit1(realMagSwitches, "AUTOPILOT REAL MAG", LightBits.AutoPilotOn));
            AddToLightList(new FalconLightBit3(realMagSwitches, "PARKING BRAKE REAL MAG", LightBits3.ParkBrakeOn));
            AddToLightList(new FalconLightBit3(realMagSwitches, "FLCS BIT REAL MAG", LightBits3.FlcsBitRun));
            AddToLightList(new FalconPowerBits(realMagSwitches, "JFS REAL MAG", PowerBits.JetFuelStarter));


            AddToLightList(new FalconHsiBits(avtrPanel, "AVTR", HsiBits.AVTR));

            //
            // Combined OM/MM Marker Beacon Lamp
            // - Removed because it doesn't support steady On state.  Use OM and MM Lamps wired in parallel to MRK Lamp.
            //
            //AddToLightList(new FalconMarkerBeacon(centerConsole, "MARKER BEACON", 
            //    flightdata => ((flightdata.hsiBits & (int)HsiBits.OuterMarker) != 0) || (flightdata.hsiBits & (int)HsiBits.MiddleMarker) != 0));

            //
            // Power Bus Enhancements to control Power Bus Relays
            //
            AddToLightList(new FalconPowerBits(powerDistribution, "BATTERY BUS", PowerBits.BusPowerBattery));
            AddToLightList(new FalconPowerBits(powerDistribution, "EMERGENCY BUS", PowerBits.BusPowerEmergency));
            AddToLightList(new FalconPowerBits(powerDistribution, "ESSENTIAL BUS", PowerBits.BusPowerEssential));
            AddToLightList(new FalconPowerBits(powerDistribution, "NON-ESSENTIAL BUS", PowerBits.BusPowerNonEssential));
            AddToLightList(new FalconPowerBits(powerDistribution, "MAIN GENERATOR", PowerBits.MainGenerator));
            AddToLightList(new FalconPowerBits(powerDistribution, "STBY GENERATOR", PowerBits.StandbyGenerator));
            AddToLightList(new FalconRPMOver65Percent(others, "RPM > 65%", flightdata => flightdata.rpm > 65.0F));


            AddToLightList(new FalconLightNoseGearDown(landingGear, "NOSE GEAR DOWN"));
            AddToLightList(new FalconLightLeftGearDown(landingGear, "LEFT GEAR DOWN"));
            AddToLightList(new FalconLightRightGearDown(landingGear, "RIGHT GEAR DOWN"));
            AddToLightList(new FalconLightSpeedBrake(landingGear, "SPEEDBRAKE > 0%", 0.0f / 3.0f));
            AddToLightList(new FalconLightSpeedBrake(landingGear, "SPEEDBRAKE > 33%", 1.0f / 3.0f));
            AddToLightList(new FalconLightSpeedBrake(landingGear, "SPEEDBRAKE > 66%", 2.0f / 3.0f));

            //
            // Support for Homebuilt Custom Mag Switches that need a pulse to reset them to the off position.  Michael
            //
            AddToLightList(new FalconMagneticSwitchReset(flightControlPanel, "FLCS BIT DIY MAG SW RESET",
                flightData => (flightData.lightBits3 & (int)LightBits3.FlcsBitRun) == 0));
            AddToLightList(new FalconMagneticSwitchReset(enginePanel, "JFS DIY MAG SW RESET",
                flightData => (flightData.lightBits2 & (int)LightBits2.JFSOn) == 0));
            AddToLightList(new FalconMagneticSwitchReset(landingGear, "PARK BRAKE DIY MAG SW RESET",
                flightData => flightData.rpm > 87.0F));
            AddToLightList(new FalconMagneticSwitchReset(miscArmament, "AUTOPILOT DIY MAG SW RESET",
                flightData => (flightData.lightBits & (int)LightBits.AutoPilotOn) == 0));

            //
            // Eric's ON-GROUND Enhancement 
            //
            AddToLightList(new FalconGearHandleSol(landingGear, "ON-GROUND", flightData => (flightData.lightBits & (int)LightBits.ONGROUND) != 0x10));
            //
            // Eric's SpeedBrake Indicator Enhancement 
            // - Allows SpeedBrake Indicator to be tri-state; OPEN, CLOSED or Barber-Pole
            //   (Barber-Pole = !OPEN && !CLOSED)
            //
            AddToLightList(new RealSpeedBrakeClosed(leftAux, "SPEEDBRAKE CLOSED", flightData => flightData.speedBrake == 0));
            AddToLightList(new RealSpeedBrakeOpen(leftAux, "SPEEDBRAKE OPEN", flightData => flightData.speedBrake > 0));


        }
        private void AddToLightList(FalconLight falconLight)
        {
            if (LightList.Find(item => item.Label == falconLight.Label) != null)
                throw new ArgumentException("Aesalon.FalconConnector.LightList already contains an item labeled " + falconLight.Label);
            else
                LightList.Add(falconLight);
        }

        #endregion // LightList

        #region GaugeList

        public List<FalconGauge> GaugeList { get; private set; }
        private void InitGaugeList()
        {
            AddToGaugeList(new FalconGauge("AOA", flightData => flightData.alpha, -32.0F, 32.0F, 4, 0, 1));
            AddToGaugeList(new FalconGauge("MACH", flightData => flightData.mach, 0.0F, 2.0F, 3, 0, 2));
            AddToGaugeList(new FalconGauge("KIAS", flightData => flightData.kias, 0.0F, 1000.0F, 4, 0, 0));
            AddToGaugeList(new FalconGauge("VVI", flightData => -flightData.zDot, -100.0F, 100.0F, 4, 0, 0));
            AddToGaugeList(new FalconGauge("NOZ POS", flightData => flightData.nozzlePos, 0.0F, 1.0F, 3, 0, 2));
            AddToGaugeList(new FalconGauge("NOZ POS 2", flightData => flightData.nozzlePos2, 0.0F, 1.0F, 3, 0, 2));
            AddToGaugeList(new FalconGauge("RPM", flightData => flightData.rpm, 0.0F, 103.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("RPM 2", flightData => flightData.rpm2, 0.0F, 103.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("FTIT", flightData => flightData.ftit, 0.0F, 12.0F, 3, 0, 1));
            AddToGaugeList(new FalconGauge("FTIT 2", flightData => flightData.ftit2, 0.0F, 12.0F, 3, 0, 1));
            AddToGaugeList(new FalconGauge("SPEEDBRAKE", flightData => flightData.speedBrake, 0.0F, 1.0F, 3, 0, 2));
            AddToGaugeList(new FalconGauge("EPU FUEL", flightData => flightData.epuFuel, 0.0F, 100.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("OIL PRESSURE", flightData => flightData.oilPressure, 0.0F, 100.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("OIL PRESSURE 2", flightData => flightData.oilPressure2, 0.0F, 100.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("CHAFF COUNT", flightData => flightData.ChaffCount >= 0.0F ? flightData.ChaffCount : 0.0F, 0.0F, 180.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("FLARE COUNT", flightData => flightData.FlareCount >= 0.0F ? flightData.FlareCount : 0.0F, 0.0F, 30.0F, 2, 0, 0));
            AddToGaugeList(new FalconGauge("TRIM PITCH", flightData => flightData.TrimPitch, -0.5F, 0.5F, 4, 0, 2));
            AddToGaugeList(new FalconGauge("TRIM ROLL", flightData => flightData.TrimRoll, -0.5F, 0.5F, 4, 0, 2));
            AddToGaugeList(new FalconGauge("TRIM YAW", flightData => flightData.TrimYaw, -0.5F, 0.5F, 4, 0, 2));
            AddToGaugeList(new FalconGauge("FUEL F/R", flightData => flightData.fwd, 0.0F, 40000.0F, 5, 0, 0));
            AddToGaugeList(new FalconGauge("FUEL A/L", flightData => flightData.aft, 0.0F, 40000.0F, 5, 0, 0));
            AddToGaugeList(new FalconGauge("FUEL TOTAL", flightData => flightData.total, 0.0F, 20000.0F, 5, 0, 0));
            AddToGaugeList(new FalconGauge("FUEL FLOW", flightData => flightData.fuelFlow, 0.0F, 80000.0F, 5, 5, 0));
            AddToGaugeList(new FalconGauge("ALT", flightData => -flightData.aauz, 0.0F, 60000.0F, 5, 0, 0));
            AddToGaugeList(new FalconGauge("CURRENT HEADING", flightData => flightData.currentHeading, 0.0F, 360.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("HSI COURSE", flightData => flightData.desiredCourse, 0.0F, 360.0F, 3, 3, 0));
            AddToGaugeList(new FalconGauge("HSI MILES", flightData => flightData.distanceToBeacon, 0.0F, 999.0F, 3, 3, 0));

            //
            // Added missing F4Shared Memory Gauges - Beau
            //
            AddToGaugeList(new FalconGauge("ALT BARO", flightData => (float)flightData.AltCalReading / 100, 27.00F, 31.99F, 4, 2, 2));
            AddToGaugeList(new FalconGauge("UHF CHANNEL", flightData => (float)flightData.BupUhfPreset, 0.0F, 20.0F, 2, 2, 0));
            AddToGaugeList(new FalconGauge("UHF FREQ", flightData => (float)flightData.BupUhfFreq / 1000, 225.000F, 399.999F, 6, 3, 3));
            AddToGaugeList(new FalconGauge("UFC TACAN CHANNEL", flightData => flightData.UFCTChan, 0.0F, 199.0F, 3, 3, 0));
            AddToGaugeList(new FalconGauge("AUX TACAN CHANNEL", flightData => flightData.AUXTChan, 0.0F, 199.0F, 3, 3, 0));
            AddToGaugeList(new FalconGauge("HYD PRESS A", flightData => flightData.hydPressureA, 0.0F, 9999.0F, 4, 0, 0));
            AddToGaugeList(new FalconGauge("HYD PRESS B", flightData => flightData.hydPressureA, 0.0F, 9999.0F, 4, 0, 0));
            AddToGaugeList(new FalconGauge("DESIRED HEADING", flightData => flightData.desiredHeading, 0.0F, 360.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("BEARING TO BEACON", flightData => flightData.bearingToBeacon, 0.0F, 360.0F, 3, 0, 0));
            AddToGaugeList(new FalconGauge("CDI", flightData => flightData.courseDeviation, -10.0F, 10.0F, 4, 3, 1));
            AddToGaugeList(new FalconGauge("CABIN ALT", flightData => flightData.cabinAlt, 0.0F, 60000.0F, 5, 0, 0));
        }

        private void AddToGaugeList(FalconGauge falconGauge)
        {
            if (GaugeList.Find(item => item.Label == falconGauge.Label) != null)
                throw new ArgumentException("Aesalon.FalconConnector.GaugeList already contains an item labeled " + falconGauge.Label);
            else
                GaugeList.Add(falconGauge);
        }

        #endregion // GaugeList
    }

    public class FlightDataChangedEventArgs : EventArgs
    {
        public readonly FlightData oldFlightData;
        public readonly FlightData newFlightData;

        public FlightDataChangedEventArgs(FlightData oldFlightData, FlightData newFlightData)
        {
            this.oldFlightData = oldFlightData;
            this.newFlightData = newFlightData;
        }
    }
}