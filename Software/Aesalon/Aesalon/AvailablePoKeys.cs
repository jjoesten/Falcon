namespace Aesalon
{
    public class AvailablePoKeys : BindableObject
    {
        public int PoKeysSerial { get; private set; }
        public string PoKeysName { get; private set; }
        public byte? PoKeysUserId { get; private set; }
        public int? PoKeysIndex { get; private set; }

        public string PoKeysId { get { return string.Format("{0} - {1} ({2})", PoKeysName, PoKeysUserId, PoKeysSerial); } }

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

        public AvailablePoKeys(int serial, string name, byte? userId, int? index)
        {
            PoKeysSerial = serial;
            PoKeysName = name;
            PoKeysUserId = userId;
            PoKeysIndex = index;
        }
    }
}