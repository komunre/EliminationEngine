using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace EliminationEngine.Render
{
    public class CameraResizeSystem : EntitySystem
    {
        public CameraResizeSystem(Elimination e) : base(e)
        {

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Engine.window == null) return;

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (camera.Active && camera.Width != Engine.window.Size.X && camera.Height != Engine.window.Size.Y)
                {
                    Engine.window.Size = new Vector2i(camera.Width, camera.Height);
                }
            }
        }
    }
}