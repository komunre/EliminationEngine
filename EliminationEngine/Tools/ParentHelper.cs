using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace EliminationEngine.Tools
{
    public static class ParentHelper
    {
        public static Vector3 GetAddedPos(GameObject obj)
        {
            var result = obj.Position;
            if (obj.Parent != null)
            {
                result += GetAddedPos(obj.Parent);
            }
            return result;
        }

        public static Quaternion GetAddedRot(GameObject obj)
        {
            var result = obj.Rotation;
            if (obj.Parent != null)
            {
                result *= GetAddedRot(obj.Parent);
            }
            return result;
        }

        public static Vector3 GetAddedDegreeRot(GameObject obj)
        {
            var result = obj.DegreeRotation;
            if (obj.Parent != null)
            {
                result += GetAddedDegreeRot(obj.Parent);
            }
            return result;
        }

        public static Vector3 GetAddedScale(GameObject obj)
        {
            var result = obj.Scale;
            if (obj.Parent != null)
            {
                result *= GetAddedScale(obj.Parent);
            }
            return result;
        }
    }
}
