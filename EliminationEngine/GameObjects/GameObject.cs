using EliminationEngine.Network;
using EliminationEngine.Tools;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EliminationEngine.GameObjects
{
    public struct SimpleRotation
    {
        public float Pitch = 0;
        public float Roll = 0;
        public float Yaw = 0;

        public SimpleRotation()
        {
        }

        public SimpleRotation(float pitch, float roll, float yaw)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
        }
    }

    public struct NetworkData
    {
        public string Name;
        public INetworkSerialize Value;

        public NetworkData(string name, INetworkSerialize value)
        {
            Name = name;
            Value = value;
        }
    }

    public class GameObject : ICopyable
    {
        public static GameObject InvalidObject = new GameObject();

        public int Id = -1;
        public string Name = "Default";
        public List<string> Layers = new();
        public GameObject? Parent = null;
        public List<GameObject> Children = new List<GameObject>();
        public Color BaseColor = new Color(255, 255, 255, 255);

        public List<string> ObjectData = new(); // JSON Data
        public object[] DeserializedObjectData = new string[0];

        public bool InvertedRotation = false;

        public Vector3 GlobalPosition => ParentHelper.GetAddedPos(this);
        public Quaternion GlobalRotation => ParentHelper.GetAddedRot(this);
        public Vector3 GlobalDegreeRotation => ParentHelper.GetAddedDegreeRot(this);
        public Vector3 GlobalScale => ParentHelper.GetAddedScale(this);

        public Vector3 Position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _degreeRotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public bool LockVisualPosition = false;
        public bool LockVisualRotation = false;
        public bool LockPhysicsRotation = false;
        public bool LockPhysicsPosition = false;

        public void AddObjectData(object data)
        {
            ObjectData.Add(JsonConvert.SerializeObject(data));
        }

        // Possibly requires to detach List from reference on original object.
        public object CreateCopy()
        {
            var obj = new GameObject();
            obj.Children = this.Children;
            obj.ObjectData = this.ObjectData;
            obj.Components = this.Components;
            return obj;
        }

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

        /// <summary>
        /// Euler angles rotation. Interconnected with quaternion.
        /// </summary>
        public Vector3 DegreeRotation
        {
            get => _degreeRotation;
            set
            {
                while (value.Y > 360)
                {
                    value.Y -= 360;
                }
                while (value.Y < 0)
                {
                    value.Y += 360;
                }
                _degreeRotation = value;
                UpdateQuatRot();
            }
        }

        /// <summary>
        /// Quaternion rotation. Interconnected with euler.
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateDegreeRot();
            }
        }

        public void ClampVerticalTo90()
        {
            _degreeRotation.X = Math.Clamp(_degreeRotation.X, -89, 89);
            UpdateQuatRot();
        }

        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {

        }

        // Forward, Right, Up
        /// <summary>
        /// Gets directions of an object.
        /// </summary>
        /// <returns>An array of directions. 0 - Forward, 1 - Right, 2 - Up</returns>
        public Vector3[] GetDirections()
        {
            var rot = DegreeRotation;

            /*var forward = new Vector3();
            forward.X = (float)Math.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)Math.Cos(MathHelper.DegreesToRadians(rot.X));
            forward.Y = (float)Math.Sin(MathHelper.DegreesToRadians(rot.X));
            forward.Z = (float)Math.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)Math.Sin(MathHelper.DegreesToRadians(rot.X));
            forward = Vector3.Normalize(forward);*/

            /*var x = (float)MathHelper.Sin(MathHelper.DegreesToRadians(rot.X));
            var y = (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.Y)) * (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.X));
            var z = (float)MathHelper.Sin(MathHelper.DegreesToRadians(rot.Y)) * (float)MathHelper.Cos(MathHelper.DegreesToRadians(rot.X));*/

            /*var x = EliminationMathHelper.DegreeCos(rot.Y) * EliminationMathHelper.DegreeCos(rot.X);
            var y = EliminationMathHelper.DegreeSin(rot.X) * EliminationMathHelper.DegreeCos(rot.X);
            var z = (EliminationMathHelper.DegreeSin(rot.Y) * EliminationMathHelper.DegreeCos(rot.X));*/

            var x = EliminationMathHelper.DegreeRadCos(rot.X) * EliminationMathHelper.DegreeRadCos(rot.Y);
            var y = EliminationMathHelper.DegreeRadSin(rot.X);
            var z = EliminationMathHelper.DegreeRadCos(rot.X) * EliminationMathHelper.DegreeRadSin(rot.Y);

            var forward = new Vector3(x, y, z).Normalized();

            //var forward = _rotation * new Vector3(1, 0, 0);

            var right = Vector3.Cross(forward, Vector3.UnitY).Normalized();
            var up = Vector3.Cross(right, forward).Normalized();

            return new Vector3[] { forward, right, up };
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

        public void ReRotate()
        {

        }

        /// <summary>
        /// NO LONGER SUPPORTED
        /// </summary>
        /// <param name="target"></param>
        public void LookAt(Vector3 target)
        {
            /*var rot = Rotation.ToEulerAngles();

            Vector3 forwardVector = Vector3.Normalize(target - Position);

            float dot = Vector3.Dot(-Vector3.UnitZ, forwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                Rotation = new Quaternion(Vector3.UnitY.X, Vector3.UnitY.Y, Vector3.UnitY.Z, 3.1415926535897932f);
                return;
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                Rotation = Quaternion.Identity;
                return;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(-Vector3.UnitZ, forwardVector);
            rotAxis = Vector3.Normalize(rotAxis);

            var desired = Quaternion.FromAxisAngle(rotAxis, rotAngle);
            Rotation = desired;*/
            //Vector3 forwardVector = Vector3.Normalize(target - GlobalPosition);
            //DegreeRotation = Vector3.Cross(DegreeForward(), forwardVector);
            //Rotation = RotationBetweenVectors(Vector3.UnitX, forwardVector);
        }

        Quaternion RotationBetweenVectors(Vector3 start, Vector3 dest)
        {
            start = Vector3.Normalize(start);
            dest = Vector3.Normalize(dest);

            float cosTheta = Vector3.Dot(start, dest);
            Vector3 rotationAxis;

            /*if (cosTheta < -1 + 0.001f)
            {
                // special case when vectors in opposite directions:
                // there is no "ideal" rotation axis
                // So guess one; any will do as long as it's perpendicular to start
                rotationAxis = Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), start);
                if (rotationAxis.Length < 0.01) // bad luck, they were parallel, try again!
                    rotationAxis = Vector3.Cross(new Vector3(1.0f, 0.0f, 0.0f), start);

                rotationAxis = Vector3.Normalize(rotationAxis);
                return Quaternion.FromAxisAngle(rotationAxis, MathHelper.DegreesToRadians(180.0f));
            }*/

            rotationAxis = Vector3.Cross(start, dest);

            float s = (float)MathHelper.Sqrt((1 + cosTheta) * 2);
            float invs = 1 / s;

            return new Quaternion(
                s * 0.5f,
                rotationAxis.X * invs,
                rotationAxis.Y * invs,
                rotationAxis.Z * invs
            );
        }

        public CompType AddComponent<CompType>() where CompType : EntityComponent
        {
            Logger.Info(Loc.Get("ADDING_COMPONENT") + typeof(CompType));
            var comp = Activator.CreateInstance(typeof(CompType), new object[] { this }) as CompType;
            Debug.Assert(comp != null, "No owner was added to component during creation");
            Components.Add(typeof(CompType), comp);
            return comp;
        }

        public void AddExistingComponent(EntityComponent comp)
        {
            Logger.Info(Loc.Get("ADDING_EXISTING_COMPONENT") + comp.GetType());
            Components.Add(comp.GetType(), comp);
        }

        public void RemoveComponent<CompType>() where CompType : EntityComponent
        {
            Logger.Info(Loc.Get("REMOVING_COMPONENT") + typeof(CompType));
            Components.Remove(typeof(CompType));
        }

        /// <summary>
        /// Accesses component directly. Might return null.
        /// </summary>
        /// <typeparam name="CompType"></typeparam>
        /// <returns>Desired component or null.</returns>
        public CompType GetComponent<CompType>() where CompType : EntityComponent
        {
            var comp = Components[typeof(CompType)] as CompType;
            if (comp == null) throw new NullReferenceException(Loc.Get("GET_COMPONENT_FAIL") + " ID: " + Id + ",Component: " + nameof(CompType));
            return comp;
        }

        /// <summary>
        /// Tries to get component, returns false if not found. Out variable is not null when returned true.
        /// </summary>
        /// <typeparam name="CompType">Component Type</typeparam>
        /// <param name="component">Desired component if found. Null otherwise.</param>
        /// <returns>True if found, false if not.</returns>
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

        /// <summary>
        /// Checks if component esists in the object.
        /// </summary>
        /// <typeparam name="CompType">Component Type</typeparam>
        /// <returns></returns>
        public bool HasComponent<CompType>() where CompType : EntityComponent
        {
            return Components.ContainsKey(typeof(CompType));
        }

        public EntityComponent[] GetAllComponents()
        {
            return Components.Values.ToArray();
        }

        public void InsertComponentsData(EntityComponent[] components)
        {
            Components.Clear();
            foreach (var comp in components)
            {
                Components.Add(comp.GetType(), comp);
            }
        }
    }
}
