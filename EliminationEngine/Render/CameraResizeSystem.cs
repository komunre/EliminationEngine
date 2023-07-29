using EliminationEngine.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace EliminationEngine.Render
{
    public class CameraResizeSystem : EntitySystem
    {
        public bool WindowResized = false;
        protected bool ResizeBlocked = false;
        protected EngineTimer timer = new EngineTimer(TimeSpan.FromMilliseconds(500));

        public CameraResizeSystem(Elimination e) : base(e)
        {

        }

        public void ChangeTime(TimeSpan span)
        {
            timer.ResetTimer(span);
        }

        public override void OnWindowResize(ResizeEventArgs args)
        {
            while (!timer.TestTimer())
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }

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