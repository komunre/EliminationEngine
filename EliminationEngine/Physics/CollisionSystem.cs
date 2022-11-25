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

        public bool CheckBoxIntersection(BoxData box1, BoxData box2)
        {
            if ((box1.Bounds.Min.X > box2.Bounds.Min.X && box1.Bounds.Min.Y > box2.Bounds.Min.Y && box1.Bounds.Min.Z > box2.Bounds.Min.Z) && (box1.Bounds.Max.X < box2.Bounds.Max.X && box1.Bounds.Max.Y < box2.Bounds.Max.Y && box1.Bounds.Max.Z < box2.Bounds.Max.Z))
            {
                return true;
            }
            return false;
        }
    }
}
