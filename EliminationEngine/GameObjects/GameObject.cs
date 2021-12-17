using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class GameObject
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion rotation { get; private set; } = Quaternion.Identity;
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            // AddComponent<Mesh>(); // Add mesh for future usage during drawing
        }

        public void AddComponent<CompType>() where CompType : EntityComponent
        {
            var comp = Activator.CreateInstance<CompType>();
            comp.Owner = this;
            Components.Add(typeof(CompType), comp);
        }

        public CompType? GetComponent<CompType>() where CompType : EntityComponent
        {
            return Components[typeof(CompType)] as CompType;
        }

        public bool TryGetComponent<CompType>(out CompType? component) where CompType : EntityComponent
        {
            if (Components.TryGetValue(typeof(CompType), out var comp))
            {
                component = comp as CompType;
                return true;
            }
            component = null;
            return false;
        }
    }
}
