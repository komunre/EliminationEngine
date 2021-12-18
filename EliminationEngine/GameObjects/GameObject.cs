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
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            // AddComponent<Mesh>(); // Add mesh for future usage during drawing
        }

        public Vector3 Forward()
        {
            var direction = Vector3.Zero;
            var euler = Rotation.ToEulerAngles();
            direction.X = (float)Math.Cos(MathHelper.DegreesToRadians(euler.Z)) * (float)Math.Cos(MathHelper.DegreesToRadians(euler.X));
            direction.Y = (float)Math.Sin(MathHelper.DegreesToRadians(euler.Z));
            direction.Z = (float)Math.Cos(MathHelper.DegreesToRadians(euler.Z)) * (float)Math.Sin(MathHelper.DegreesToRadians(euler.X));
            direction.Normalize();

            direction += Position;

            return direction;
        }

        public void LookAt(Vector3 target)
        {
            Rotation = EliminationMathHelper.QuaternionFromEuler(Position - target);
        }

        public CompType AddComponent<CompType>() where CompType : EntityComponent
        {
            var comp = Activator.CreateInstance<CompType>();
            comp.Owner = this;
            Components.Add(typeof(CompType), comp);
            return comp;
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
