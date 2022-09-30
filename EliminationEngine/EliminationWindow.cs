using EliminationEngine.GameObjects;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public int WorldCounter = 0;
        public int CurrentWorld = 0;
        public Dictionary<int, List<GameObject>> WorldObjects = new();
        public int MaxObjectId = 0;
        public Dictionary<int, GameObject> GameObjects = new();
        public Elimination Engine;
        protected Stopwatch stopwatch = new();
        public EliminationWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings, Elimination engine) : base(settings, nativeSettings)
        {
            Engine = engine;
            CreateWorld();
        }

        public int CreateWorld()
        {
            WorldObjects.Add(WorldCounter, new List<GameObject>());
            return WorldCounter++;
        }

        public void RemoveWorld(int world)
        {
            WorldObjects.Remove(world);
        }

        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            var compsList = new List<CompType>();
            foreach (var obj in GameObjects.Values)
            {
                if (obj.TryGetComponent<CompType>(out var comp))
                {
                    if (comp != null)
                    {
                        compsList.Add(comp);
                    }
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

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnLoad();
            }

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.PostLoad();
            }
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            stopwatch.Start();

            Engine.KeyState = KeyboardState;

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

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                camera.BindFrameBuffer();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

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

            GL.Viewport(0, 0, Size.X, Size.Y);

            Engine.GetSystem<CameraResizeSystem>().WindowResized = true;
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            foreach (var system in Engine.RegisteredSystems.Values)
            {
                system.OnTextInput(e);
            }
        }
    }
}
