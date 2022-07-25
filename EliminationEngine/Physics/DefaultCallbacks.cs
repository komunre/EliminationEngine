using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using System.Numerics;

namespace EliminationEngine.Physics
{
    public struct DefaultPoseIntegrator : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;

        public Vector3Wide GravityDt;

        public AngularIntegrationMode AngularIntegrationMode => throw new NotImplementedException();

        public bool AllowSubstepsForUnconstrainedBodies => throw new NotImplementedException();

        public bool IntegrateVelocityForKinematics => throw new NotImplementedException();

        public void Initialize(Simulation simulation)
        {

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
            throw new NotImplementedException();
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            throw new NotImplementedException();
        }

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            throw new NotImplementedException();
        }

        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize(Simulation simulation)
        {
            throw new NotImplementedException();
        }
    }
}
