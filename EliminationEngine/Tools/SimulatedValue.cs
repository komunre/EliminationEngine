using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    public class SimulatedValue
    {
        private float _currentValue;
        private float _desiredValue;

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
        }

        public void UpdateInterpolation(Interpolator.InterpolationFunction func)
        {
            if (_interpolationProcedure == null) return;
            if (_interpolationProcedure.GetPercent() >= 1) return;

            CurrentValue = Interpolator.InterpolateWithProcedure(_interpolationProcedure, func);
        }

        public Interpolator.InterpolationProcedure GetInterpolationProcedure()
        {
            return _interpolationProcedure;
        }
    }
}
