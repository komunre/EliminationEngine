using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using EliminationEngine.GameObjects;
using System.Globalization;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public List<GameObject> GameObjects = new();
        public Elimination Engine;
        public EliminationWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings, Elimination engine) : base(settings, nativeSettings)
        {
            Engine = engine;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnLoad();
            }
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnUpdate();
            }
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            foreach (var gameObject in GameObjects)
            {
                if (gameObject.TryGetComponent<Mesh>(out var mesh))
                {
                    mesh.UpdatePos();
                    mesh.DrawMesh();
                }
            }


            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
