using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EliminationEngine.Physics
{
    public struct DefaultPoseIntegrator : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;

        public Vector3Wide GravityDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public bool AllowSubstepsForUnconstrainedBodies => false;

        public bool IntegrateVelocityForKinematics => false;

        public void Initialize(Simulation simulation)
        {
            Gravity = -Vector3.UnitY;
        }

        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear += GravityDt;
        }

        public void PrepareForIntegration(float dt)
        {
            GravityDt = Vector3Wide.Broadcast(Gravity * dt);
        }
    }

    public struct DefaultNarrowPhase : INarrowPhaseCallbacks
    {
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 1.0f;
            pairMaterial = default;
            return true;
        }

        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {

        }

        public void Initialize(Simulation simulation)
        {

        }
    }
}
