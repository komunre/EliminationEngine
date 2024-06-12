using EliminationEngine.GameObjects;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using EliminationEngine.Tools;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public int WorldCounter = 0;
        public int CurrentWorld = 0;
        public Dictionary<int, Dictionary<int, GameObject>> WorldObjects = new();
        public int MaxObjectId = 0;
        public Dictionary<int, GameObject> GameObjects = new();
        public Elimination Engine;
        protected Stopwatch stopwatch = new();
        public Color ClearColor = new Color(0, 0, 0, 1);

        public EliminationWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings, Elimination engine) : base(settings, nativeSettings)
        {
            Engine = engine;
            CreateWorld();
        }

        public int CreateWorld()
        {
            WorldObjects.Add(WorldCounter, new Dictionary<int, GameObject>());
            return WorldCounter++;
        }

        public void SwitchWorld(int world)
        {
            WorldObjects[CurrentWorld] = GameObjects;
            GameObjects = WorldObjects[world];
        }

        public void RemoveWorld(int world)
        {
            WorldObjects.Remove(world);
        }

        public void DeleteAllObjects()
        {
            GameObjects.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowClearColor()
        {
            GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected override void OnLoad()
        {
            base.OnLoad();

            SetWindowClearColor();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            try
            {
                stopwatch.Start();

                Engine.KeyState = KeyboardState;

                foreach (var system in Engine.RegisteredSystems.Values)
                {
                    system.OnUpdate();
                }

                base.OnUpdateFrame(args);
            }
            catch (Exception ex)
            {
                Logger.Error("[!!!] EXCEPTION IN UPDATE: \n=---------------------=");
                Logger.Error(ex.Message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            try
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

                Engine.GetSystem<ImGuiSystem>().Render();


                SwapBuffers();

                stopwatch.Stop();

                Engine.Elapsed = Engine.Elapsed.Add(stopwatch.Elapsed);
                Engine.DeltaTime = (float)stopwatch.Elapsed.TotalMilliseconds / 1000f;

                stopwatch.Reset();
            }
            catch (Exception ex)
            {
                Logger.Error("[!!!] EXCEPTION IN RENDER: \n=---------------------=");
                Logger.Error(ex.Message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);

            Engine.GetSystem<CameraResizeSystem>().WindowResized = true;
            Engine.GetSystem<ImGuiSystem>().WindowResized();

            foreach (var system in Engine.GetAllSystems())
            {
                system.OnWindowResize(e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
