using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    public class EliminationMathHelper
    {
        /// <summary>
        /// Converts angles to quaternion
        /// </summary>
        /// <param name="vec">Unnormalized angles</param>
        /// <returns>New quaternion</returns>
        public static Quaternion QuaternionFromEuler(Vector3 vec)
        {
            return Quaternion.FromEulerAngles(MathHelper.NormalizeAngle(vec.X), MathHelper.NormalizeAngle(vec.Y), MathHelper.NormalizeAngle(vec.Z));
        }
    }
}
