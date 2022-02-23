using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public static OpenTK.Mathematics.Quaternion QuaternionFromEuler(OpenTK.Mathematics.Vector3 vec)
        {
            return OpenTK.Mathematics.Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(vec.X), MathHelper.DegreesToRadians(vec.Y), MathHelper.DegreesToRadians(vec.Z));
        }

        public static Matrix4 NumericsMatrixToMatrix(Matrix4x4 matrix)
        {
            return new Matrix4(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
    }
}
