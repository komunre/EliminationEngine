using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class SimpleRotation
    {
        public float Pitch = 0;
        public float Roll = 0;
        public float Yaw = 0;
    }
    public class GameObject
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            // AddComponent<Mesh>(); // Add mesh for future usage during drawing
        }

        public Vector3 Forward()
        {
            var direction = Vector3.Zero;
            var euler = Rotation.ToEulerAngles();

            var mul = 180;
            direction.X = (float)Math.Cos(Rotation.X) * (float)Math.Cos(Rotation.Y);
            direction.Y = (float)Math.Sin(Rotation.X);
            direction.Z = (float)Math.Cos(Rotation.X) * (float)Math.Sin(Rotation.Y);

            //direction.X = (float)Math.Cos(euler.Y) * (float)Math.Cos(euler.X);
            //direction.Y = (float)Math.Sin(euler.Y) * (float)Math.Cos(euler.X);
            //direction.Z = (float)Math.Sin(euler.X);

            direction = Vector3.Normalize(direction);

            return Rotation * new Vector3(0, 0, -1) + Position;
        }

        public Vector3 Up()
        {
            return Vector3.Cross(Forward(), new Vector3(1, 0, 0));
        }

        public Vector3 Right()
        {
            return Vector3.Cross(Forward(), new Vector3(0, 1, 0));
        }

        public void LookAt(Vector3 target)
        {
            var rot = Rotation.ToEulerAngles();

            Vector3 forwardVector = Vector3.Normalize(target - Position);

            float dot = Vector3.Dot(-Vector3.UnitZ, forwardVector);

            /*if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                Rotation = new Quaternion(Vector3.UnitY.X, Vector3.UnitY.Y, Vector3.UnitY.Z, 3.1415926535897932f);
                return;
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                Rotation = Quaternion.Identity;
                return;
            }*/

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(-Vector3.UnitZ, forwardVector);
            rotAxis = Vector3.Normalize(rotAxis);

            var desired = Quaternion.FromAxisAngle(rotAxis, rotAngle);
            Rotation = Quaternion.Slerp(desired * 0.9f, desired * 1.2f, 0.05f);
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
