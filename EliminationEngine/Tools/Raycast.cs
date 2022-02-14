using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace EliminationEngine.Tools
{
    public class RayHit
    {
        public bool Hit = false;
        public GameObject? HitObject = null;
        public float Distance = 0;
        public Vector3 StartPos = Vector3.Zero;
        public Vector3 EndPos = Vector3.Zero;

        public RayHit(bool hit, GameObject obj, float dist, Vector3 start, Vector3 end)
        {
            Hit = hit;
            HitObject = obj;
            Distance = dist;
            StartPos = start;
            EndPos = end;
        }
    }
    public class Raycast : EntitySystem
    {
        public Raycast(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();


        }


        public List<RayHit> RaycastFromPos(Vector3 pos, Quaternion angle, float maxDist = 1000)
        {
            var localAngle = angle;
            var delta = angle * pos;
            delta.Normalize();
            delta *= -1;

            var hitboxes = Engine.GetObjectsOfType<HitBox>();

            var hits = new List<RayHit>();

            foreach (var hitbox in hitboxes)
            {
                foreach (var box in hitbox.GetBoxes())
                {
                    float txmin = box.Min.X - pos.X / delta.X;
                    float txmax = box.Max.X - pos.X / delta.X;

                    if (txmin > txmax)
                    {
                        var temp = txmin;
                        txmin = txmax;
                        txmax = temp;
                    }

                    float tymin = box.Min.Y - pos.Y / delta.Y;
                    float tymax = box.Max.Y - pos.Y / delta.Y;

                    if (txmin > tymax || tymin > tymax)
                    {
                        continue;
                    }

                    if (tymin > txmin)
                    {
                        txmin = tymin;
                    }

                    if (tymax < txmax)
                    {
                        txmax = tymax;
                    }

                    float tzmin = box.Min.Z - pos.Z / delta.Z;
                    float tzmax = box.Max.Z - pos.Z / delta.Z;

                    if (tzmin > tzmax)
                    {
                        var temp = tzmin;
                        tzmin = tzmax;
                        tzmax = temp;
                    }

                    if ((txmin > tzmax) || (tzmin > txmax))
                        continue;

                    if (tzmin > txmin)
                        txmin = tzmin;

                    if (tzmax < txmax)
                        txmax = tzmax;

                    hits.Add(new RayHit(true, hitbox.Owner, (box.Center - pos).Length, pos, box.Center));
                }

            }

            if (hits.Count < 1)
            {
                return new List<RayHit>() { new RayHit(false, null, 0, pos, Vector3.Zero) };
            }
            return hits;
        }

        public List<RayHit> RaycastFromObject(GameObject obj, float maxDist = 1000)
        {
            return RaycastFromPos(obj.Position, obj.Rotation, maxDist);
        }

        public List<RayHit> RaycastFromCameraCenter(float maxDist = 1000)
        {
            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return new List<RayHit>() { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };
            var camera = cameras.ElementAt(0);
            if (camera == null) return new List<RayHit>() { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };

            return RaycastFromObject(camera.Owner, maxDist);
        }
    }
}
