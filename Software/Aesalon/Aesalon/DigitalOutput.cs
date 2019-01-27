using PoKeysDevice_DLL;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace Aesalon
{
    public class DigitalOutput : FalconLightConsumer
    {
        #region Construction

        static DigitalOutput()
        {
            pinIdList = new List<byte>();
            for (byte pinId = 1; pinId <= 55; ++pinId)
                pinIdList.Add(pinId);
        }

        public DigitalOutput()
        {
            RemoveDigitalOutputCommand = new RelayCommand(executeRemoveDigitalOutput);
        }

        #endregion // Construction

        #region PinIdList
        private static readonly List<byte> pinIdList;
        [XmlIgnore]
        public static List<byte> PinIdList { get { return pinIdList; } }
        #endregion // PinIdList

        #region PinId
        private byte? pinId;
        public byte? PinId
        {
            get { return pinId; }
            set
            {
                if (pinId == value)
                    return;
                pinId = value;
                RaisePropertyChanged(() => PinId);

                UpdateStatus();
            }
        }

        #endregion // PinId

        #region RemoveDigitalOutputCommand
        [XmlIgnore]
        public RelayCommand RemoveDigitalOutputCommand { get; private set; }
        private void executeRemoveDigitalOutput(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveDigitalOutputText, PinId),
                Translations.Main.RemoveDigitalOutputCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.DigitalOutputList.Remove(this);
            Dispose();
            
        }
        #endregion // RemoveDigitalOutputCommand

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
        #endregion // Error

        #region owner
        private PoKeys owner;
        public void SetOwner(PoKeys pokeys)
        {
            owner = pokeys;
            UpdateStatus();
        }
        #endregion // owner

        #region UpdateStatus

        public void UpdateStatus()
        {
            if (owner == null)
                return;

            if (!PinId.HasValue)
                Error = null;
            else if (!owner.PoKeysIndex.HasValue)
                Error = null;
            else
            {
                PoKeysDevice poKeysDevice = PoKeysEnumerator.Singleton.PoKeysDevice;

                if (!poKeysDevice.ConnectToDevice(owner.PoKeysIndex.Value))
                {
                    Error = Translations.Main.PoKeysConnectError;
                }
                else
                {
                    byte pinFunction = 0;
                    if (!poKeysDevice.GetPinData((byte)(PinId.Value-1), ref pinFunction))
                    {
                        Error = Translations.Main.DigitalOutputErrorGetIOType;
                    }
                    else
                    {
                        if ((pinFunction & 0x4) == 0)
                        {
                            Error = Translations.Main.DigitalOutputErrorBadIOType;
                        }
                        else
                        {
                            Error = null;
                        }
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
            if (string.IsNullOrEmpty(Error) && owner != null && owner.PoKeysIndex.HasValue && PinId.HasValue)
            {
                PoKeysDevice pokeysDevice = PoKeysEnumerator.Singleton.PoKeysDevice;

                if (!pokeysDevice.ConnectToDevice(owner.PoKeysIndex.Value))
                {
                    Error = Translations.Main.PoKeysConnectError;
                }
                else
                {
                    if (!pokeysDevice.SetOutput((byte)(PinId.Value - 1), OutputState))
                    {
                        Error = Translations.Main.DigitalOutputErrorWrite;
                    }

                    pokeysDevice.DisconnectDevice();
                }
            }
        }

        #endregion // WriteOutputState
    }
}