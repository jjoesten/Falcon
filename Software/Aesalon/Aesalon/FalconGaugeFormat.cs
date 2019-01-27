using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Aesalon
{
    public class FalconGaugeFormat : BindableObject
    {
        public FalconGaugeFormat()
        {
            IncrementTotalSizeCommand = new RelayCommand(ExecuteIncrementTotalSize);
            DecrementTotalSizeCommand = new RelayCommand(ExecuteDecrementTotalSize, CanExecuteDecrementTotalSize);

            UpdateFormat();
        }

        private int totalSize;
        public int TotalSize
        {
            get { return totalSize; }
            set
            {
                if (totalSize == value)
                    return;
                totalSize = value;
                RaisePropertyChanged(nameof(TotalSize));
            }
        }

        private int integralPartMinSize;
        public int IntegralPartMinSize
        {
            get { return integralPartMinSize; }
            set
            {
                if (integralPartMinSize == value)
                    return;
                integralPartMinSize = value;
                RaisePropertyChanged(nameof(IntegralPartMinSize));

                UpdateFormat();
            }
        }

        private int fractionalPartSize;
        public int FractionalPartSize
        {
            get { return fractionalPartSize; }
            set
            {
                if (fractionalPartSize == value)
                    return;
                fractionalPartSize = value;
                RaisePropertyChanged(nameof(FractionalPartSize));

                UpdateFormat();
            }
        }

        private bool padFractionalPartWithZero;
        public bool PadFractionalPartWithZero
        {
            get { return padFractionalPartWithZero; }
            set
            {
                if (padFractionalPartWithZero == value)
                    return;
                padFractionalPartWithZero = value;
                RaisePropertyChanged(nameof(PadFractionalPartWithZero));

                UpdateFormat();
            }
        }

        private string format;
        [XmlIgnore]
        public string Format
        {
            get { return format; }
            set
            {
                if (format == value)
                    return;
                format = value;
                RaisePropertyChanged(nameof(Format));
            }
        }

        #region IncrementTotalSizeCommand

        [XmlIgnore]
        public RelayCommand IncrementTotalSizeCommand { get; private set; }
        private void ExecuteIncrementTotalSize(object o)
        {
            ++TotalSize;
        }

        #endregion

        #region DecrementTotalSizeCommand

        [XmlIgnore]
        public RelayCommand DecrementTotalSizeCommand { get; private set; }
        private void ExecuteDecrementTotalSize(object o)
        {
            --TotalSize;
        }

        private bool CanExecuteDecrementTotalSize(object o)
        {
            return TotalSize > 0;
        }

        #endregion

        private void UpdateFormat()
        {
            StringBuilder builder = new StringBuilder();

            if (IntegralPartMinSize > 0)
                builder.Append('0', IntegralPartMinSize);
            else
                builder.Append('0');

            if (FractionalPartSize > 0)
            {
                builder.Append('.');
                builder.Append(PadFractionalPartWithZero ? '0' : '#', FractionalPartSize);
            }

            Format = builder.ToString();
        }

        public string ToString(float? value)
        {
            if (!value.HasValue)
                return string.Empty;

            return value.Value.ToString(Format, CultureInfo.InvariantCulture);
        }
    }
}