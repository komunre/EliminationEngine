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
    }
}
