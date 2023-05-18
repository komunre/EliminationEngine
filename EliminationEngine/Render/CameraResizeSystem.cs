using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace EliminationEngine.Render
{
    public class CameraResizeSystem : EntitySystem
    {
        public bool WindowResized = false;
        protected bool ResizeBlocked = false;
        protected EngineTimer timer = new EngineTimer(TimeSpan.FromMilliseconds(2500));

        public CameraResizeSystem(Elimination e) : base(e)
        {

        }

        public void ChangeTime(TimeSpan span)
        {
            timer.ResetTimer(span);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Engine.window == null) return;

            if (ResizeBlocked)
            {
                if (!timer.TestTimer()) return;
                ResizeBlocked = false;
            }
            if (!WindowResized) return;

            Logger.Info(Loc.Get("INFO_RESIZING"));
            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (!camera.Active) continue;
                camera.ClearTextureTools();
                camera.Width = Engine.window.Size.X;
                camera.Height = Engine.window.Size.Y;
                camera.SetOrthoVisibility(camera.OrthoVisibility);
                camera.RunFullFrameBufferConfig();
            }

            ResizeBlocked = true;
            WindowResized = false;
            timer.ResetTimer();
        }
    }
}