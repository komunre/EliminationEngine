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

        public delegate void PhysicsInit(PhysicsSystem sys);
        /// <summary>
        /// Initialize function. Use custom function as a value to override simulation init values.
        /// </summary>
        public PhysicsInit? InitFunc = DefaultPhysicsInit;

        /// <summary>
        /// Timestep of the simulation. It is recommended to keep it consistent after PostLoad.
        /// </summary>
        public float CurrentTimeStep = 0.01f;

        public static void DefaultPhysicsInit(PhysicsSystem sys)
        {
            sys.PhysicsSimulation = Simulation.Create(sys.Pool, new DefaultNarrowPhase(), new DefaultPoseIntegrator(), new SolveDescription(1));
        }

        public PhysicsSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        public override void PostLoad()
        {
            base.PostLoad();
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

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (!Initialized)
            {
                return;
            }

            PhysicsSimulation.Timestep(CurrentTimeStep);
        }

        public void AddPhysicsObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh, float mass)
        {
            if (!Initialized) return;
            mesh.ComputeClosedInertia(mass, out var inertia);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription()));
        }

        public void AddPhysicsObject(GameObject obj, Render.Mesh mesh, float mass)
        {
            if (!Initialized) return;
            AddPhysicsObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool), mass);
        }
    }
}
