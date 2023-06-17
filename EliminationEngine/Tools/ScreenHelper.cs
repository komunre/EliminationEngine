using EliminationEngine.GameObjects;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace EliminationEngine.Tools
{
    public static class ScreenHelper
    {
        public static Vector3 GetCursorWorldPos(CameraComponent camera, Vector2 mousePos, Vector2 windowSize)
        {
            var camPos = camera.Owner.Position;
            var directions = camera.Owner.GetDirections();
            var forward = directions[0];
            var up = directions[2];
            var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FoV), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
            var lookAt = Matrix4.LookAt(camera.Owner.GlobalPosition, forward, up);
            var viewMatrix = lookAt * (fovMatrix);
            var newMousePos = mousePos;
            newMousePos.X = 2f * mousePos.X / windowSize.X - 1;
            newMousePos.Y = 2f * mousePos.Y / windowSize.Y - 1;
            newMousePos.Y *= -1;
            var coords = new Vector4(newMousePos.X, newMousePos.Y, -1, 1);
            fovMatrix.Invert();
            var eyeCoords = fovMatrix * coords;
            var eyeCoords2 = new Vector4(eyeCoords.X, eyeCoords.Y, -1, 1);
            lookAt.Invert();

            var rayWorld = lookAt * eyeCoords2;
            var mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);

            mouseRay.Normalize();

            return mouseRay;
        }
    }
}
