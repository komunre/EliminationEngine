using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine.Render;
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

        public Elimination(int width, int height)
        {
            var settings = new GameWindowSettings();
            //settings.IsMultiThreaded = true;
            var native = new NativeWindowSettings();
            native.Size = new OpenTK.Mathematics.Vector2i(width, height);
            native.Title = Title;
            native.Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible;
            window = new EliminationWindow(settings, native, this);
        }
        public void Run()
        {
            if (window == null) throw new InvalidDataException("No window was opened");
            RegisterSystem<MeshSystem>();
            window.Run();
        }

        public void AddGameObject(GameObject obj)
        {
            window.GameObjects.Add(obj);
        }

        public void RegisterSystem<EntitySystemType>() where EntitySystemType : EntitySystem
        {
            var system = Activator.CreateInstance<EntitySystemType>();
            system.Engine = this;
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

        public CompType[] GetObjectsOfType<CompType>() where CompType : EntityComponent
        {
            return window.GetObjectsOfType<CompType>();
        }
    }
}
