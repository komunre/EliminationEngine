using EliminationEngine.GameObjects;
using EliminationEngine.Network;
using EliminationEngine.Physics;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using EliminationEngine.Systems;
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

        public Elimination(string[] args)
        {
            ProgramArgs = args;
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
            if (window == null) throw new InvalidDataException("No window was opened, no headless flag was specified");
            RegisterSystem<MeshSystem>();
            RegisterSystem<SoundSystem>();
            RegisterSystem<Raycast>();
            RegisterSystem<CollisionSystem>();
            RegisterSystem<RemovalSystem>();
            RegisterSystem<ImGuiSystem>();
            RegisterSystem<CameraResizeSystem>();
            RegisterSystem<DebugRenderSystem>();
            RegisterSystem<NetworkManager>();
            window.Run();
        }

        public void AddGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn("Start the engine before accessing gameobjects");
                return;
            }
            if (obj.TryGetComponent<MeshGroupComponent>(out var comp))
            {
                var meshSys = GetSystem<MeshSystem>();
                meshSys?.LoadMeshGroup(comp);
            }
            window.GameObjects.Add(window.MaxObjectId, obj);
            window.MaxObjectId++;
            if (OnObjectCreate != null)
            {
                OnObjectCreate.Invoke(obj, window.CurrentWorld);
            }
        }

        public void AddGameObjectNoId(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn("Start the engine before accessing gameobjects");
                return;
            }
            if (obj.TryGetComponent<MeshGroupComponent>(out var comp))
            {
                var meshSys = GetSystem<MeshSystem>();
                meshSys?.LoadMeshGroup(comp);
            }
            if (OnObjectCreate != null)
            {
                OnObjectCreate.Invoke(obj, window.CurrentWorld);
            }
        }

        public void RemoveGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn("Start the engine before accessing gameobjects");
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
            //window.CursorVisible = false;
            //window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Empty;
        }

        public void UnlockCursor()
        {
            if (window == null) return;
            StopEngine();
            //window.CursorGrabbed = false;
            //window.CursorVisible = true;
            //window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Default;
        }

        public void ToggleCursor()
        {
            if (window == null) return;
            window.CursorGrabbed = !window.CursorGrabbed;
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

        public void EnterFullscreen()
        {
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
