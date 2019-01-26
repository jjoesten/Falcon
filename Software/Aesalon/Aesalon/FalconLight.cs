using F4SharedMem;
using F4SharedMem.Headers;
using System;
using System.Windows.Threading;

namespace Aesalon
{
    #region FalconLight
    public abstract class FalconLight
    {
        public string Label { get; private set; }
        public string Group { get; private set; }

        protected FalconLight(string group, string label)
        {
            Label = label;
            Group = group;
        }

        private EventHandler<FalconLightChangedEventArgs> falconLightChanged;
        private int nbUser = 0;

        public event EventHandler<FalconLightChangedEventArgs> FalconLightChanged
        {
            add
            {
                falconLightChanged += value;

                ++nbUser;
                if (nbUser == 1)
                    FalconConnector.Singleton.FlightDataLightsChanged += OnFlightDataChanged;
            }

            remove
            {
                falconLightChanged -= value;

                --nbUser;
                if (nbUser == 0)
                    FalconConnector.Singleton.FlightDataLightsChanged -= OnFlightDataChanged;
            }
        }

        protected void RaiseFalconLightChanged(bool? oldValue, bool? newValue)
        {
            if (falconLightChanged != null)
                falconLightChanged(this, new FalconLightChangedEventArgs(oldValue, newValue));
        }

        protected abstract void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e);
    }
    #endregion //FalconLight

    #region FalconLightBit

    public abstract class FalconLightBit : FalconLight
    {
        protected readonly int bit;

        protected FalconLightBit(string group, string label, int bit)
            : base(group, label)
        {
            this.bit = bit;
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (oldValue != newValue)
                RaiseFalconLightChanged(oldValue, newValue);
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return GetNonNullValue(flightData);
        }

        protected abstract bool GetNonNullValue(FlightData flightData);
    }

    #endregion // FalconLightBit

    #region FalconLightBit1

    public class FalconLightBit1 : FalconLightBit
    {
        public FalconLightBit1(string group, string label, LightBits bit)
            : base(group, label, (int)bit)
        {
        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.lightBits & bit) != 0;
        }
    }

    #endregion // FalconLightBit1

    #region FalconLightBit2

    public class FalconLightBit2 : FalconLightBit
    {
        public FalconLightBit2(string group, string label, LightBits2 bit)
            : base(group, label, (int)bit)
        {

        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.lightBits2 & bit) != 0;
        }
    }

    #endregion // FalconLightBit2

    #region FalconLightBit3

    public class FalconLightBit3 : FalconLightBit
    {
        public FalconLightBit3(string group, string label, LightBits3 bit)
            : base(group, label, (int)bit)
        { }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.lightBits3 & bit) != 0;
        }
    }

    #endregion // FalconLightBit3

    #region FalconHsiBits

    public class FalconHsiBits : FalconLightBit
    {
        public FalconHsiBits(string group, string label, HsiBits bit)
            : base(group, label, (int)bit)
        {

        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.hsiBits & bit) != 0;
        }
    }

    #endregion // FalconHsiBits

    #region FalconPowerBits

    public class FalconPowerBits : FalconLightBit
    {
        public FalconPowerBits(string group, string label, PowerBits bit)
            : base(group, label, (int)bit)
        {

        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.powerBits & bit) != 0;
        }
    }

    #endregion FalconPowerBits

    #region FalconLightGearDown

    public abstract class FalconLightGearDown : FalconLightBit3
    {
        public FalconLightGearDown(string group, string label, LightBits3 bit)
            : base(group, label, bit)
        {

        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return base.GetNonNullValue(flightData);
        }

        protected abstract bool GetGearDownValue(FlightData flightData);
    }

    #endregion // FalconLightGearDown

    #region FalconLightNoseGearDown

    public class FalconLightNoseGearDown : FalconLightGearDown
    {
        public FalconLightNoseGearDown(string group, string label)
            : base(group, label, LightBits3.NoseGearDown)
        { }

        protected override bool GetGearDownValue(FlightData flightData)
        {
            return flightData.NoseGearPos == 1.0f;
        }
    }

    #endregion // FalconLightNoseGearDown

    #region FalconLightLeftGearDown

    public class FalconLightLeftGearDown : FalconLightGearDown
    {
        public FalconLightLeftGearDown(string group, string label)
            : base(group, label, LightBits3.LeftGearDown)
        {

        }

        protected override bool GetGearDownValue(FlightData flightData)
        {
            return flightData.LeftGearPos == 1.0f;
        }
    }

    #endregion // FalconLightLeftGearDown

    #region FalconLightRightGearDown

    public class FalconLightRightGearDown : FalconLightGearDown
    {
        public FalconLightRightGearDown(string group, string label)
            : base(group, label, LightBits3.RightGearDown)
        {

        }

        protected override bool GetGearDownValue(FlightData flightData)
        {
            return flightData.RightGearPos == 1.0f;
        }
    }

    #endregion // FalconLightRightGearDown

    #region FalconLightSpeedBrake

    public class FalconLightSpeedBrake : FalconLight
    {
        bool emergBusPwr = Convert.ToBoolean(PowerBits.BusPowerEmergency);
        protected readonly float percent;

        public FalconLightSpeedBrake(string group, string label, float percent)
            : base(group, label)
        {
            this.percent = percent;
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (emergBusPwr)
            {
                if (oldValue != newValue)
                {
                    RaiseFalconLightChanged(oldValue, newValue);
                }
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return flightData.speedBrake > percent;
        }
    }

    #endregion // FalconLightSpeedBrake

    // FOr use with homebuild mag switches that need an output to force the switch into the off position.
    #region FalconMagneticSwitchReset : FalconLight

    /// <summary>
    /// For use with homebuild magnetic switches that need an output to force the switch into the off position.
    /// </summary>
    public class FalconMagneticSwitchReset : FalconLight
    {
        private readonly Func<FlightData, bool> getFlightDataValue;
        private DispatcherTimer timer;

        public FalconMagneticSwitchReset(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(200);
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (oldValue != newValue && newValue == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(oldValue, newValue);
                timer.Start();
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            RaiseFalconLightChanged(true, false);
        }
    }

    #endregion // FalconMagneticSwitchReset

    // Provides functionality for Lights than can blink
    #region FalconBlinkingLamp

    /// <summary>
    /// Provides functionality for Lights that can both be on steady or blink depending on <see cref="F4SharedMem.Headers.BlinkBits"/>
    /// </summary>
    public class FalconBlinkingLamp : FalconLight
    {
        private bool lampLit = false;
        private bool newBlinkingBitSet = false;
        private bool oldBlinkingBitSet = false;
        private bool lightBitSet = false;

        private DispatcherTimer timer;

        private readonly Func<FlightData, bool> getFlightDataValue;
        private readonly BlinkBits blinkBit;

        public FalconBlinkingLamp(string group, string label, Func<FlightData, bool> getFlightDataValue, BlinkBits blinkBits, int rate)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;
            this.blinkBit = blinkBits;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(rate);
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);
            if (newValue == true) { lightBitSet = true; } else { lightBitSet = false; }

            // Catch case when BMS has started and e.newFlightData is null
            if (e.newFlightData != null)
                newBlinkingBitSet = ((e.newFlightData.blinkBits & (int)blinkBit) != 0);
            else
                newBlinkingBitSet = false;

            // Catch case when BMS has exited and e.oldFlightData is null
            if (e.oldFlightData != null)
                oldBlinkingBitSet = ((e.oldFlightData.blinkBits & (int)blinkBit) != 0);
            else
                oldBlinkingBitSet = false;

            if ((oldValue != newValue) || (newBlinkingBitSet != oldBlinkingBitSet))     // Data changed
            {
                timer.Stop();
                RaiseFalconLightChanged(oldValue, newValue);
                if (newValue == true)       // LB went from OFF to ON
                {
                    lampLit = true;
                    if (newBlinkingBitSet)  // Blinking - start blink timer
                        timer.Start();
                    else                    // Not Blinking - stop blink timer
                        timer.Stop();
                }
                else                        // LB went from ON to OFF
                {
                    lampLit = false;
                    timer.Stop();
                }
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (lightBitSet)    // Light is ON
            {
                if (newBlinkingBitSet)  // Light is blinking
                {
                    if (lampLit)        // Lamp is ON, turn it OFF
                    {
                        lampLit = false;
                        timer.Stop();
                        RaiseFalconLightChanged(true, false);
                        timer.Start();
                    }
                    else                // Lmap is OFF, turn it ON
                    {
                        lampLit = true;
                        timer.Stop();
                        RaiseFalconLightChanged(false, true);
                        timer.Start();
                    }
                }
                else                    // Lamp is ON but no longer blinking, Stop blink timer.
                {
                    lampLit = true;
                    timer.Stop();
                    RaiseFalconLightChanged(false, true);
                }
            }
            else                // Light is OFF
            {
                lampLit = false;
                timer.Stop();
                RaiseFalconLightChanged(true, false);
            }
        }
    }

    #endregion // FalconBlinkingLamp

    // Used to control blower in the cockpit that blows air through the air vents to add realism
    #region FalconRPMOver65Percent

    public class FalconRPMOver65Percent : FalconLight
    {
        private readonly Func<FlightData, bool> getFlightDataValue;

        public FalconRPMOver65Percent(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (oldValue != newValue)
                RaiseFalconLightChanged(oldValue, newValue);
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }
    }

    #endregion // FalconRPMOver65Percent

    #region FalconGearHandleSol

    public class FalconGearHandleSol : FalconLight
    {
        public int Engaged = 0;
        public int OnGnd = 0;

        private bool mainGenOn = Convert.ToBoolean(PowerBits.MainGenerator);
        private bool inPit = Convert.ToBoolean(HsiBits.Flying);

        private readonly Func<FlightData, bool> getFlightDataValue;
        private DispatcherTimer timer;

        public FalconGearHandleSol(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(260);
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (newValue == true) { Engaged = 1;  } else { Engaged = 0; }

            if (oldValue != newValue && newValue == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(oldValue, newValue);
                timer.Start();
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Engaged == 0 && inPit == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(true, false);
                timer.Start();
            }
            else
            {
                timer.Stop();
                RaiseFalconLightChanged(false, true);
                timer.Start();
            }
        }
    }

    #endregion // FalconGearHandleSol

    #region RealSpeedBrakeOpen

    public class RealSpeedBrakeOpen : FalconLight
    {
        private int engaged;
        private bool emergBusPwr = Convert.ToBoolean(PowerBits.BusPowerEmergency);

        private readonly Func<FlightData, bool> getFlightDataValue;
        private DispatcherTimer timer;

        public RealSpeedBrakeOpen(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(260);
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (newValue == true) { engaged = 1; } else { engaged = 0; }

            if (oldValue != newValue && newValue == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(oldValue, newValue);
                timer.Start();
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (engaged == 1 & emergBusPwr == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(false, true);
                timer.Start();
            }
            else
            {
                timer.Stop();
                RaiseFalconLightChanged(true, false);
                timer.Start();
            }
        }
    }

    #endregion

    #region RealSpeedBrakeClosed

    public class RealSpeedBrakeClosed : FalconLight
    {
        private int engaged;
        private bool emergBusPower = Convert.ToBoolean(PowerBits.BusPowerEmergency);

        private readonly Func<FlightData, bool> getFlightDataValue;
        private DispatcherTimer timer;

        public RealSpeedBrakeClosed(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(260);
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (newValue == true) { engaged = 1; } else { engaged = 0; }

            if (oldValue != newValue && newValue == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(oldValue, newValue);
                timer.Start();
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (engaged == 1 && emergBusPower == true)
            {
                timer.Stop();
                RaiseFalconLightChanged(true, false);
                timer.Start();
            }
            else
            {
                timer.Stop();
                RaiseFalconLightChanged(false, true);
                timer.Start();
            }
        }
    }

    #endregion // RealSpeedBrakeClosed

    #region FalconTacanBits

    public class FalconTacanBits : FalconLightBit
    {
        protected readonly int tacanSource;

        public FalconTacanBits(string group, string label, TacanBits bit, int tacanSource)
            : base(group, label, (int)bit)
        {
            this.tacanSource = tacanSource;
        }

        protected override bool GetNonNullValue(FlightData flightData)
        {
            return (flightData.tacanInfo[tacanSource] & bit) != 0;
        }
    }

    #endregion // FalconTacanBits

    #region FalconDataBits

    public class FalconDataBits : FalconLight
    {
        private readonly Func<FlightData, bool> getFlightDataValue;

        public FalconDataBits(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (oldValue != newValue)
                RaiseFalconLightChanged(oldValue, newValue);
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }
    }

    #endregion // FalconDataBits

    #region FalconTWPOpenLight

    public class FalconTWPOpenLight : FalconLight
    {
        private bool newAuxPwrOn = false;
        private bool oldAuxPwrOn = false;

        private readonly Func<FlightData, bool> getFlightDataValue;

        public FalconTWPOpenLight(string group, string label, Func<FlightData, bool> getFlightDataValue)
            : base(group, label)
        {
            this.getFlightDataValue = getFlightDataValue;
        }

        protected override void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            bool? oldValue = GetValue(e.oldFlightData);
            bool? newValue = GetValue(e.newFlightData);

            if (oldValue != null && newValue != null)
            {
                newAuxPwrOn = ((e.newFlightData.lightBits2 & (int)LightBits2.AuxPwr) != 0);
                oldAuxPwrOn = ((e.oldFlightData.lightBits2 & (int)LightBits2.AuxPwr) != 0);

                if (newAuxPwrOn)                // Only flip-flop OPEN light if RWR is ON
                {
                    if (oldValue != newValue)   // PRIORITY Light status changed. Update OPEN Light   
                    {
                        if (newValue == true)   // PRIORITY light went ON. Turn OPEN light OFF
                        {
                            RaiseFalconLightChanged(true, false);
                        }
                        else                    // PRIORITY light went OFF. Turn OPEN light ON
                        {
                            RaiseFalconLightChanged(false, true);
                        }
                    }
                }

                if (oldAuxPwrOn != newAuxPwrOn) // AuxPwr changed
                {
                    if (newAuxPwrOn && newValue == false)   // AuxPwr came back on & PRIORITY light is OFF. Turn OPEN light ON
                    {
                        RaiseFalconLightChanged(false, true);
                    }
                    else                                    // AuxPwr came back on but PRIORITY light is ON. Turn OPEN light OFF
                    {
                        RaiseFalconLightChanged(true, false);
                    }
                }
            }
        }

        private bool? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataValue(flightData);
        }
    }

    #endregion // FalconTWPOpenLight


    #region FalconLightChangedEventArgs

    public class FalconLightChangedEventArgs : EventArgs
    {
        public readonly bool? oldValue;
        public readonly bool? newValue;

        public FalconLightChangedEventArgs(bool? oldValue, bool? newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }

    #endregion // FalconLightChangedEventAgs
}