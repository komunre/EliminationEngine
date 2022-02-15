using EliminationEngine.GameObjects;
using EliminationEngine;
using BepuPhysics;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using EliminationEngine.Extensions;
using BepuPhysics.Constraints;
using System.Numerics;

namespace EliminationEngine.Physics
{
    public class PhysicsSystem : EntitySystem
    {
        public Simulation Sim;
        public float CurrentTimestep = 0.1f;
        public PhysicsSystem(Elimination e) : base(e)
        {

        }

        /*
        public override void OnLoad()
        {
            base.OnLoad();

            Sim = Simulation.Create(new BufferPool(), new DefaultNarrowPhase(), new DefaultPoseIntegrator(), new SolveDescription());

            Engine.OnObjectCreate += OnEngineObjectCreate;
        }

        private void OnEngineObjectCreate(GameObject obj, int world)
        {
            if (obj.TryGetComponent<HitBox>(out var hitbox))
            {
                var boxesList = new List<BodyHandle>();

                foreach (var box in hitbox.GetBoxes())
                {
                    var shapeBox = new Box(box.Bounds.Max.X - box.Bounds.Min.X, box.Bounds.Max.Y - box.Bounds.Min.X, box.Bounds.Max.Z - box.Bounds.Min.Z);
                    var bodyIndex = Sim.Shapes.Add(shapeBox);
                    if (!box.Static)
                    {
                        var bodyHandle = Sim.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(hitbox.Owner.Position.ToNumerics(), hitbox.Owner.Rotation.ToNumerics()), shapeBox.ComputeInertia(1.0f), new CollidableDescription(bodyIndex), new BodyActivityDescription(0.2f)));
                        boxesList.Add(bodyHandle);
                    }
                    else
                    {
                        Sim.Statics.Add(new StaticDescription(new RigidPose(hitbox.Owner.Position.ToNumerics(), hitbox.Owner.Rotation.ToNumerics()), new CollidableDescription(bodyIndex)));
                    }
                }

                for (var i = 0; i < boxesList.Count - 1; i++)
                {
                    Sim.Solver.Add(boxesList[i], boxesList[i + 1], new CenterDistanceConstraint());
                }
            }
        }

        public bool RaycastFromPos(Vector3 pos, Vector3 dir, float max)
        {
            return false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Sim.Timestep(CurrentTimestep);
        }*/
    }
}
