using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Aesalon
{
    public class ArduinoGaugeStepperMotor : BindableObject, IDisposable
    {
        public float DefaultFalconMinValue => 0.0F;
        public float DefaultFalconMaxValue => 1.0F;

        #region Construction
        public ArduinoGaugeStepperMotor()
        {
            RemoveStepperMotorCommand = new RelayCommand(ExecuteRemoveStepperMotor, CanExecuteRemoveStepperMotor);
            AddAdditionalPointCommand = new RelayCommand(ExecuteAddAdditionalPoint);
            RemoveAdditionalPointCommand = new RelayCommand(ExecuteRemoveAdditionalPoint, CanExecuteRemoveAdditionalPoint);

            minPoint = DefaultMinPoint();
            maxPoint = DefaultMaxPoint();
            AdditionalPointList = new ObservableCollection<StepperGaugePoint>();
        }

        public void Dispose()
        {
            if (falconGauge != null)
                falconGauge.FalconGaugeChanged -= OnFalconGaugeChanged;
        }
        #endregion

        #region Owner
        private ArduinoGauge owner;
        public void SetOwner(ArduinoGauge arduinoGauge)
        {
            owner = arduinoGauge;
            UpdateStatus();
            OnPointChanged(this, null);
        }
        #endregion

        #region FalconGauge
        private FalconGauge falconGauge;
        [XmlIgnore]
        public FalconGauge FalconGauge
        {
            get { return falconGauge; }
            set
            {
                if (falconGauge == value)
                    return;
                if (falconGauge != null)
                    falconGauge.FalconGaugeChanged -= OnFalconGaugeChanged;
                falconGauge = value;
                if (falconGauge != null)
                    falconGauge.FalconGaugeChanged += OnFalconGaugeChanged;
                RaisePropertyChanged(() => FalconGauge);

                if (falconGauge == null)
                    FalconGaugeLabel = null;
                else
                    FalconGaugeLabel = falconGauge.Label;

                ResetFalconValue();

                if (falconGauge != null)
                {
                    MinPoint.FalconValue = falconGauge.MinValue;
                    MaxPoint.FalconValue = falconGauge.MaxValue;
                }
                else
                {
                    MinPoint.FalconValue = DefaultFalconMinValue;
                    MaxPoint.FalconValue = DefaultFalconMaxValue;
                }

                AdditionalPointList.Clear();
            }
        }
        #endregion

        #region FalconGaugeLabel
        private string falconGaugeLabel;
        public string FalconGaugeLabel
        {
            get { return falconGaugeLabel; }
            set
            {
                if (falconGaugeLabel == value)
                    return;
                falconGaugeLabel = value;
                RaisePropertyChanged(() => FalconGaugeLabel);

                if (string.IsNullOrEmpty(FalconGaugeLabel))
                    FalconGauge = null;
                else
                    FalconGauge = FalconConnector.Singleton.GaugeList.FirstOrDefault(item => item.Label == falconGaugeLabel);
            }
        }
        #endregion

        #region FalconValue
        private float? falconValue;
        [XmlIgnore]
        public float? FalconValue
        {
            get { return falconValue; }
            set
            {
                if (falconValue == value)
                    return;
                falconValue = value;
                RaisePropertyChanged(() => FalconValue);

                OutputTarget = FalconValueToStepperValue(FalconValue);
            }
        }

        private void ResetFalconValue()
        {
            FalconValue = null;
        }
        #endregion

        #region FalconValueToStepperValue
        private ushort FalconValueToStepperValue(float? falconValue)
        {
            if (!falconValue.HasValue)
                return -0;

            // Clamp value to [min, max]
            if (falconValue.Value <= MinPoint.FalconValue)
                return MinPoint.StepperValue;
            if (falconValue.Value >= MaxPoint.FalconValue)
                return MaxPoint.StepperValue;

            // Find the segment that contains falconValue
            StepperGaugePoint previousPoint = MinPoint;
            StepperGaugePoint nextPoint = null;

            foreach (StepperGaugePoint additionalPoint in AdditionalPointList)
            {
                if (falconValue.Value >= additionalPoint.FalconValue)
                    previousPoint = additionalPoint;
                else
                {
                    nextPoint = additionalPoint;
                    break;
                }
            }

            if (nextPoint == null)
                nextPoint = MaxPoint;

            // Interpolate in segment
            float normalizedValue = 0.5F;
            if (previousPoint.FalconValue != nextPoint.FalconValue)
                normalizedValue = (falconValue.Value - previousPoint.FalconValue) / (nextPoint.FalconValue - previousPoint.FalconValue);
            return (ushort)(normalizedValue * (nextPoint.StepperValue - previousPoint.StepperValue) + previousPoint.StepperValue);
        }
        #endregion

        #region MinPoint
        private StepperGaugePoint minPoint;
        public StepperGaugePoint MinPoint
        {
            get { return minPoint; }
            set
            {
                if (minPoint == value)
                    return;
                if (minPoint != null)
                    minPoint.PropertyChanged -= OnPointChanged;
                minPoint = value;
                if (minPoint != null)
                    minPoint.PropertyChanged += OnPointChanged;
                RaisePropertyChanged(() => MinPoint);
                RaisePropertyChanged(() => Points);
                OnPointChanged(this, null);
            }
        }

        private StepperGaugePoint DefaultMinPoint()
        {
            StepperGaugePoint point = new StepperGaugePoint() { FalconValue = DefaultFalconMinValue, StepperValue = 0 };
            point.PropertyChanged += OnPointChanged;
            return point;
        }
        #endregion

        #region MaxPoint
        private StepperGaugePoint maxPoint;
        public StepperGaugePoint MaxPoint
        {
            get { return maxPoint; }
            set
            {
                if (maxPoint == value)
                    return;
                if (maxPoint != null)
                    maxPoint.PropertyChanged -= OnPointChanged;
                maxPoint = value;
                if (maxPoint != null)
                    maxPoint.PropertyChanged += OnPointChanged;
                RaisePropertyChanged(() => MaxPoint);
                RaisePropertyChanged(() => Points);
                OnPointChanged(this, null);
            }
        }

        private StepperGaugePoint DefaultMaxPoint()
        {
            StepperGaugePoint point = new StepperGaugePoint() { FalconValue = DefaultFalconMaxValue, StepperValue = (315 * 3) };
            point.PropertyChanged += OnPointChanged;
            return point;
        }
        #endregion

        #region AdditionalPointsList
        private ObservableCollection<StepperGaugePoint> additionalPointList;
        public ObservableCollection<StepperGaugePoint> AdditionalPointList
        {
            get { return additionalPointList; }
            set
            {
                if (additionalPointList != null)
                {
                    additionalPointList.CollectionChanged -= OnAdditionalPointListChanged;
                    foreach (StepperGaugePoint additionalPoint in additionalPointList)
                        additionalPoint.PropertyChanged -= OnPointChanged;
                }

                additionalPointList = value;

                if (additionalPointList != null)
                {
                    additionalPointList.CollectionChanged += OnAdditionalPointListChanged;
                    foreach (StepperGaugePoint additionalPoint in additionalPointList)
                        additionalPoint.PropertyChanged += OnPointChanged;
                }

                RaisePropertyChanged(() => AdditionalPointList);
                RaisePropertyChanged(() => Points);
                OnPointChanged(this, null);
            }
        }

        private void OnAdditionalPointListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (StepperGaugePoint additionalPoint in e.NewItems.Cast<StepperGaugePoint>())
                        additionalPoint.PropertyChanged += OnPointChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (StepperGaugePoint additionalPoint in e.OldItems.Cast<StepperGaugePoint>())
                        additionalPoint.PropertyChanged -= OnPointChanged;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (StepperGaugePoint additionalPoint in e.OldItems.Cast<StepperGaugePoint>())
                        additionalPoint.PropertyChanged -= OnPointChanged;
                    foreach (StepperGaugePoint additionalPoint in e.NewItems.Cast<StepperGaugePoint>())
                        additionalPoint.PropertyChanged += OnPointChanged;
                    break;
            }

            RaisePropertyChanged(() => Points);
            OnPointChanged(sender, e);
        }
        #endregion

        #region Points
        public IEnumerable<StepperGaugePoint> Points
        {
            get
            {
                yield return MinPoint;

                foreach (StepperGaugePoint additionalPoint in AdditionalPointList)
                    yield return additionalPoint;

                yield return MaxPoint;
            }
        }
        #endregion

        #region StepperMotorId
        private byte? stepperMotorId;
        public byte? StepperMotorId
        {
            get { return stepperMotorId; }
            set
            {
                if (stepperMotorId == value)
                    return;
                stepperMotorId = value;
                RaisePropertyChanged(() => StepperMotorId);

                UpdateStatus();
            }
        }

        private IEnumerable<byte> StepperMotorIdAsEnumerable()
        {
            if (StepperMotorId.HasValue)
                yield return StepperMotorId.Value;
        }
        #endregion

        #region StepperMotorIdList
        private List<byte> stepperMotorIdList;
        [XmlIgnore]
        public List<byte> StepperMotorIdList
        {
            get { return stepperMotorIdList; }
            set
            {
                stepperMotorIdList = value;
                RaisePropertyChanged(() => StepperMotorIdList);
            }
        }

        private void RefreshStepperMotorIdList()
        {
            StepperMotorIdList = AvailableStepperMotorIdList()
                .Union(StepperMotorIdAsEnumerable())
                .OrderBy(stepperMotorId => stepperMotorId)
                .ToList();
        }

        private IEnumerable<byte> AvailableStepperMotorIdList()
        {
            if (owner == null || owner.Device == null)
                yield break;

            for (byte availableStepperMotorId = 0; availableStepperMotorId < owner.Device.StepperMotorCount; availableStepperMotorId++)
                yield return availableStepperMotorId;
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
                RaisePropertyChanged(() => Error);
            }
        }
        #endregion

        #region OnFalconGaugeChanged
        private void OnFalconGaugeChanged(object sender, FalconGaugeChangedEventArgs e)
        {
            FalconValue = e.falconValue;
        }
        #endregion

        #region OnPointChanged
        private void OnPointChanged(object sender, EventArgs e)
        {
            UpdateTargetOutputRange();
            OutputTarget = FalconValueToStepperValue(FalconValue);
        }
        #endregion

        #region OutputTarget
        private ushort outputTarget;
        [XmlIgnore]
        public ushort OutputTarget
        {
            get { return outputTarget; }
            set
            {
                if (outputTarget == value)
                    return;
                outputTarget = value;
                RaisePropertyChanged(() => OutputTarget);

                UpdateOutputTarget();
            }
        }

        /// <summary>
        /// Sends the output target value to the stepper motor controller.
        /// </summary>
        private void UpdateOutputTarget()
        {
            if (string.IsNullOrEmpty(Error) && owner != null && owner.Device != null && StepperMotorId.HasValue)
            {
                try
                {
                    owner.Device.SetTarget(StepperMotorId.Value, OutputTarget);
                }
                catch (Exception e)
                {
                    Error = e.Message;
                }
            }
        }
        #endregion

        #region OutputTargetRange
        [XmlIgnore]
        public ushort MinStepperMotorValue { get; set; }
        [XmlIgnore]
        public ushort MaxStepperMotorValue { get; set; }
        [XmlIgnore]
        public double TickFrequency { get { return (MaxStepperMotorValue - MinStepperMotorValue) / 10; } }

        private void UpdateTargetOutputRange()
        {
            MinStepperMotorValue = MinPoint.StepperValue;
            MaxStepperMotorValue = MaxPoint.StepperValue;

            foreach (StepperGaugePoint point in Points)
            {
                if (point.StepperValue < MinStepperMotorValue)
                    MinStepperMotorValue = point.StepperValue;
                if (point.StepperValue > MaxStepperMotorValue)
                    MaxStepperMotorValue = point.StepperValue;
            }

            RaisePropertyChanged(() => MinStepperMotorValue);
            RaisePropertyChanged(() => MaxStepperMotorValue);
            RaisePropertyChanged(() => TickFrequency);
        }
        #endregion

        #region UpdateStatus
        public void UpdateStatus()
        {
            RefreshStepperMotorIdList();

            if (owner == null)
                return;

            if (!StepperMotorId.HasValue)
                Error = null;
            else if (owner.Device == null)
                Error = null;
            else
            {
                try
                {
                    // NOTE: Here we request the status of each stepper motor from the device.
                    // In PololuMaestroServo it doesn't look like we do anything with the result, we are just checking that the device response sucessfully.
                    Error = null;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                }
            }

            UpdateOutputTarget();
        }
        #endregion

        #region Commands
        [XmlIgnore]
        public RelayCommand AddAdditionalPointCommand { get; private set; }
        private void ExecuteAddAdditionalPoint(object o)
        {
            StepperGaugePoint previousPoint = AdditionalPointList.LastOrDefault() ?? MinPoint;
            StepperGaugePoint nextPoint = MaxPoint;
            StepperGaugePoint additionalPoint = new StepperGaugePoint()
            {
                FalconValue = (previousPoint.FalconValue + nextPoint.FalconValue) / 2,
                StepperValue = (ushort)((previousPoint.StepperValue + nextPoint.StepperValue) / 2)
            };
        }

        [XmlIgnore]
        public RelayCommand RemoveAdditionalPointCommand { get; private set; }
        private void ExecuteRemoveAdditionalPoint(object o)
        {
            StepperGaugePoint additionalPoint = (StepperGaugePoint)o;
            AdditionalPointList.Remove(additionalPoint);
        }
        private bool CanExecuteRemoveAdditionalPoint(object o)
        {
            StepperGaugePoint additionalPoint = (StepperGaugePoint)o;
            return AdditionalPointList.Contains(additionalPoint);
        }

        [XmlIgnore]
        public RelayCommand RemoveStepperMotorCommand { get; private set; }
        private void ExecuteRemoveStepperMotor(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveStepperMotorText, StepperMotorId),
                Translations.Main.RemoveStepperMotorCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.StepperMotorList.Remove(this);
            Dispose();
        }
        private bool CanExecuteRemoveStepperMotor(object o)
        {
            return owner.StepperMotorList.Contains(this);
        }
        #endregion
    }
}