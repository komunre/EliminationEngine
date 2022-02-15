
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System.Numerics;

namespace EliminationEngine.Physics
{
    public class RayHitData {
        public Vector3 OriginPos;
        public Vector3 HitPos;
        public float Distance;
    }
    public class DefaultRayHitHandler : IRayHitHandler
    {
        public List<RayHitData> Hits = new();
        public bool AllowTest(CollidableReference collidable)
        {
            return true;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return true;
        }

        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {
            var data = new RayHitData();
            data.Distance = t;
            data.OriginPos = ray.Origin;
            data.HitPos = ray.Origin + ray.Direction * t;
            Hits.Add(data);
        }
    }
}
