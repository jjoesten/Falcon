using System;
using System.Linq;
using System.Xml.Serialization;

namespace Aesalon
{
    public abstract class FalconLightConsumer : BindableObject, IDisposable
    {
        #region Construction
        protected FalconLightConsumer()
        {
            SetOutputStateToOnCommand = new RelayCommand(ExecuteSetOutputStateToOn);
            SetOutputStateToOffCommand = new RelayCommand(ExecuteSetOutputStateToOff);
        }

        public void Dispose()
        {
            if (FalconLight != null)
                FalconLight.FalconLightChanged -= OnFalconLightChanged;
        }
        #endregion

        #region FalconLight
        private FalconLight falconLight;
        [XmlIgnore]
        public FalconLight FalconLight
        {
            get { return falconLight; }
            set
            {
                if (falconLight == value)
                    return;
                if (falconLight != null)
                    falconLight.FalconLightChanged -= OnFalconLightChanged;
                falconLight = value;
                if (falconLight != null)
                    falconLight.FalconLightChanged += OnFalconLightChanged;
                RaisePropertyChanged(() => FalconLight);

                ResetOutputState();

                if (falconLight == null)
                    FalconLightLabel = null;
                else
                    FalconLightLabel = falconLight.Label;
            }
        }
        #endregion

        #region FalconLightLabel
        private string falconLightLabel;
        public string FalconLightLabel
        {
            get { return falconLightLabel; }
            set
            {
                if (falconLightLabel == value)
                    return;
                falconLightLabel = value;
                RaisePropertyChanged(() => FalconLightLabel);

                if (string.IsNullOrEmpty(falconLightLabel))
                    FalconLight = null;
                else
                    FalconLight = FalconConnector.Singleton.LightList.FirstOrDefault(item => item.Label == falconLightLabel);
            }
        }
        #endregion

        #region OnFalconLightChanged
        private void OnFalconLightChanged(object sender, FalconLightChangedEventArgs e)
        {
            if (e.newValue == true)
                OutputState = true;
            else
                OutputState = false;
        }
        #endregion

        #region Output State
        private bool outputState = false;
        [XmlIgnore]
        public bool OutputState
        {
            get { return outputState; }
            set
            {
                outputState = value;
                RaisePropertyChanged(() => OutputState);

                WriteOutputState();
            }
        }

        protected abstract void WriteOutputState();
        #endregion

        #region ResetOutputState
        private void ResetOutputState() => OutputState = false;
        #endregion

        #region OutputState Commands
        [XmlIgnore]
        public RelayCommand SetOutputStateToOnCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand SetOutputStateToOffCommand { get; private set; }

        private void ExecuteSetOutputStateToOn(object o) => OutputState = true;
        private void ExecuteSetOutputStateToOff(object o) => OutputState = false;
        #endregion
    }
}