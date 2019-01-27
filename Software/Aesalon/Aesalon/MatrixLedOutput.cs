using PoKeysDevice_DLL;
using System;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Aesalon
{
    public class MatrixLedOutput : FalconLightConsumer
    {
        public MatrixLedOutput()
        {
            RemoveMatrixLedOutputCommand = new RelayCommand(ExecuteRemoveMatrixLedOutput);
        }

        #region MatrixLed

        private MatrixLed matrixLed;
        [XmlIgnore]
        public MatrixLed MatrixLed
        {
            get { return matrixLed; }
            set
            {
                if (matrixLed == value)
                    return;
                matrixLed = value;
                RaisePropertyChanged(nameof(MatrixLed));

                UpdateStatus();

                if (matrixLed == null)
                    MatrixLedName = null;
                else
                    MatrixLedName = matrixLed.Name;
            }
        }

        #endregion // MatrixLed

        #region MatrixLedName

        private string matrixLedName;
        public string MatrixLedName
        {
            get { return matrixLedName; }
            set
            {
                if (matrixLedName == value)
                    return;
                matrixLedName = value;
                RaisePropertyChanged(nameof(MatrixLedName));

                if (string.IsNullOrEmpty(matrixLedName))
                    MatrixLed = null;
                else
                    MatrixLed = MatrixLed.AvailableMatrixLedList.FirstOrDefault(item => item.Name == matrixLedName);
            }
        }

        #endregion // MatrixLedName

        #region Row
        private byte? row;
        public byte? Row
        {
            get { return row; }
            set
            {
                if (row == value)
                    return;
                row = value;
                RaisePropertyChanged(nameof(Row));

                UpdateStatus();
            }
        }
        #endregion // Row

        #region Column
        private byte? column;
        public byte? Column
        {
            get { return column; }
            set
            {
                if (column == value)
                    return;
                column = value;
                RaisePropertyChanged(nameof(Column));

                UpdateStatus();
            }
        }
        #endregion // Column

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

        #endregion // Error

        #region owner
        private PoKeys owner;
        internal void SetOwner(PoKeys poKeys)
        {
            owner = poKeys;
            UpdateStatus();
        }
        #endregion // owner

        #region RemoveMatrixLedOutputCommand

        [XmlIgnore]
        public RelayCommand RemoveMatrixLedOutputCommand { get; private set; }

        private void ExecuteRemoveMatrixLedOutput(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveMatrixLedOutputText, MatrixLed != null ? MatrixLed.Name : string.Empty, Row, Column),
                Translations.Main.RemoveMatrixLedOutputCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            owner.MatrixLedOutputList.Remove(this);
            Dispose();
        }

        #endregion // RemoveMatrixLedOutputCommand

        #region UpdateStatus
        internal void UpdateStatus()
        {
            if (owner == null)
                return;

            if (MatrixLed == null || !Row.HasValue || !Column.HasValue)
            {
                Error = null;
            }
            else if (!owner.PoKeysIndex.HasValue)
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
                    if (!MatrixLed.IsPixelEnabled((byte)(Row.Value - 1), (byte)(Column.Value - 1)))
                    {
                        Error = string.Format(Translations.Main.MatrixLedPixelErrorNotEnabled, Row, Column);
                    }
                    else
                    {
                        Error = null;
                    }

                    poKeysDevice.DisconnectDevice();
                }
            }

            WriteOutputState();
        }
        #endregion // UpdateStatus

        #region WriteOutputState
        protected override void WriteOutputState()
        {
            if (string.IsNullOrEmpty(Error) && owner != null && owner.PoKeysIndex.HasValue && MatrixLed != null && Row.HasValue && Column.HasValue)
            {
                PoKeysDevice poKeysDevice = PoKeysEnumerator.Singleton.PoKeysDevice;

                if (!poKeysDevice.ConnectToDevice(owner.PoKeysIndex.Value))
                {
                    Error = Translations.Main.PoKeysConnectError;
                }
                else
                {
                    if (!MatrixLed.SetPixel((byte)(Row.Value - 1), (byte)(Column.Value - 1), OutputState))
                    {
                        Error = Translations.Main.MatrixLedErrorWrite;
                    }

                    poKeysDevice.DisconnectDevice();
                }
            }
        }
        #endregion // WriteOutputstate
    }
}