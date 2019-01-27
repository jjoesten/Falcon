using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: Add Arduino Gauge Driver
            // TODO: Add DEDuino
        }

        public void Dispose()
        {
            // TODO: Implement Configuration.Dispose
            // Dispose of registered devices
            foreach (PoKeys poKeys in PoKeysList)
                poKeys.Dispose();
        }
        #endregion

        #region SetOwner
        public void SetOwner()
        {
            // TODO: SetOwner for registered devices (pokeys, gauge drivers)
            foreach (PoKeys poKeys in PoKeysList)
                poKeys.SetOwner(this);
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
    }
}
