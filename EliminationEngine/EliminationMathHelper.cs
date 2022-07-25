using OpenTK.Mathematics;

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
            var better = vec;
            return Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(better.X), MathHelper.DegreesToRadians(better.Y), MathHelper.DegreesToRadians(better.Z));
        }
        public static float DegreeCos(float value)
        {
            return (float)MathHelper.RadiansToDegrees(MathHelper.Cos(MathHelper.DegreesToRadians(value)));
        }
        public static float DegreeSin(float value)
        {
            return (float)MathHelper.RadiansToDegrees(MathHelper.Sin(MathHelper.DegreesToRadians(value)));
        }

        public static Matrix4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = (target - eye).Normalized();
            Vector3 xAxis = Vector3.Cross(zAxis, up).Normalized();
            Vector3 yAxis = Vector3.Cross(xAxis, zAxis);

            zAxis = -zAxis;

            Matrix4 mat = new Matrix4(
                new Vector4(xAxis.X, xAxis.Y, xAxis.Z, -Vector3.Dot(xAxis, eye)),
                new Vector4(yAxis.X, yAxis.Y, yAxis.Z, -Vector3.Dot(yAxis, eye)),
                new Vector4(zAxis.X, zAxis.Y, zAxis.Z, -Vector3.Dot(zAxis, eye)),
                new Vector4(0, 0, 0, 1)
            );

            return mat;
        }
    }
}
