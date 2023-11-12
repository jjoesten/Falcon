using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Aesalon
{
    public class PoKeys : BindableObject, IDisposable
    {
        #region Construction / Destruction

        public PoKeys()
        {
            RemovePoKeysCommand = new RelayCommand(ExecuteRemovePoKeys);
            AddDigitalOutputCommand = new RelayCommand(ExecuteAddDigitalOutput);
            AddMatrixLedOutputCommand = new RelayCommand(ExecuteAddMatrixLedOutput);
            AddSevenSegmentDisplayCommand = new RelayCommand(ExecuteAddSevenSegmentDisplay);
            AddPoStepCommand = new RelayCommand(ExecuteAddPoStepCommand);
        }

        public void Dispose()
        {
            foreach (DigitalOutput digitalOutput in DigitalOutputList)
                digitalOutput.Dispose();

            foreach (MatrixLedOutput matrixLedOutput in MatrixLedOutputList)
                matrixLedOutput.Dispose();

            foreach (SevenSegmentDisplay sevenSegmentDisplay in SevenSegmentDisplayList)
                sevenSegmentDisplay.Dispose();
        }

        #endregion

        #region Owner
        private Configuration owner;
        public void SetOwner(Configuration config)
        {
            owner = config;
            UpdateStatus();

            foreach (DigitalOutput digitalOutput in DigitalOutputList)
                digitalOutput.SetOwner(this);

            foreach (MatrixLedOutput matrixLedOutput in MatrixLedOutputList)
                matrixLedOutput.SetOwner(this);

            foreach (SevenSegmentDisplay sevenSegmentDisplay in SevenSegmentDisplayList)
                sevenSegmentDisplay.SetOwner(this);
        }
        #endregion

        #region AvailablePoKeysList
        private List<AvailablePoKeys> availablePoKeysList;
        [XmlIgnore]
        public List<AvailablePoKeys> AvailablePoKeysList
        {
            get
            {
                if (availablePoKeysList == null)
                {
                    availablePoKeysList = PoKeysEnumerator.Singleton.AvailablePoKeysList
                        .OrderBy(ap => ap.PoKeysId)
                        .ToList();
                }
                return availablePoKeysList;
            }
        }
        #endregion

        #region SelectedPoKeys
        private AvailablePoKeys selectedPoKeys;
        [XmlIgnore]
        public AvailablePoKeys SelectedPoKeys
        {
            get { return selectedPoKeys; }
            set
            {
                if (selectedPoKeys == value)
                    return;

                selectedPoKeys = value;
                RaisePropertyChanged(() => SelectedPoKeys);

                UpdateStatus();
                UpdateChildrenStatus();

                if (selectedPoKeys == null)
                    Serial = null;
                else
                    Serial = selectedPoKeys.PoKeysSerial;
            }
        }
        #endregion

        #region Serial
        private int? serial;
        public int? Serial
        {
            get { return serial; }
            set
            {
                if (serial == value)
                    return;
                serial = value;
                RaisePropertyChanged(() => Serial);

                if (!Serial.HasValue)
                    SelectedPoKeys = null;
                else
                {
                    AvailablePoKeys availablePoKeys = PoKeysEnumerator.Singleton.AvailablePoKeysList
                        .FirstOrDefault(ap => ap.PoKeysSerial == serial);

                    if (availablePoKeys == null)
                    {
                        availablePoKeys = new AvailablePoKeys(serial.Value, string.Empty, null, null);
                        availablePoKeys.Error = Translations.Main.PoKeysNotFoundError;
                        PoKeysEnumerator.Singleton.AvailablePoKeysList.Add(availablePoKeys);
                    }

                    SelectedPoKeys = availablePoKeys;
                }
            }
        }
        #endregion

        #region DigitalOutputList
        private ObservableCollection<DigitalOutput> digitalOutputList = new ObservableCollection<DigitalOutput>();
        public ObservableCollection<DigitalOutput> DigitalOutputList
        {
            get { return digitalOutputList; }
            set
            {
                digitalOutputList = value;
                RaisePropertyChanged(() => DigitalOutputList);
            }
        }
        #endregion

        #region MatrixLedOutputList
        private ObservableCollection<MatrixLedOutput> matrixLedOutputList = new ObservableCollection<MatrixLedOutput>();
        public ObservableCollection<MatrixLedOutput> MatrixLedOutputList
        {
            get { return matrixLedOutputList; }
            set
            {
                matrixLedOutputList = value;
                RaisePropertyChanged(() => MatrixLedOutputList);
            }
        }
        #endregion

        #region SevenSegmentDisplayList
        private ObservableCollection<SevenSegmentDisplay> sevenSegmentDisplayList = new ObservableCollection<SevenSegmentDisplay>();
        public ObservableCollection<SevenSegmentDisplay> SevenSegmentDisplayList
        {
            get { return sevenSegmentDisplayList; }
            set
            {
                sevenSegmentDisplayList = value;
                RaisePropertyChanged(() => SevenSegmentDisplayList);
            }
        }
        #endregion

        #region PoStep Board

        private PoStep poStep;
        public PoStep PoStep
        {
            get { return poStep; }
            set
            {
                poStep = value;
                RaisePropertyChanged(() => PoStep);
            }
        }

        #endregion

        #region Error
        private string error;
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

        #region PoKeysIndex
        [XmlIgnore]
        public int? PoKeysIndex { get; private set; }
        #endregion

        #region UpdateStatus
        private void UpdateStatus()
        {
            if (owner == null)
                return;

            if (SelectedPoKeys == null)
            {
                Error = null;
                PoKeysIndex = null;
            }
            else
            {
                Error = SelectedPoKeys.Error;
                PoKeysIndex = SelectedPoKeys.PoKeysIndex;
            }
        }

        private void UpdateChildrenStatus()
        {
            if (owner == null)
                return;

            foreach (DigitalOutput digitalOutput in DigitalOutputList)
                digitalOutput.UpdateStatus();

            foreach (MatrixLedOutput matrixLedOutput in MatrixLedOutputList)
                matrixLedOutput.UpdateStatus();

            foreach (SevenSegmentDisplay sevenSegmentDisplay in SevenSegmentDisplayList)
                sevenSegmentDisplay.UpdateStatus();
        }
        #endregion

        #region RemovePoKeysCommand
        [XmlIgnore]
        public RelayCommand RemovePoKeysCommand { get; private set; }
        private void ExecuteRemovePoKeys(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemovePoKeysText, SelectedPoKeys?.PoKeysId),
                Translations.Main.RemovePoKeysCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.PoKeysList.Remove(this);
            Dispose();
        }
        #endregion

        #region AddDigitalOutputCommand
        [XmlIgnore]
        public RelayCommand AddDigitalOutputCommand { get; private set; }
        private void ExecuteAddDigitalOutput(object o)
        {
            DigitalOutput digitalOutput = new DigitalOutput();
            digitalOutput.SetOwner(this);
            DigitalOutputList.Add(digitalOutput);
        }
        #endregion

        #region AddMatrixLedOutputCommand
        [XmlIgnore]
        public RelayCommand AddMatrixLedOutputCommand { get; private set; }
        private void ExecuteAddMatrixLedOutput(object o)
        {
            MatrixLedOutput matrixLedOutput = new MatrixLedOutput();
            matrixLedOutput.SetOwner(this);
            MatrixLedOutputList.Add(matrixLedOutput);
        }
        #endregion

        #region AddSevenSegmentDisplayCommand
        [XmlIgnore]
        public RelayCommand AddSevenSegmentDisplayCommand { get; private set; }
        private void ExecuteAddSevenSegmentDisplay(object o)
        {
            SevenSegmentDisplay sevenSegmentDisplay = new SevenSegmentDisplay();
            sevenSegmentDisplay.SetOwner(this);
            SevenSegmentDisplayList.Add(sevenSegmentDisplay);
        }
        #endregion

        #region AddPoStepCommand
        [XmlIgnore]
        public RelayCommand AddPoStepCommand { get; private set; }
        private void ExecuteAddPoStepCommand(object o)
        {
            PoStep poStep = new PoStep();
            poStep.SetOwner(this);
            PoStep = poStep;
        }
        #endregion

        #region SevenSegmentMatrixLed1Config / SevenSegmentMatrixLed2Config
        private SevenSegmentMatrixLedConfig sevenSegmentMatrixLed1Config;
        public SevenSegmentMatrixLedConfig SevenSegmentMatrixLed1Config
        {
            get { return sevenSegmentMatrixLed1Config; }
            set
            {
                if (sevenSegmentMatrixLed1Config == value)
                    return;
                sevenSegmentMatrixLed1Config = value;
                RaisePropertyChanged(nameof(SevenSegmentMatrixLed1Config));
            }
        }
        public SevenSegmentMatrixLedConfig GetOrCreateSevenSegmentMatrixLed1Config()
        {
            if (SevenSegmentMatrixLed1Config == null)
                SevenSegmentMatrixLed1Config = new SevenSegmentMatrixLedConfig();
            return SevenSegmentMatrixLed1Config;
        }

        private SevenSegmentMatrixLedConfig sevenSegmentMatrixLed2Config;
        public SevenSegmentMatrixLedConfig SevenSegmentMatrixLed2Config
        {
            get { return sevenSegmentMatrixLed2Config; }
            set
            {
                if (sevenSegmentMatrixLed2Config == value)
                    return;
                sevenSegmentMatrixLed2Config = value;
                RaisePropertyChanged(nameof(SevenSegmentMatrixLed2Config));
            }
        }
        public SevenSegmentMatrixLedConfig GetOrCreateSevenSegmentMatrixLed2Config()
        {
            if (SevenSegmentMatrixLed2Config == null)
                SevenSegmentMatrixLed2Config = new SevenSegmentMatrixLedConfig();
            return SevenSegmentMatrixLed2Config;
        }

        #endregion // SevenSegmentMatrixLed1Config / SevenSegmentMatrixLed2Config
    }
}