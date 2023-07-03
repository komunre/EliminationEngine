using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(Simulation simulation)
        {
            Gravity = -Vector3.UnitY * 0.3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear += GravityDt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareForIntegration(float dt)
        {
            GravityDt = Vector3Wide.Broadcast(Gravity * dt);
        }
    }

    public struct DefaultNarrowPhase : INarrowPhaseCallbacks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 120000000000000000000000000000.0f;
            pairMaterial.MaximumRecoveryVelocity = 24.0f;
            pairMaterial.SpringSettings = new SpringSettings(1, 10);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(Simulation simulation)
        {

        }
    }
}
