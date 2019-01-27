namespace Aesalon
{
    public class SevenSegmentDigitSegment : BindableObject
    {
        private bool myValue;
        public bool Value
        {
            get { return myValue; } 
            set
            {
                if (myValue == value)
                    return;
                myValue = value;
                RaisePropertyChanged(nameof(Value));

                Dirty = true;
            }
        }

        private bool dirty = true;
        public bool Dirty
        {
            get { return dirty; }
            set
            {
                if (dirty == value)
                    return;
                dirty = value;
                RaisePropertyChanged(nameof(Dirty));
            }
        }
    }
}