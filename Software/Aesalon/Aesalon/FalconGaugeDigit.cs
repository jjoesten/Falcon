using System.Collections.Generic;
using System.Linq;

namespace Aesalon
{
    public class FalconGaugeDigit : BindableObject
    {
        private readonly SevenSegmentDisplay sevenSegmentDisplay;
        private readonly int position;

        private static readonly List<string> availableIndexList;
        public static List<string> AvailableIndexList { get { return availableIndexList; } }

        static FalconGaugeDigit()
        {
            availableIndexList = new List<string>();
            availableIndexList.Add(string.Empty);
            availableIndexList.AddRange(Enumerable.Range(1, 8).Select(i => i.ToString()));
        }

        public FalconGaugeDigit(SevenSegmentDisplay sevenSegmentDisplay, int position)
        {
            this.sevenSegmentDisplay = sevenSegmentDisplay;
            this.position = position;
        }

        private char myValue;
        public char Value
        {
            get { return myValue; }
            set
            {
                if (myValue == value)
                    return;
                myValue = value;
                RaisePropertyChanged(nameof(Value));

                if (SevenSegmentDigit != null)
                    SevenSegmentDigit.Value = value;
            }
        }

        private bool decimalPoint;
        public bool DecimalPoint
        {
            get { return decimalPoint; }
            set
            {
                if (decimalPoint == value)
                    return;
                decimalPoint = value;
                RaisePropertyChanged(nameof(DecimalPoint));

                if (SevenSegmentDigit != null)
                    SevenSegmentDigit.SegmentDP.Value = value;
            }
        }

        private SevenSegmentDigit sevenSegmentDigit;
        public SevenSegmentDigit SevenSegmentDigit
        {
            get { return sevenSegmentDigit; }
            set
            {
                if (sevenSegmentDigit == value)
                    return;
                if (sevenSegmentDigit != null)
                    sevenSegmentDisplay.RemoveSevenSegmentDigit(sevenSegmentDigit);
                sevenSegmentDigit = value;
                RaisePropertyChanged(nameof(SevenSegmentDigit));

                if (sevenSegmentDigit == null)
                {
                    SevenSegmentDigitIndex = null;
                }
                else
                {
                    SevenSegmentDigitIndex = sevenSegmentDigit.Index;
                    SevenSegmentDigit.Value = Value;
                    SevenSegmentDigit.SegmentDP.Value = DecimalPoint;
                }
            }
        }

        private byte? sevenSegmentDigitIndex;
        public byte? SevenSegmentDigitIndex
        {
            get { return sevenSegmentDigitIndex; }
            set
            {
                if (sevenSegmentDigitIndex == value)
                    return;
                sevenSegmentDigitIndex = value;
                RaisePropertyChanged(nameof(SevenSegmentDigitIndex));

                if (sevenSegmentDigitIndex.HasValue)
                {
                    RemoveOtherSevenSegmentDigit(sevenSegmentDigitIndex.Value);

                    if (SevenSegmentDigit == null || SevenSegmentDigit.Index != sevenSegmentDigitIndex.Value)
                    {
                        SevenSegmentDigit = new SevenSegmentDigit() { Index = sevenSegmentDigitIndex.Value, Position = position };
                        sevenSegmentDisplay.AddSevenSegmentDigit(SevenSegmentDigit);
                    }
                }
                else
                {
                    SevenSegmentDigit = null;
                }
            }
        }

        private void RemoveOtherSevenSegmentDigit(byte index)
        {
            foreach (FalconGaugeDigit falconGaugeDigit in sevenSegmentDisplay.FalconGaugeDigits.Where(digit => digit != this && digit.SevenSegmentDigitIndex == index))
                falconGaugeDigit.SevenSegmentDigitIndex = null;

            foreach (SevenSegmentDigit oldSevenSegmentDigit in sevenSegmentDisplay.SevenSegmentDigits.Where(digit => digit.Index == index && digit.Position != position).ToList())
                sevenSegmentDisplay.RemoveSevenSegmentDigit(oldSevenSegmentDigit);
        }
    }
}