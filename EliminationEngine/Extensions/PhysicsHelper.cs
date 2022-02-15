using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;
using BepuPhysics;
using System.Numerics;

namespace EliminationEngine.Extensions
{
    public static class PhysicsHelper
    {
        public static System.Numerics.Vector3 ToNumerics(this OpenTK.Mathematics.Vector3 vec)
        {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static System.Numerics.Quaternion ToNumerics(this OpenTK.Mathematics.Quaternion quaternion)
        {
            return new System.Numerics.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
    }
}
