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
        public Vector3 GlobalDegreeRotation => ParentHelper.GetAddedDegreeRot(this);
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

        /// Forward, Right, Up
        public Vector3[] GetDirections() {
            var rot = GlobalDegreeRotation;

            /*var forward = new Vector3();
            forward.X = (float)Math.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)Math.Cos(MathHelper.DegreesToRadians(rot.X));
            forward.Y = (float)Math.Sin(MathHelper.DegreesToRadians(rot.X));
            forward.Z = (float)Math.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)Math.Sin(MathHelper.DegreesToRadians(rot.X));
            forward = Vector3.Normalize(forward);*/

            /*var horizontal = (float)MathHelper.Sin(MathHelper.DegreesToRadians(rot.X));
            var vertical = (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.X));
            var stabilization = (float)MathHelper.Sin(MathHelper.DegreesToRadians(rot.Y)) * (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.X));
            var forward = new Vector3(vertical, horizontal, stabilization);*/

            var x = EliminationMathHelper.DegreeCos(rot.Y) * EliminationMathHelper.DegreeCos(rot.X);
            var y = EliminationMathHelper.DegreeSin(rot.X) * EliminationMathHelper.DegreeCos(rot.X);
            var z = (EliminationMathHelper.DegreeSin(rot.Y) * EliminationMathHelper.DegreeCos(rot.X));

            var forward = new Vector3(x, y, z).Normalized();

            var right = Vector3.Cross(Vector3.UnitY, forward).Normalized();
            var up = Vector3.Cross(right, forward).Normalized();

            Console.WriteLine(forward);

            return new Vector3[]{forward, right, up};
        }
        public Vector3 DegreeForward()
        {
            return GetDirections()[0];
        }

        public Vector3 DegreeRight()
        {
            return GetDirections()[1];
        }

        public Vector3 DegreeUp()
        {
            return GetDirections()[2];
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
