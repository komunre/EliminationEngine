using EliminationEngine.GameObjects;
using EliminationEngine.Network;
using EliminationEngine.Physics;
using EliminationEngine.Render;
using EliminationEngine.Render.UI;
using EliminationEngine.Systems;
using EliminationEngine.Tools;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Text;

namespace EliminationEngine
{
    public class Elimination
    {
        public string Title { get; set; } = "Elimination";
        /// <summary>
        /// Window of an engine. Direct access is not recommended.
        /// </summary>
        public EliminationWindow? window = null;
        /// <summary>
        /// List of registered system. Direct access is not recommended, use engine functions like `GetSystem` and `TryGetSystem` instead.
        /// </summary>
        public Dictionary<Type, EntitySystem> RegisteredSystems = new();
        /// <summary>
        /// Seconds past last frame.
        /// </summary>
        public float DeltaTime = 0;
        /// <summary>
        /// Total engine running time.
        /// </summary>
        public TimeSpan Elapsed = new TimeSpan(0);
        /// <summary>
        /// Keyboard state.
        /// </summary>
        public KeyboardState? KeyState;
        /// <summary>
        /// Mouse state.
        /// </summary>
        public MouseState? MouseState;

        public delegate void ObjectCreateEvent(GameObject obj, int world);
        /// <summary>
        /// Called when object is created.
        /// </summary>
        public event ObjectCreateEvent? OnObjectCreate;

        public string[] ProgramArgs = new string[0];
        public EliminationArgs ProcessedArgs;

        /// <summary>
        /// Sets if engine should be running in headless mode. Must be set before Run().
        /// </summary>
        public bool Headless = false;
        /// <summary>
        /// Idenntifies if engine is currently running.
        /// </summary>
        public bool IsRunning = false;

        /// <summary>
        /// Default on borderless upon fullscreen.
        /// </summary>
        public bool DefaultBorderless = true;

        /// <summary>
        /// Sets timer before user can toggle cursor visiblity and lock again.
        /// </summary>
        private EngineTimer _cursorToggleTimer = new EngineTimer(TimeSpan.FromSeconds(1)); 


        public class EliminationArgs
        {
            public int headless { get; set; }
            public string lang { get; set; }
            public int networked { get; set; }
            public int server { get; set; }
            public string host { get; set; }
            public int port { get; set; }

            public EliminationArgs()
            {
                headless = 0;
                lang = "en";
                networked = 0;
                server = 0;
                host = "localhost";
                port = 55784;
            }
        }

        public static ArgumentClass ParseArguments<ArgumentClass>(string[] args)
        {
            var combined = "";
            foreach (var arg in args)
            {
                combined += arg + " ";
            }
            return FileParser.Deserialize<ArgumentClass>(combined);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">Console arguments from main function.</param>
        public Elimination(string[] args)
        {
            ProgramArgs = args;
            Console.OutputEncoding = Encoding.UTF8;
            ProcessedArgs = ParseArguments<EliminationArgs>(ProgramArgs);
            Headless = ProcessedArgs.headless == 1 ? true : false;
            Loc.InitLoc(ProcessedArgs.lang);
            RegisterSystem<EngineStaticsInitSystem>();
            RegisterSystem<MeshSystem>();
            RegisterSystem<SoundSystem>();
            RegisterSystem<PhysicsSystem>();
            RegisterSystem<Raycast>();
            RegisterSystem<CollisionSystem>();
            RegisterSystem<RemovalSystem>();
            RegisterSystem<ImGuiSystem>();
            RegisterSystem<CameraResizeSystem>();
            RegisterSystem<DebugRenderSystem>();
            RegisterSystem<EditorSystem>();
            RegisterSystem<NetworkManager>();
        }

        /// <summary>
        /// Removes system from list of registered systems.
        /// </summary>
        /// <typeparam name="EntitySystemType">EntitySystem type</typeparam>
        public void UnregisterSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            if (!RegisteredSystems.ContainsKey(typeof(EntitySystemType)))
            {
                Logger.Warn(Loc.Get("WARN_UNREGISTER_FAIL_NONEXIST") + typeof(EntitySystemType));
                return;
            }
            RegisteredSystems.Remove(typeof(EntitySystemType));
            Logger.Info(Loc.Get("INFO_UNREGISTERED") + typeof(EntitySystemType));
        }

        /// <summary>
        /// Sets clear color. This is the color player will see when nothing is drawn on certain pixel.
        /// </summary>
        /// <param name="color">Desired clear color.</param>
        public void SetClearColor(Tools.Color color)
        {
            color.ConvertToFloat();
            window.ClearColor = color;
            window.SetWindowClearColor();
        }

        public void Run()
        {
            IsRunning = true;
            if (Loc.CurrentLang != ProcessedArgs.lang) Loc.InitLoc(ProcessedArgs.lang);
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
            window.Run();

            window.Cursor = OpenTK.Windowing.Common.Input.MouseCursor.Crosshair;
        }

        /// <summary>
        /// Inserts gameobject into the game world.
        /// </summary>
        /// <param name="obj">Object to insert.</param>
        public void AddGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn(Loc.Get("WARN_START_BEFORE_ACCESS"));
                return;
            }
            if (obj.TryGetComponent<MeshGroupComponent>(out var comp))
            {
                var meshSys = GetSystem<MeshSystem>();
                meshSys?.LoadMeshGroup(comp);
            }
            obj.Id = window.MaxObjectId;
            window.GameObjects.Add(window.MaxObjectId, obj);
            window.MaxObjectId++;
            if (OnObjectCreate != null)
            {
                OnObjectCreate.Invoke(obj, window.CurrentWorld);
            }
        }

        /// <summary>
        /// Inserts gameobject into the game world, but does not set new object ID.
        /// </summary>
        /// <param name="obj">Object to insert.</param>
        public void AddGameObjectNoId(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn(Loc.Get("WARN_START_BEFORE_ACCESS"));
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

        /// <summary>
        /// Removes gameobject from the game world.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        public void RemoveGameObject(GameObject obj)
        {
            if (window == null)
            {
                Logger.Warn(Loc.Get("WARN_START_BEFORE_ACCESS"));
                return;
            }
            window.GameObjects.Remove(obj.Id);
        }

        /// <summary>
        /// Removes gameobject from the game world.
        /// </summary>
        /// <param name="id">ID of an object to remove.</param>
        public void RemoveGameObject(int id)
        {
            if (window == null) return;
            window.GameObjects.Remove(id);
        }

        /// <summary>
        /// Registers system.
        /// </summary>
        /// <typeparam name="EntitySystemType">EntitySystem type.</typeparam>
        public void RegisterSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            var system = Activator.CreateInstance(typeof(EntitySystemType), new object[] { this }) as EntitySystemType;
            Debug.Assert(system != null, "System is null after creation during registration");
            RegisteredSystems.Add(typeof(EntitySystemType), system);
            Logger.Info(Loc.Get("INFO_REGISTERED") + typeof(EntitySystemType));
        }

        public EntitySystem[] GetAllSystems()
        {
            return RegisteredSystems.Values.ToArray();
        }

        /// <summary>
        /// Directly accesses EntitySystem from registered list. Might return null.
        /// </summary>
        /// <typeparam name="EntitySystemType">EntitySystem type.</typeparam>
        /// <returns>Desired system or null if not found.</returns>
        public EntitySystemType GetSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            var sys = RegisteredSystems[typeof(EntitySystemType)] as EntitySystemType;
            if (sys == null) throw new NullReferenceException(Loc.Get("GET_SYSTEM_FAIL") + nameof(EntitySystemType));
            return sys;
        }

        /// <summary>
        /// Tries to get system, returns false if not found. Out variable is not null when returned true.
        /// </summary>
        /// <typeparam name="EntitySystemType">EntitySystem type.</typeparam>
        /// <param name="system">Desired system if found. Null otherwise.</param>
        /// <returns>True if found, false if not.</returns>
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

        /// <summary>
        /// Gets objects with certain type of component attached to them.
        /// </summary>
        /// <typeparam name="CompType">EntityComponent type.</typeparam>
        /// <returns>Objects with component with type of CompType.</returns>
        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            if (window == null)
            {
                Logger.Warn(Loc.Get("WARN_START_BEFORE_ACCESS"));
                return new CompType[0];
            }
            return window.GetObjectsOfType<CompType>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id of desired object.</param>
        /// <returns>Object with certain id.</returns>
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
