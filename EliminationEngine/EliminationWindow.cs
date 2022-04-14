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
using EliminationEngine.Render.UI;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public int WorldCounter = 0;
        public int CurrentWorld = 0;
        public Dictionary<int, List<GameObject>> WorldObjects = new();
        public List<GameObject> GameObjects = new();
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

        public void SwitchWorld(int world)
        {
            if (!WorldObjects.ContainsKey(world))
            {
                Logger.Error("Can not load world (does not exist): " + world);
            }
            CurrentWorld = world;
            GameObjects = WorldObjects[world];
        }

        public void RemoveWorld(int world)
        {
            WorldObjects.Remove(world);
        }

        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            var compsList = new List<CompType>();
            foreach (var obj in GameObjects)
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

            GL.Viewport(0, 0, e.Width, e.Height);

            foreach (var camera in GetObjectsOfType<CameraComponent>())
            {
                camera.Width = e.Width;
                camera.Height = e.Height;
            }

            Engine.GetSystem<GwenSystem>().GwenGui.Resize(new Vector2i(e.Width, e.Height));
        }
    }
}
