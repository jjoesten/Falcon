using F4SharedMem;
using System;

namespace Aesalon
{
    public class FalconGauge
    {
        public string Label { get; private set; }
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }
        public int FormatTotalSize { get; private set; }
        public int FormatIntegralPartMinSize { get; private set; }
        public int FormatFractionalPartSize { get; private set; }

        private readonly Func<FlightData, float> getFlightDataProperty;

        public FalconGauge(string label, Func<FlightData, float> getFlightDataProperty, float minValue, float maxValue, int formatTotalSize, int formatIntegralPartMinSize, int formatFactionalPartSize)
        {
            Label = label;
            MinValue = minValue;
            MaxValue = maxValue;
            FormatTotalSize = formatTotalSize;
            FormatIntegralPartMinSize = formatIntegralPartMinSize;
            FormatFractionalPartSize = formatFactionalPartSize;
            this.getFlightDataProperty = getFlightDataProperty;
        }

        #region FalconGaugeChanged

        private EventHandler<FalconGaugeChangedEventArgs> falconGaugeChanged;
        private int nbUser = 0;

        public event EventHandler<FalconGaugeChangedEventArgs> FalconGaugeChanged
        {
            add
            {
                falconGaugeChanged += value;

                ++nbUser;
                if (nbUser == 1)
                    FalconConnector.Singleton.FlightDataChanged += OnFlightDataChanged;
            }

            remove
            {
                falconGaugeChanged -= value;

                --nbUser;
                if (nbUser == 0)
                    FalconConnector.Singleton.FlightDataChanged -= OnFlightDataChanged;
            }
        }

        protected void RaiseFalconLightChanged(float? falconValue)
        {
            if (falconGaugeChanged != null)
                falconGaugeChanged(this, new FalconGaugeChangedEventArgs(falconValue));
        }

        #endregion // FalconGaugeChanged

        #region OnFlightDataChanged

        private void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            float? oldValue = GetValue(e.oldFlightData);
            float? newValue = GetValue(e.newFlightData);

            if (oldValue != newValue)
                RaiseFalconLightChanged(newValue);
        }

        private float? GetValue(FlightData flightData)
        {
            if (flightData == null)
                return null;
            else
                return getFlightDataProperty(flightData);
        }

        #endregion // OnFlightDataChanged
    }

    public class FalconGaugeChangedEventArgs : EventArgs
    {
        public readonly float? falconValue;

        public FalconGaugeChangedEventArgs(float? falconValue)
        {
            this.falconValue = falconValue;
        }
    }
}