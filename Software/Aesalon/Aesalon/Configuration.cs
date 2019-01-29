using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Aesalon
{
    public class Configuration : BindableObject, IDisposable
    {
        // NOTE: Unsure if this format version stuff is necessary...
        [XmlIgnore]
        public static Version CurrentFormatVersion { get; } = new Version(1, 1);
        [XmlAttribute("formatVersion")]
        public string FormatVersion { get; set; }

        public double ReadFalconDataTimerIntervalMS
        {
            get { return FalconConnector.Singleton.ReadFalconDataTimerInterval.TotalMilliseconds; }
            set { FalconConnector.Singleton.ReadFalconDataTimerInterval = TimeSpan.FromMilliseconds(value); }
        }

        #region Construction / Destruction
        public Configuration()
        {
            AddPoKeysCommand = new RelayCommand(ExecuteAddPoKeys);
            AddArduinoGaugeCommand = new RelayCommand(ExecuteAddArduinoGauge);
            AddDEDuinoCommand = new RelayCommand(ExecuteAddDEDuino);
        }

        public void Dispose()
        {
            foreach (PoKeys poKeys in PoKeysList)
                poKeys.Dispose();

            foreach (ArduinoGauge arduinoGauge in ArduinoGaugeList)
                arduinoGauge.Dispose();

            foreach (DEDuino deduino in DEDuinoList)
                deduino.Dispose();
        }
        #endregion

        #region SetOwner
        public void SetOwner()
        {            
            foreach (PoKeys poKeys in PoKeysList)
                poKeys.SetOwner(this);

            foreach (ArduinoGauge arduinoGauge in ArduinoGaugeList)
                arduinoGauge.SetOwner(this);

            foreach (DEDuino deduino in DEDuinoList)
                deduino.SetOwner(this);
        }
        #endregion

        #region PoKeys

        private ObservableCollection<PoKeys> poKeysList = new ObservableCollection<PoKeys>();
        public ObservableCollection<PoKeys> PoKeysList
        {
            get { return poKeysList; }
            set
            {
                poKeysList = value;
                RaisePropertyChanged(() => PoKeysList);
            }
        }

        [XmlIgnore]
        public RelayCommand AddPoKeysCommand { get; private set; }

        private void ExecuteAddPoKeys(object o)
        {
            PoKeys poKeys = new PoKeys();
            poKeys.SetOwner(this);
            PoKeysList.Add(poKeys);
        }

        #endregion

        #region ArduinoGauge
        private ObservableCollection<ArduinoGauge> arduinoGaugeList = new ObservableCollection<ArduinoGauge>();
        public ObservableCollection<ArduinoGauge> ArduinoGaugeList
        {
            get { return arduinoGaugeList; }
            set
            {
                arduinoGaugeList = value;
                RaisePropertyChanged(() => ArduinoGaugeList);
            }
        }

        [XmlIgnore]
        public RelayCommand AddArduinoGaugeCommand { get; private set; }
        private void ExecuteAddArduinoGauge(object o)
        {
            ArduinoGauge arduinoGauge = new ArduinoGauge();
            arduinoGauge.SetOwner(this);
            ArduinoGaugeList.Add(arduinoGauge);
        }
        #endregion

        #region DEDuino

        private ObservableCollection<DEDuino> deduinoList = new ObservableCollection<DEDuino>();
        public ObservableCollection<DEDuino> DEDuinoList
        {
            get { return deduinoList; }
            set
            {
                deduinoList = value;
                RaisePropertyChanged(() => DEDuinoList);
            }
        }

        [XmlIgnore]
        public RelayCommand AddDEDuinoCommand { get; private set; }
        private void ExecuteAddDEDuino(object o)
        {
            DEDuino deduino = new DEDuino();
            deduino.SetOwner(this);
            DEDuinoList.Add(deduino);
        }

        #endregion
    }
}
