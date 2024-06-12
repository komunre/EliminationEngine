using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static EliminationEngine.Tools.Interpolator;

namespace EliminationEngine.Tools
{
    public class SimulatedValue
    {
        protected float _currentValue;
        protected float _desiredValue;

        public float CurrentValue { get; set; }
        public float DesiredValue
        {
            get => _desiredValue; set
            {
                _desiredValue = value;
                if (DesiredValueChanged != null)
                {
                    DesiredValueChanged.Invoke(value);
                }
            }
        }
            

        public event Action<float> DesiredValueChanged;

        private Interpolator.InterpolationProcedure? _interpolationProcedure = null;

        public SimulatedValue()
        {
            CurrentValue = 0;
            DesiredValue = 0;
        }

        public SimulatedValue(float desired)
        {
            DesiredValue = desired;
        }

        public SimulatedValue(float current, float desiredValue) : this(desiredValue)
        {
            CurrentValue = current;
        }

        public void SetDesiredWithInterpolation(float value, TimeSpan span)
        {
            _desiredValue = value;
            InitiateInterpolation(span);
        }

        public void InitiateInterpolation(TimeSpan span)
        {
            _interpolationProcedure = new Interpolator.InterpolationProcedure(CurrentValue, DesiredValue, span);
            Logger.Info("curr: " + CurrentValue + " - desired: " + DesiredValue + " - span: " + span.TotalSeconds);
        }

        public void UpdateInterpolation(Interpolator.InterpolationFunction func)
        {
            if (_interpolationProcedure == null) return;
            if (_interpolationProcedure.GetPercent() >= 1) return;

            CurrentValue = Interpolator.InterpolateWithProcedure(_interpolationProcedure, func);
        }

        public float GetInterpolationPercent()
        {
            if (_interpolationProcedure == null) return 1;
            return _interpolationProcedure.GetPercent();
        }

        public Interpolator.InterpolationProcedure GetInterpolationProcedure()
        {
            return _interpolationProcedure;
        }
    }

    public class SimulatedVectorValue
    {
        protected SimulatedValue X;
        protected SimulatedValue Y;
        protected SimulatedValue Z;

        //public event Action<Vector3> DesiredValueChanged;

        private Interpolator.InterpolationProcedure? _interpolationProcedureX = null;
        private Interpolator.InterpolationProcedure? _interpolationProcedureY = null;
        private Interpolator.InterpolationProcedure? _interpolationProcedureZ = null;

        public void SetCurrent(Vector3 value)
        {
            X.CurrentValue = value.X;
            Y.CurrentValue = value.Y;
            Z.CurrentValue = value.Z;
        }
        public void SetDesired(Vector3 value)
        {
            X.DesiredValue = value.X;
            Y.DesiredValue = value.Y;
            Z.DesiredValue = value.Z;
        }

        public void InitiateInterpolation(TimeSpan span)
        {
            _interpolationProcedureX = new InterpolationProcedure(X.CurrentValue, X.DesiredValue, span);
            _interpolationProcedureY = new InterpolationProcedure(Y.CurrentValue, Y.DesiredValue, span);
            _interpolationProcedureZ = new InterpolationProcedure(Z.CurrentValue, Z.DesiredValue, span);
        }

        public void UpdateInterpolation(Interpolator.InterpolationFunction func)
        {
            if (_interpolationProcedureX == null) return;
            if (_interpolationProcedureX.GetPercent() >= 1) return;

            X.CurrentValue = Interpolator.InterpolateWithProcedure(_interpolationProcedureX, func);
            Y.CurrentValue = Interpolator.InterpolateWithProcedure(_interpolationProcedureY, func);
            Z.CurrentValue = Interpolator.InterpolateWithProcedure(_interpolationProcedureZ, func);
        }

        public float GetInterpolationPercent()
        {
            if (_interpolationProcedureX == null) return 1;
            return _interpolationProcedureX.GetPercent();
        }

        public Interpolator.InterpolationProcedure[] GetInterpolationProcedure()
        {
            return new InterpolationProcedure[3] { _interpolationProcedureX, _interpolationProcedureY, _interpolationProcedureZ };
        }
    }
}
