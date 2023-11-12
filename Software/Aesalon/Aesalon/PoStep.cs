using PoKeysDevice_DLL;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Navigation;
using System.Xml.Serialization;

namespace Aesalon
{
    public class PoStep : BindableObject, IDisposable
    {
        #region Construction / Destruction

        public PoStep()
        {
            RemovePoStepCommand = new RelayCommand(ExecuteRemovePoStepCommand);
        }

        public void Dispose()
        {
            // TODO: Dispose of stepper motors
            // TODO: Dispose of device
            throw new NotImplementedException();
        }

        #endregion

        #region StepperMotorList
        private ObservableCollection<PoStepStepperMotor> stepperMotorList = new ObservableCollection<PoStepStepperMotor> ();
        public ObservableCollection<PoStepStepperMotor> StepperMotorList
        {
            get { return stepperMotorList; }
            set
            {
                stepperMotorList = value;
                RaisePropertyChanged(() => StepperMotorList);
            }
        }
        #endregion

        #region RemovePoStepCommand
        [XmlIgnore]
        public RelayCommand RemovePoStepCommand { get; private set; }
        private void ExecuteRemovePoStepCommand(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                Translations.Main.RemovePoStepDisplayText,
                Translations.Main.RemovePoStepCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.PoStep = null;
            Dispose();
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
                RaisePropertyChanged(nameof(Error));
            }
        }
        #endregion

        #region Owner
        private PoKeys owner;
        public void SetOwner(PoKeys poKeys)
        {
            owner = poKeys;

            UpdateStatus();

            // TODO: Update status for each stepper motor 
        }
        #endregion

        #region Update
        private void UpdateStatus()
        {
            if (owner != null)
                return;

            // TODO: PoStep Config??
            if (!owner.PoKeysIndex.HasValue)
            {
                Error = null;
            }
            else
            {
                PoKeysDevice poKeysDevice = PoKeysEnumerator.Singleton.PoKeysDevice;

                if (!poKeysDevice.ConnectToDevice(owner.PoKeysIndex.Value))
                {
                    Error = Translations.Main.PoKeysConnectError;
                }
                else
                {
                    // TODO: Check that PoStep board settings are correct and available?
                    Error = null;

                    poKeysDevice.DisconnectDevice();
                }
            }
        }
        #endregion
    }
}