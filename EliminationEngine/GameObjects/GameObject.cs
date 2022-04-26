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
        public Color BaseColor = new Color(255, 255, 255, 255);

        public bool InvertedRotation = false;

        public Vector3 GlobalPosition => ParentHelper.GetAddedPos(this);
        public Quaternion GlobalRotation => ParentHelper.GetAddedRot(this);
        public Vector3 GlobalScale => ParentHelper.GetAddedScale(this);

        public Vector3 Position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _degreeRotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public void UpdateQuatRot()
        {
            _rotation = EliminationMathHelper.QuaternionFromEuler(_degreeRotation);
        }
        public void UpdateDegreeRot()
        {
            var result = Vector3.Zero;

            var rot = _rotation;
            Quaternion.ToEulerAngles(in rot, out var euler);

            result.X = euler.X / 3.14f * 180;
            result.Y = euler.Y / 3.14f * 180;
            result.Z = euler.Z / 3.14f * 180;

            _degreeRotation = result;
        }
        public Vector3 DegreeRotation
        {
            get => _degreeRotation;
            set {
                _degreeRotation = value;
                UpdateQuatRot();
            }
        }
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateDegreeRot();
            }
        }
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            
        }

        public Vector3 Forward()
        {
            var front = Vector3.Zero;

            var rot = GlobalRotation;
            Quaternion.ToEulerAngles(in rot, out var euler);

            front.X = euler.X / 3.14f * 180;
            front.Y = euler.Y / 3.14f * 180;
            front.Z = euler.Z / 3.14f * 180;

            return front;
        }

        public Vector3 DegreeForward()
        {
            return DegreeRotation;
        }

        public Vector3 DegreeRight()
        {
            return Vector3.Cross(DegreeForward(), Vector3.UnitY);
        }

        public Vector3 DegreeUp()
        {
            return Vector3.Cross(DegreeRight(), DegreeForward());
        }

        public Vector3 RadiansForward()
        {
            var front = Vector3.Zero;

            var rot = GlobalRotation;
            Quaternion.ToEulerAngles(in rot, out var euler);

            front.X = euler.X / 3.14f * 180;
            front.Y = euler.Y / 3.14f * 180;
            front.Z = euler.Z / 3.14f * 180;

            return front;
        }

        public Vector3 ForwardIsolated()
        {
            var direction = Vector3.Zero;

            var rot = GlobalRotation;

            return rot * new Vector3(0, 0, -1);
        }

        public Vector3 Up()
        {
            return Vector3.Cross(Right(), Forward()).Normalized();
        }

        public Vector3 Right()
        {
            return Vector3.Cross(Forward(), Vector3.UnitY).Normalized();
        }

        public void ReRotate()
        {
            if (_degreeRotation.Y > 165)
            {
                _degreeRotation.Y = 0.1f;
                _degreeRotation.X = -_degreeRotation.X;
                Console.WriteLine("Switch!");
            }
            if (_degreeRotation.X > 165)
            {
                _degreeRotation.X = 0.1f;
                _degreeRotation.Y = -_degreeRotation.Y;
                Console.WriteLine("Switch!");
            }
            if (_degreeRotation.Z < -180)
            {
                _degreeRotation.Z = -180;
                _degreeRotation.X = -_degreeRotation.X;
                InvertedRotation = !InvertedRotation;
                Console.WriteLine("Switch!");
            }
            if (_degreeRotation.Z > 180)
            {
                _degreeRotation.Z = 180;
                _degreeRotation.X = -_degreeRotation.X;
                InvertedRotation = !InvertedRotation;
                Console.WriteLine("Switch!");
            }
            Console.WriteLine(_degreeRotation);
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
