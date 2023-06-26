using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    public static class Interpolator
    {
        public delegate float InterpolationFunction(float value1, float value2, float position);

        public static InterpolationProcedure InitiateInterpolation(float value1, float value2, TimeSpan span)
        {
            return new InterpolationProcedure(value1, value2, span);
        }

        public static float InterpolateWithProceduure(InterpolationProcedure procedure, InterpolationFunction func)
        {
            return func(procedure.GetStartValue(), procedure.GetEndValue(), procedure.GetPercent());
        }
        public static float InterpolateWithTimer(float value1, float value2, EngineTimer timer, InterpolationFunction func)
        {
            return func(value1, value2, timer.GetPercent());
        }

        public static float Interpolate(float value1, float value2, float position, InterpolationFunction func)
        {
            return func(value1, value2, position);
        }

        public static float LinearInterpolation(float value1, float value2, float position)
        {
            return value2 * ((value2 - value1) * position);
        }

        public class InterpolationProcedure
        {
            private float _startValue;
            private float _endValue;
            private EngineTimer _timer;

            public InterpolationProcedure(float  startValue, float endValue, TimeSpan span)
            {
                _startValue = startValue;
                _endValue = endValue;
                _timer = new EngineTimer(span);
            }

            public void Reset()
            {
                _timer.ResetTimer();
            }

            public float GetStartValue()
            {
                return _startValue;
            }

            public float GetEndValue()
            {
                return _endValue;
            }

            public float GetPercent()
            {
                return _timer.GetPercent();
            }
        }
    }
}
