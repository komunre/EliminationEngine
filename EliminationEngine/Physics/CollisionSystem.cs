using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace EliminationEngine.Physics
{
    public class CollisionSystem : EntitySystem
    {
        public CollisionSystem(Elimination e) : base(e)
        {
        }

        public List<GameObject> ObjectsInSphere(Vector3 spherePos, float sphereRadius)
        {
            var list = new List<GameObject>();
            foreach (var obj in Engine.GetObjectsOfType<HitBox>())
            {
                if ((obj.Owner.Position - spherePos).Length < sphereRadius * sphereRadius)
                {
                    list.Add(obj.Owner);
                }
            }
            return list;
        }
    }
}
