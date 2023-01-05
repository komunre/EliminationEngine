using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Extensions
{
    public static class VectorCalc
    {
        public static OpenTK.Mathematics.Vector3 ToOpenTK(this System.Numerics.Vector3 v)
        {
            return new OpenTK.Mathematics.Vector3(v.X, v.Y, v.Z);
        }

        public static OpenTK.Mathematics.Quaternion ToOpenTK(this System.Numerics.Quaternion q)
        {
            return new OpenTK.Mathematics.Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
