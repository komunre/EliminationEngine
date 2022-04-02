using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine.Physics;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using EliminationEngine.Systems;
using EliminationEngine.Tools;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EliminationEngine
{
    public class Elimination
    {
        public string Title { get; set; } = "Elimination";
        public EliminationWindow? window = null;
        public Dictionary<Type, EntitySystem> RegisteredSystems = new();
        public float DeltaTime = 0;
        public TimeSpan Elapsed = new TimeSpan(0);
        public KeyboardState KeyState;
        public MouseState MouseState;

        public delegate void ObjectCreateEvent(GameObject obj, int world);
        public event ObjectCreateEvent OnObjectCreate;

        public Elimination(int width, int height)
        {
            var settings = new GameWindowSettings();
            //settings.IsMultiThreaded = true;
            var native = new NativeWindowSettings();
            native.Size = new OpenTK.Mathematics.Vector2i(width, height);
            native.Title = Title;
            native.Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible;
            window = new EliminationWindow(settings, native, this);
            KeyState = window.KeyboardState;
            MouseState = window.MouseState;
        }
        public void Run()
        {
            if (window == null) throw new InvalidDataException("No window was opened");
            RegisterSystem<MeshSystem>();
            RegisterSystem<SoundSystem>();
            RegisterSystem<UISystem>();
            RegisterSystem<Raycast>();
            RegisterSystem<CollisionSystem>();
            RegisterSystem<RemovalSystem>();
            RegisterSystem<GwenSystem>();
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
            window.GameObjects.Add(obj);
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
            window.GameObjects.Remove(obj);
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
            if (RegisteredSystems.TryGetValue(typeof(EntitySystemType), out var sys)) {
                system = sys as EntitySystemType;
                return true;
            }
            system = null;
            return false;
        }

        public CompType[]? GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            if (window == null)
            {
                Logger.Warn("Start the engine before accessing gameobjects");
                return null;
            }
            return window.GetObjectsOfType<CompType>();
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
        }

        public void UnlockCursor()
        {
            if (window == null) return;
            window.CursorGrabbed = false;
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
            if (window == null) return;
            window.Close();
        }

        public GameObject[]? GetAllObjects()
        {
            if (window == null) return null;
            return window.GameObjects.ToArray();
        }

        public int CreateWorld()
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
        }
    }
}
