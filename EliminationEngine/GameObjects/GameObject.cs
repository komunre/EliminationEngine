using EliminationEngine.Tools;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        public string Name = "Default";
        public GameObject? Parent = null;
        public List<GameObject> Children = new List<GameObject>();
        public Color BaseColor = new Color(1, 1, 1, 1);

        public Vector3 GlobalPosition => ParentHelper.GetAddedPos(this);
        public Quaternion GlobalRotation => ParentHelper.GetAddedRot(this);
        public Vector3 GlobalScale => ParentHelper.GetAddedScale(this);

        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            
        }

        public Vector3 Forward()
        {
            var direction = Vector3.Zero;

            var rot = GlobalRotation;

            return rot * new Vector3(0, 0, -1) + GlobalPosition;
        }

        public Vector3 ForwardIsolated()
        {
            var direction = Vector3.Zero;

            var rot = GlobalRotation;

            return rot * new Vector3(0, 0, -1);
        }

        public Vector3 UpIsolated()
        {
            return Vector3.Cross(ForwardIsolated(), new Vector3(-1, 0, 0));
        }

        public Vector3 Up()
        {
            return Vector3.Cross(Forward(), new Vector3(-1, 0, 0));
        }

        public Vector3 Right()
        {
            return Vector3.Cross(Forward(), new Vector3(0, -1, 0));
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
            Rotation = desired;
        }

        public CompType AddComponent<CompType>() where CompType : EntityComponent
        {
            var comp = Activator.CreateInstance(typeof(CompType), new object[] { this }) as CompType;
            Debug.Assert(comp != null, "No owner was added to component during creation");
            Components.Add(typeof(CompType), comp);
            return comp;
        }

        public CompType? GetComponent<CompType>() where CompType : EntityComponent
        {
            return Components[typeof(CompType)] as CompType;
        }

        public bool TryGetComponent<CompType>([NotNullWhen(true)] out CompType? component) where CompType : EntityComponent
        {
            if (Components.TryGetValue(typeof(CompType), out var comp))
            {
                component = (CompType)comp;
                return true;
            }
            component = null;
            return false;
        }

        public bool HasComponent<CompType>() where CompType : EntityComponent
        {
            return Components.ContainsKey(typeof(CompType));
        }
    }
}
