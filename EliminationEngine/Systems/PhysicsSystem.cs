using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using EliminationEngine.Extensions;
using EliminationEngine.GameObjects;
using EliminationEngine.Physics;
using EliminationEngine.Tools.Physics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EliminationEngine.Systems
{
    public class PhysicsSystem : EntitySystem
    {
        public Simulation? PhysicsSimulation;
        public BufferPool Pool = new BufferPool();

        /// <summary>
        /// Identifies if physics system was initialized. Does not indicate correct functioning.
        /// </summary>
        public bool Initialized = false;

        /// <summary>
        /// Allow Physics System to automatically update object position and rotation according to simulation data.
        /// </summary>
        public bool AutoUpdateObjects = true;

        public delegate void PhysicsInit(PhysicsSystem sys);
        /// <summary>
        /// Initialize function. Use custom function as a value to override simulation init values.
        /// </summary>
        public PhysicsInit? InitFunc = DefaultPhysicsInit;

        /// <summary>
        /// Timestep of the simulation. It is recommended to keep it consistent after PostLoad.
        /// </summary>
        public float CurrentTimeStep = 0.01f;

        public Dictionary<GameObject, BodyHandle> ObjectBodies = new();

        public static void DefaultPhysicsInit(PhysicsSystem sys)
        {
            sys.PhysicsSimulation = Simulation.Create(sys.Pool, new DefaultNarrowPhase(), new DefaultPoseIntegrator(), new SolveDescription(6, 4));
        }

        public PhysicsSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();

            if (InitFunc == null)
            {
                Logger.Warn(Loc.Get("WARN_NO_PHYSICS_INIT"));
                return;
            }
            InitFunc.Invoke(this);
            Initialized = true;

            if (PhysicsSimulation == null)
            {
                Logger.Error(Loc.Get("ERROR_NO_SIMULATION_CREATED"));
            }
        }

        public override void PostLoad()
        {
            base.PostLoad();
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (!Initialized)
            {
                return;
            }

            PhysicsSimulation.Timestep(CurrentTimeStep);

            if (!AutoUpdateObjects) return;

            foreach (var pair in ObjectBodies)
            {
                UpdateObjectFromSimulationInfo(pair.Value, pair.Key);
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();

        }

        public OpenTK.Mathematics.Vector3 GetObjectVelocity(GameObject obj)
        {
            return GetObjectVelocity(GetObjectHandle(obj));
        }

        public OpenTK.Mathematics.Vector3 GetObjectVelocity(BodyHandle handle)
        {
            return PhysicsSimulation.Bodies.GetBodyReference(handle).Velocity.Linear.ToOpenTK();
        }

        public BodyHandle AddPhysicsObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            mesh.ComputeClosedInertia(mass, out var inertia);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        public BodyHandle AddBoxObject(GameObject obj, float width, float height, float length, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            var box = new Box(width, height, length);
            var meshIndex = PhysicsSimulation.Shapes.Add(box);
            var inertia = box.ComputeInertia(mass);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        public BodyHandle AddCapsuleObject(GameObject obj, float radius, float length, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            var capsule = new Capsule(radius, length);
            var meshIndex = PhysicsSimulation.Shapes.Add(capsule);
            var inertia = capsule.ComputeInertia(mass);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        public BodyHandle AddKinematicObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh)
        {
            if (!Initialized) return new BodyHandle(-1);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        public StaticHandle AddStaticObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh)
        {
            if (!Initialized) return new StaticHandle(-1);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Statics.Add(new StaticDescription(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), new CollidableDescription(meshIndex)));
            return handle;
        }

        public BodyHandle AddPhysicsObject(GameObject obj, Render.Mesh mesh, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            return AddPhysicsObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool), mass);
        }

        public BodyHandle AddKinematicObject(GameObject obj, Render.Mesh mesh)
        {

            if (!Initialized) return new BodyHandle(-1);
            return AddKinematicObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool));
        }

        public StaticHandle AddStaticObject(GameObject obj, Render.Mesh mesh)
        {
            if (!Initialized) return new StaticHandle(-1);
            return AddStaticObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool));
        }

        public void AddDistanceConstraint(BodyHandle handle1, BodyHandle handle2, float minDist, float maxDist, float springFreq, float springDamp)
        {
            PhysicsSimulation.Solver.Add(handle1, handle2, new DistanceLimit(Vector3.Zero, Vector3.Zero, minDist, maxDist, new SpringSettings(springFreq, springDamp)));
        }

        public void AddFixedDistanceConstraint(BodyHandle handle1, BodyHandle handle2, float distance)
        {
            AddDistanceConstraint(handle1, handle2, 0, distance, 0.01f, 1);
        }

        public void UpdateObjectFromSimulationInfo(BodyHandle handle, GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            if (float.IsNaN(pose.Position.X) || float.IsNaN(pose.Position.Y) || float.IsNaN(pose.Position.Z))
            {
                Logger.Error(Loc.Get("ERROR_SIMULATION_RETURNED_NAN"));
                return;
            }
            if (!obj.LockVisualPosition && !obj.LockPhysicsPosition) obj.Position = pose.Position.ToOpenTK();
            if (!obj.LockVisualRotation && !obj.LockPhysicsRotation) obj.Rotation = pose.Orientation.ToOpenTK();
            if (obj.LockPhysicsRotation)
            {
                pose.Orientation = obj.Rotation.ToNumerics();
                bodyRef.Pose = pose;
            }
        }

        public void UpdateObjectFromSimulationInfo(GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(GetObjectHandle(obj));
            var pose = bodyRef.Pose;
            obj.Position = pose.Position.ToOpenTK();
            obj.Rotation = pose.Orientation.ToOpenTK();
        }

        public void OverrideObjectPoseWithValues(BodyHandle handle, Vector3 position, Quaternion orientation)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            pose.Position = position;
            pose.Orientation = orientation;
            bodyRef.Pose = pose;
        }

        public void OverrideObjectPose(BodyHandle handle, GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            pose.Position = obj.Position.ToNumerics();
            pose.Orientation = obj.Rotation.ToNumerics();
            bodyRef.Pose = pose;
        }

        public void AddObjectVelocity(BodyHandle handle, Vector3 velocity)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            bodyRef.Awake = true;
            var pose = bodyRef.Pose;
            bodyRef.Velocity.Linear += velocity * CurrentTimeStep;
        }

        public void LimitObjectVelocity(BodyHandle handle, float horizontalLimit, float verticalLimit)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var originalVelocity = bodyRef.Velocity.Linear;
            var vel = new Vector3(originalVelocity.X, originalVelocity.Y, originalVelocity.Z);
            vel.Y = 0;
            if (vel.Length() > horizontalLimit)
            {
                var normalized = Vector3.Normalize(vel);
                var result = normalized * horizontalLimit;
                bodyRef.Velocity.Linear = new Vector3(result.X, originalVelocity.Y, result.Z);
            }
        }

        public void LimitObjectVelocity(BodyHandle handle, float limit)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var originalVelocity = bodyRef.Velocity.Linear;
            var vel = new Vector3(originalVelocity.X, originalVelocity.Y, originalVelocity.Z);
            if (vel.Length() > limit)
            {
                var normalized = Vector3.Normalize(vel);
                var result = normalized * limit;
                bodyRef.Velocity.Linear = new Vector3(result.X, originalVelocity.Y, result.Z);
            }
        }

        public BodyHandle GetObjectHandle(GameObject obj)
        {
            return ObjectBodies[obj];
        }
    }
}
