using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Memory;
using EliminationEngine.Extensions;
using EliminationEngine.GameObjects;
using EliminationEngine.Physics;
using EliminationEngine.Tools.Physics;
using System.Diagnostics.CodeAnalysis;

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

        protected Dictionary<GameObject, BodyHandle> ObjectBodies = new();

        public static void DefaultPhysicsInit(PhysicsSystem sys)
        {
            sys.PhysicsSimulation = Simulation.Create(sys.Pool, new DefaultNarrowPhase(), new DefaultPoseIntegrator(), new SolveDescription(6));
        }

        public PhysicsSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();

            if (InitFunc == null)
            {
                Logger.Warn("No physics system init function declared. Physics system is disabled.");
                return;
            }
            InitFunc.Invoke(this);
            Initialized = true;

            if (PhysicsSimulation == null)
            {
                Logger.Error("No simulation was created during init.");
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

        public BodyHandle AddPhysicsObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            mesh.ComputeClosedInertia(mass, out var inertia);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
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

        public StaticHandle AddStaticObject(GameObject obj, Render.Mesh mesh)
        {
            if (!Initialized) return new StaticHandle(-1);
            return AddStaticObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool));
        }

        public void UpdateObjectFromSimulationInfo(BodyHandle handle, GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            if (float.IsNaN(pose.Position.X) || float.IsNaN(pose.Position.Y) || float.IsNaN(pose.Position.Z))
            {
                Logger.Error("Simulation returned NaN as an object position! Aborting object position update to avoid render issues.");
                return;
            }
            obj.Position = pose.Position.ToOpenTK();
            obj.Rotation = pose.Orientation.ToOpenTK();
        }

        public void UpdateObjectFromSimulationInfo(GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(GetObjectHandle(obj));
            var pose = bodyRef.Pose;
            obj.Position = pose.Position.ToOpenTK();
            obj.Rotation = pose.Orientation.ToOpenTK();
        }

        public void OverrideObjectPose(BodyHandle handle, GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            pose.Position = obj.Position.ToNumerics();
            pose.Orientation = obj.Rotation.ToNumerics();
        }

        public BodyHandle GetObjectHandle(GameObject obj)
        {
            return ObjectBodies[obj];
        }
    }
}
