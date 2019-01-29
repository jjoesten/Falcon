using Aesalon.ArduinoDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Aesalon
{
    public class DEDuino : BindableObject, IDisposable
    {
        #region Construction / Destruction

        public DEDuino()
        {
            RemoveDEDuinoCommand = new RelayCommand(ExecuteRemoveDEDuino);

            FalconConnector.Singleton.FlightDataChanged += OnFlightDataChanged;
        }

        public void Dispose()
        {

        }

        #endregion

        #region COM Port
        private string comPort;
        public string COMPort
        {
            get { return comPort; }
            set
            {
                comPort = value;
                RaisePropertyChanged(() => COMPort);

                UpdateDevice();
            }
        }

        private IEnumerable<string> ComPortAsEnumerable()
        {
            if (!string.IsNullOrWhiteSpace(COMPort))
                yield return COMPort;
        }

        private List<string> comPortList;
        [XmlIgnore]
        public List<string> COMPortList
        {
            get
            {
                if (comPortList == null)
                {
                    comPortList = DEDuinoEnumerator.Singleton.AvailableCOMPorts
                        .Select(comPort => comPort)
                        .Union(ComPortAsEnumerable())
                        .OrderBy(comPort => comPort)
                        .ToList();
                }
                return comPortList;
            }
        }
        #endregion

        #region Device
        private DEDuinoDevice device;
        [XmlIgnore]
        public DEDuinoDevice Device
        {
            get { return device; }
            set
            {
                if (device == value)
                    return;

                if (device != null)
                {
                    try
                    {
                        device.DisconnectDevice();
                    }
                    catch
                    {
                        throw;
                    }
                }
                device = value;
            }
        }
        #endregion

        #region Error
        private string error;
        [XmlIgnore]
        public string Error
        {
            get { return error; }
            set
            {
                if (error == value)
                    return;
                error = value;
                RaisePropertyChanged(() => Error);
            }
        }
        #endregion

        #region Owner
        private Configuration owner;

        internal void SetOwner(Configuration config)
        {
            owner = config;

            UpdateDevice();
        }
        #endregion

        #region UpdateStatus
        private void UpdateDevice()
        {
            if (owner == null)
                return;

            Device = null;

            if (string.IsNullOrWhiteSpace(COMPort))
                Error = null;
            else
            {
                string assignedCOMPort = DEDuinoEnumerator.Singleton.AvailableCOMPorts.FirstOrDefault(item => item == COMPort);
                if (assignedCOMPort == null)
                    Error = Translations.Main.DEDuinoNotFoundError;
                else
                {
                    try
                    {
                        Device = new DEDuinoDevice(assignedCOMPort);
                        Error = null;
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                    }
                }
            }
        }
        #endregion

        #region OnFlightDataChanged
        private void OnFlightDataChanged(object sender, FlightDataChangedEventArgs e)
        {
            Device.FlightData = e.newFlightData;
        }
        #endregion

        #region RemoveDEDuinoCommand
        [XmlIgnore]
        public RelayCommand RemoveDEDuinoCommand { get; private set; }
        private void ExecuteRemoveDEDuino(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveDEDuinoText, COMPort),
                Translations.Main.RemoveDEDuinoCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.DEDuinoList.Remove(this);
            Dispose();
        }
        #endregion
    }
}
