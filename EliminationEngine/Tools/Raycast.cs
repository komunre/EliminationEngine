using EliminationEngine.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EliminationEngine.Tools
{
    public struct RayHit
    {
        public bool Hit = false;
        public GameObject? HitObject = null;
        public float Distance = 0;
        public Vector3 StartPos = Vector3.Zero;
        public Vector3 EndPos = Vector3.Zero;

        public RayHit(bool hit, GameObject? obj, float dist, Vector3 start, Vector3 end)
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


        public RayHit[] RaycastFromPos(Vector3 pos, Vector3 dir, float maxDist = 1000, uint maxHits = 2)
        {
            var debugRender = Engine.GetSystem<DebugRenderSystem>();
            if (debugRender.DebugActive)
            {
                var lineDraw = new float[]
                {
                    pos.X, pos.Y, pos.Z,
                    pos.X + dir.X * maxDist, pos.Y + dir.Y * maxDist, pos.Z + dir.Z * maxDist
                };
                debugRender.AddLine(lineDraw);
            }

            var delta = dir;

            var hitboxes = Engine.GetObjectsOfType<HitBox>();

            var hits = new RayHit[maxHits];
            var hitsCount = 0;

            foreach (var hitbox in hitboxes)
            {
                foreach (var box in hitbox.GetBoxes())
                {
                    float x1;
                    float x2;
                    if (delta.X >= 0)
                    {
                        x1 = box.Bounds.Min.X;
                        x2 = box.Bounds.Max.X;
                    }
                    else
                    {
                        x1 = box.Bounds.Max.X;
                        x2 = box.Bounds.Min.X;
                    }
                    float tmin = (x1 + hitbox.Owner.GlobalPosition.X - pos.X) / delta.X;
                    float tmax = (x2 + hitbox.Owner.GlobalPosition.X - pos.X) / delta.X;

                    var prevx = tmin;

                    float y1;
                    float y2;
                    if (delta.Y >= 0)
                    {
                        y1 = box.Bounds.Min.Y;
                        y2 = box.Bounds.Max.Y;
                    }
                    else
                    {
                        y1 = box.Bounds.Max.Y;
                        y2 = box.Bounds.Min.Y;
                    }

                    float tymin = (y1 + hitbox.Owner.GlobalPosition.Y - pos.Y) / delta.Y;
                    float tymax = (y2 + hitbox.Owner.GlobalPosition.Y - pos.Y) / delta.Y;

                    if ((tmin > tymax) || (tymin > tmax))
                        continue;
                    if (tymin > tmin)
                        tmin = tymin;
                    if (tymax < tmax)
                        tmax = tymax;

                    float z1;
                    float z2;
                    if (delta.Z >= 0)
                    {
                        z1 = box.Bounds.Min.Z;
                        z2 = box.Bounds.Max.Z;
                    }
                    else
                    {
                        z1 = box.Bounds.Max.Z;
                        z2 = box.Bounds.Min.Z;
                    }

                    float tzmin = (z1 + hitbox.Owner.GlobalPosition.Z - pos.Z) / delta.Z;
                    float tzmax = (z2 + hitbox.Owner.GlobalPosition.Z - pos.Z) / delta.Z;

                    if ((tmin > tzmax) || (tzmin > tmax))
                        continue;
                    if (tzmin > tmin)
                        tmin = tzmin;
                    if (tzmax < tmax)
                        tmax = tzmax;



                    var hitPos = pos + delta * tmin;
                    hits[hitsCount] = new RayHit(true, hitbox.Owner, tmin, pos, hitPos);
                    hitsCount++;
                    if (hitsCount >= maxHits)
                    {
                        return hits;
                    }
                }

            }

            if (hitsCount < 1)
            {
                return new RayHit[] { new RayHit(false, null, 0, pos, Vector3.Zero) };
            }
            return hits;
        }

        public RayHit[] RaycastFromObject(GameObject obj, float maxDist = 1000)
        {
            var dir = obj.GetDirections()[0];
            //dir.Y = dir.Y / 2;
            //dir.X = dir.X / 2;
            return RaycastFromPos(obj.GlobalPosition, dir, maxDist);
        }

        public RayHit[] RaycastFromCameraCenter(float maxDist = 1000)
        {
            var cameras = Engine.GetObjectsOfType<CameraComponent>().Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return new RayHit[] { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };
            var camera = cameras.ElementAt(0);
            if (camera == null) return new RayHit[] { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };

            return RaycastFromObject(camera.Owner, maxDist);
        }

        public RayHit[] RaycastFromCameraCursor(float maxDist = 1000)
        {
            if (Engine.Headless)
            {
                Logger.Warn(Loc.Get("WARN_HEADLESS_CAMERA_RAYCAST"));
            }
            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return new RayHit[] { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };
            var camera = cameras.ElementAt(0);
            if (camera == null) return new RayHit[] { new RayHit(false, null, 0, Vector3.Zero, Vector3.Zero) };

            var camPos = camera.Owner.Position;
            var directions = camera.Owner.GetDirections();
            var forward = directions[0];
            var up = directions[2];

            var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FoV), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
            var lookAt = Matrix4.LookAt(camera.Owner.GlobalPosition, forward, up);
            var viewMatrix = lookAt * (fovMatrix);
            var mousePos = Engine.MouseState.Position;
            var newMousePos = mousePos;
            newMousePos.X = 2f * mousePos.X / Engine.window.Size.X - 1;
            newMousePos.Y = 2f * mousePos.Y / Engine.window.Size.Y - 1;
            newMousePos.Y *= -1;
            var coords = new Vector4(newMousePos.X, newMousePos.Y, -1, 1);

            fovMatrix.Invert();
            var eyeCoords = fovMatrix * coords;
            var eyeCoords2 = new Vector4(eyeCoords.X, eyeCoords.Y, -1, 1);

            lookAt.Invert();

            var rayWorld = lookAt * eyeCoords2;
            var mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);

            mouseRay.Normalize();

            return RaycastFromPos(camera.Owner.GlobalPosition, mouseRay);
        }
    }
}
