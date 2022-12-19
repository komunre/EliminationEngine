using EliminationEngine.GameObjects;
using EliminationEngine.Network;
using EliminationEngine.Physics;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using EliminationEngine.Systems;
using EliminationEngine.Systems.Tiles;
using EliminationEngine.Tools;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace EliminationEngine
{
    public class Elimination
    {
        public string Title { get; set; } = "Elimination";
        public EliminationWindow? window = null;
        public Dictionary<Type, EntitySystem> RegisteredSystems = new();
        public float DeltaTime = 0;
        public TimeSpan Elapsed = new TimeSpan(0);
        public KeyboardState? KeyState;
        public MouseState? MouseState;

        public delegate void ObjectCreateEvent(GameObject obj, int world);
        public event ObjectCreateEvent? OnObjectCreate;

        public string[] ProgramArgs = new string[0];
        public bool Headless = false;
        public bool IsRunning = false;

        public bool DefaultBorderless = true;

        private EngineTimer _cursorToggleTimer = new EngineTimer(TimeSpan.FromSeconds(1)); 

        public Elimination(string[] args)
        {
            ProgramArgs = args;
            RegisterSystem<EngineStaticsInitSystem>();
            RegisterSystem<MeshSystem>();
            RegisterSystem<SoundSystem>();
            RegisterSystem<Raycast>();
            RegisterSystem<CollisionSystem>();
            RegisterSystem<RemovalSystem>();
            RegisterSystem<ImGuiSystem>();
            RegisterSystem<CameraResizeSystem>();
            RegisterSystem<DebugRenderSystem>();
            RegisterSystem<EditorSystem>();
            RegisterSystem<NetworkManager>();
            RegisterSystem<TileSystem>();
        }

        public void SetClearColor(Tools.Color color)
        {
            color.ConvertToFloat();
            window.ClearColor = color;
            window.SetWindowClearColor();
        }

        public void Run()
        {
            IsRunning = true;
            var settings = new GameWindowSettings();
            //settings.IsMultiThreaded = true;
            var native = new NativeWindowSettings();
            native.Size = new OpenTK.Mathematics.Vector2i(800, 800);
            native.Title = Title;
            native.Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible;
            window = new EliminationWindow(settings, native, this);
            if (Headless)
            {
                window.IsVisible = false;
            }
            KeyState = window.KeyboardState;
            MouseState = window.MouseState;
            if (window == null) throw new InvalidDataException("No window was created.");
            window.Run();

            window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Crosshair;
        }

        public void AddGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Error("Engine not started. Aborted adding gameobject.");
                return;
            }
            if (obj.TryGetComponent<MeshGroupComponent>(out var comp))
            {
                var meshSys = GetSystem<MeshSystem>();
                meshSys?.LoadMeshGroup(comp);
            }
            while (window.GameObjects.ContainsKey(window.MaxObjectId))
            {
                window.MaxObjectId++;
            }
            obj.Id = window.MaxObjectId;
            window.GameObjects.Add(window.MaxObjectId, obj);
            window.MaxObjectId++;
            if (OnObjectCreate != null)
            {
                OnObjectCreate.Invoke(obj, window.CurrentWorld);
            }
        }

        public void AddGameObjectBypassID(GameObject obj)
        {
            if (window == null)
            {
                Logger.Error("Engine not started. Aborted adding gameobject.");
                return;
            }
            if (obj.TryGetComponent<MeshGroupComponent>(out var comp))
            {
                var meshSys = GetSystem<MeshSystem>();
                meshSys?.LoadMeshGroup(comp);
            }
            window.GameObjects.Add(obj.Id, obj);
            if (OnObjectCreate != null)
            {
                OnObjectCreate.Invoke(obj, window.CurrentWorld);
            }
        }

        public void RemoveGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn("Engine not started. Aborted removing gameobject.");
                return;
            }
            window.GameObjects.Remove(obj.Id);
        }

        public void RemoveGameObject(int id)
        {
            if (window == null) return;
            window.GameObjects.Remove(id);
        }

        public void RegisterSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            var system = Activator.CreateInstance(typeof(EntitySystemType), new object[] { this }) as EntitySystemType;
            Debug.Assert(system != null, "System is null after creation during registration");
            RegisteredSystems.Add(typeof(EntitySystemType), system);
        }

        public EntitySystem[] GetAllSystems()
        {
            return RegisteredSystems.Values.ToArray();
        }

        public EntitySystemType? GetSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            return RegisteredSystems[typeof(EntitySystemType)] as EntitySystemType;
        }

        public bool TryGetSystem<EntitySystemType>(out EntitySystemType? system) where EntitySystemType : EntitySystem
        {
            if (RegisteredSystems.TryGetValue(typeof(EntitySystemType), out var sys))
            {
                system = sys as EntitySystemType;
                return true;
            }
            system = null;
            return false;
        }

        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            if (window == null)
            {
                Logger.Warn("Start the engine before accessing gameobjects");
                return new CompType[0];
            }
            return window.GetObjectsOfType<CompType>();
        }

        public GameObject GetObjectById(int id)
        {
            if (window == null) return GameObject.InvalidObject;
            return window.GameObjects[id];
        }

        public void AddChildTo(GameObject parent, GameObject child)
        {
            child.Parent = parent;
            parent.Children.Add(child);
            AddGameObject(child);
        }

        public void LockCursor()
        {
            if (window == null) return;
            window.CursorGrabbed = true;
            window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Empty;
        }

        public void UnlockCursor()
        {
            if (window == null) return;
            window.CursorVisible = true;
            window.CursorGrabbed = false;
            window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Crosshair;
        }

        public void ToggleCursor()
        {
            if (window == null) return;

            if (!_cursorToggleTimer.TestTimer()) return;

            if (window.CursorGrabbed)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }

            _cursorToggleTimer.ResetTimer();
        }

        public bool GetCursorLockState()
        {
            if (window == null) return false;
            return window.CursorGrabbed;
        }

        public void StopEngine()
        {
            IsRunning = false;
            if (window == null) return;
            window.Close();
        }

        public GameObject[] GetAllObjects()
        {
            if (window == null) return new GameObject[0];
            return window.GameObjects.Values.ToArray();
        }

        // Not supported anymore
        /*public int CreateWorld()
        {
            return window.CreateWorld();
        }

        public void SwitchWorld(int world)
        {
            window.SwitchWorld(world);
        }

        public void RemoveWorld(int world)
        {
            window.RemoveWorld(world);
        }*/

        public void ToggleFullscreen()
        {
            if (window.WindowBorder == OpenTK.Windowing.Common.WindowBorder.Hidden)
            {
                EnterNormal();
                return;
            }
            if (DefaultBorderless)
            {
                EnterBorderless();
            }
            else
            {
                EnterFullscreen();
            }
        }

        public void EnterBorderless()
        {
            if (window == null) return;
            window.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
            var monitor = Monitors.GetPrimaryMonitor();
            window.CurrentMonitor = monitor.Handle;
            window.Size = new OpenTK.Mathematics.Vector2i(monitor.HorizontalResolution, monitor.VerticalResolution);
        }

        public void EnterFullscreen()
        {
            if (DefaultBorderless)
            {
                EnterBorderless();
                return;
            }
            if (window == null) return;
            window.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
            window.WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
        }

        public void EnterNormal()
        {
            if (window == null) return;
            window.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Resizable;
            window.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
        }
    }
}
