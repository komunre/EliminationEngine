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
using OpenTK.Mathematics;
using System.Diagnostics;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public List<GameObject> GameObjects = new();
        public Elimination Engine;
        protected Stopwatch stopwatch = new();
        public EliminationWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings, Elimination engine) : base(settings, nativeSettings)
        {
            Engine = engine;
        }

        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            var compsList = new List<CompType>();
            foreach (var obj in GameObjects)
            {
                if (obj.TryGetComponent<CompType>(out var comp))
                {
                    compsList.Add(comp);
                }
            }
            return compsList.ToArray();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less); // Doesn't work properly without CullFace? (Draws only back side) // I don't remember
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnLoad();

                system.PostLoad();
            }
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            stopwatch.Start();

            Engine.KeyState = KeyboardState;

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

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnDraw();
            }


            SwapBuffers();

            stopwatch.Stop();

            Engine.Elapsed = Engine.Elapsed.Add(stopwatch.Elapsed);
            Engine.DeltaTime = stopwatch.ElapsedTicks / 10000000f;

            stopwatch.Reset();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            foreach (var camera in GetObjectsOfType<CameraComponent>())
            {
                camera.Width = e.Width;
                camera.Height = e.Height;
            }
        }
    }
}
