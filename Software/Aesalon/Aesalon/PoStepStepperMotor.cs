using PoKeysDevice_DLL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Aesalon
{
    public class PoStepStepperMotor : BindableObject, IDisposable
    {
        public static readonly float DefaultFalconMinValue = 0.0F;
        public static readonly float DefaultFalconMaxValue = 1.0F;

        #region Construction / Destruction
        static PoStepStepperMotor()
        {
            motorIdList = new List<byte>();
            for (byte motorId = 1; motorId <= 8; ++motorId)
                motorIdList.Add(motorId);
        }
        public PoStepStepperMotor()
        {
            RemoveStepperMotorCommand = new RelayCommand(ExecuteRemoveStepperMotor);
            AddAdditionalPointCommand = new RelayCommand(ExecuteAddAdditionalPoint);
            RemoveAdditionalPointCommand = new RelayCommand(ExecuteRemoveAdditionalPoint, CanExecuteRemoveAdditionalPoint);

            minPoint = DefaultMinPoint();
            maxPoint = DefaultMaxPoint();
            additionalPointList = new ObservableCollection<StepperGaugePoint>();
        }

        public void Dispose()
        {

            if (falconGauge != null)
                falconGauge.FalconGaugeChanged -= OnFalconGaugeChanged;
        }
        #endregion

        #region MotorIdList
        private static readonly List<byte> motorIdList;
        [XmlIgnore]
        public static List<byte> MotorIdList { get { return motorIdList; } }
        #endregion

        #region MotorId
        private byte? motorId;
        public byte? MotorId
        {
            get { return motorId; }
            set
            {
                if (motorId == value)
                    return;
                motorId = value;
                RaisePropertyChanged(() => MotorId);

                UpdateStatus();
            }
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

                //if (falconGauge == null)
                //    FalconGaugeLabel = null;
                //else
                //    FalconGaugeLabel = falconGauge.Label;

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
        private void OnFalconGaugeChanged(object sender, FalconGaugeChangedEventArgs e)
        {
            FalconValue = e.falconValue;
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
                return 0;

            // Clamp to [min, max]
            if (falconValue.Value <= MinPoint.FalconValue)
                return MinPoint.StepperValue;
            if (FalconValue.Value >= MaxPoint.FalconValue)
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

                WriteOutputState();
            }
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
                    foreach (StepperGaugePoint point in e.NewItems.Cast<StepperGaugePoint>())
                        point.PropertyChanged += OnPointChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (StepperGaugePoint point in e.OldItems.Cast<StepperGaugePoint>())
                        point.PropertyChanged -= OnPointChanged;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (StepperGaugePoint point in e.OldItems.Cast<StepperGaugePoint>())
                        point.PropertyChanged -= OnPointChanged;
                    foreach (StepperGaugePoint point in e.NewItems.Cast<StepperGaugePoint>())
                        point.PropertyChanged += OnPointChanged;
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

        #region OnPointChanged
        private void OnPointChanged(object sender, EventArgs e)
        {
            UpdateTargetOutputRange();
            OutputTarget = FalconValueToStepperValue(FalconValue);
        }
        #endregion

        #region UpdateTargetRange
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

        #region Commands
        [XmlIgnore]
        public RelayCommand RemoveStepperMotorCommand { get; private set; }
        private void ExecuteRemoveStepperMotor(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveStepperMotorText, MotorId.HasValue ? MotorId.Value.ToString() : string.Empty),
                Translations.Main.RemoveStepperMotorCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.StepperMotorList.Remove(this);
            Dispose();
        }
        
        [XmlIgnore]
        public RelayCommand AddAdditionalPointCommand { get; private set; }
        private void ExecuteAddAdditionalPoint(object e)
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

        #region Owner
        private PoKeys owner;
        internal void SetOwner(PoKeys poKeys)
        {
            owner = poKeys;
            UpdateStatus();
        }
        #endregion

        #region UpdateStatus
        public void UpdateStatus()
        {
            if (owner == null) 
                return;

            if (!MotorId.HasValue)
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
                    // TODO: Stepper Motor Checks

                    poKeysDevice.DisconnectDevice();
                }
            }

            // TODO: set dirty? like seven segment?
            WriteOutputState();
        }
        #endregion

        #region WriteOutputState
        private void WriteOutputState()
        {
            if (string.IsNullOrEmpty(Error) && owner != null && owner.PoKeysIndex.HasValue && MotorId.HasValue)  // TODO: any other check for stepper here? ref 7seg
            {
                PoKeysDevice poKeysDevice = PoKeysEnumerator.Singleton.PoKeysDevice;

                if (!poKeysDevice.ConnectToDevice(owner.PoKeysIndex.Value))
                {
                    Error = Translations.Main.PoKeysConnectError;
                }
                else
                {
                    poKeysDevice.COM_PEv2_MoveP(MotorId.Value - 1, OutputTarget);

                    poKeysDevice.DisconnectDevice();
                }
            }
        }
        #endregion
    }
}