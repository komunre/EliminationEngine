using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    public class SimulatedValue
    {
        public float CurrentValue;
        public float DesiredValue;

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
    }
}
