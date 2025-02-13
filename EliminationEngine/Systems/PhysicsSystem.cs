﻿using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using EliminationEngine.Extensions;
using EliminationEngine.GameObjects;
using EliminationEngine.Physics;
using EliminationEngine.Tools;
using EliminationEngine.Tools.Physics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EliminationEngine.Systems
{
    public class PhysicsSystem : EntitySystem
    {
        public static PhysicsSystem? InstanceSelf;

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
        public Dictionary<GameObject, StaticHandle> StaticBodies = new();

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

            InstanceSelf = this;
        }

        public override void PostLoad()
        {
            base.PostLoad();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public override void OnDraw()
        {
            base.OnDraw();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OpenTK.Mathematics.Vector3 GetObjectVelocity(GameObject obj)
        {
            return GetObjectVelocity(GetObjectHandle(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OpenTK.Mathematics.Vector3 GetObjectVelocity(BodyHandle handle)
        {
            return PhysicsSimulation.Bodies.GetBodyReference(handle).Velocity.Linear.ToOpenTK();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BodyHandle AddPhysicsObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            mesh.ComputeClosedInertia(mass, out var inertia);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), inertia, new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StaticHandle AddStaticBoxObject(GameObject obj, float width, float height, float length)
        {
            if (!Initialized) return new StaticHandle(-1);
            var box = new Box(width, height, length);
            var meshIndex = PhysicsSimulation.Shapes.Add(box);
            var handle = PhysicsSimulation.Statics.Add(new StaticDescription(new RigidPose(obj.GlobalPosition.ToNumerics() + new Vector3(width/2, height/2, length/3), obj.Rotation.ToNumerics()), new CollidableDescription(meshIndex)));
            StaticBodies.Add(obj, handle);
            return handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BodyHandle AddKinematicObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh)
        {
            if (!Initialized) return new BodyHandle(-1);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), new CollidableDescription(meshIndex), new BodyActivityDescription(0.001f)));
            ObjectBodies.Add(obj, handle);
            return handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StaticHandle AddStaticObject(GameObject obj, BepuPhysics.Collidables.Mesh mesh)
        {
            if (!Initialized) return new StaticHandle(-1);
            var meshIndex = PhysicsSimulation.Shapes.Add(mesh);
            var handle = PhysicsSimulation.Statics.Add(new StaticDescription(new RigidPose(obj.GlobalPosition.ToNumerics(), obj.Rotation.ToNumerics()), new CollidableDescription(meshIndex)));
            StaticBodies.Add(obj, handle);
            return handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BodyHandle AddPhysicsObject(GameObject obj, Render.Mesh mesh, float mass)
        {
            if (!Initialized) return new BodyHandle(-1);
            return AddPhysicsObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool), mass);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BodyHandle AddKinematicObject(GameObject obj, Render.Mesh mesh)
        {

            if (!Initialized) return new BodyHandle(-1);
            return AddKinematicObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StaticHandle AddStaticObject(GameObject obj, Render.Mesh mesh)
        {
            if (!Initialized) return new StaticHandle(-1);
            return AddStaticObject(obj, EliminationMeshToBepuMesh.Convert(mesh, PhysicsSimulation.BufferPool));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDistanceConstraint(BodyHandle handle1, BodyHandle handle2, float minDist, float maxDist, float springFreq, float springDamp)
        {
            PhysicsSimulation.Solver.Add(handle1, handle2, new DistanceLimit(Vector3.Zero, Vector3.Zero, minDist, maxDist, new SpringSettings(springFreq, springDamp)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFixedDistanceConstraint(BodyHandle handle1, BodyHandle handle2, float distance)
        {
            AddDistanceConstraint(handle1, handle2, 0, distance, 0.01f, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveObject(GameObject obj)
        {
            if (!Initialized) return;
            if (ObjectBodies.ContainsKey(obj))
            {
                var handle = ObjectBodies[obj];
                PhysicsSimulation.Bodies.Remove(handle);
                ObjectBodies.Remove(obj);
            }
            else if (StaticBodies.ContainsKey(obj))
            {
                var handle = StaticBodies[obj];
                PhysicsSimulation.Statics.Remove(handle);
                StaticBodies.Remove(obj);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateObjectFromSimulationInfo(GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(GetObjectHandle(obj));
            var pose = bodyRef.Pose;
            obj.Position = pose.Position.ToOpenTK();
            obj.Rotation = pose.Orientation.ToOpenTK();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OverrideObjectPoseWithValues(BodyHandle handle, Vector3 position, Quaternion orientation)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            pose.Position = position;
            pose.Orientation = orientation;
            bodyRef.Pose = pose;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OverrideObjectPose(BodyHandle handle, GameObject obj)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            var pose = bodyRef.Pose;
            pose.Position = obj.Position.ToNumerics();
            pose.Orientation = obj.Rotation.ToNumerics();
            bodyRef.Pose = pose;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddObjectVelocity(BodyHandle handle, Vector3 velocity)
        {
            if (!Initialized) return;
            var bodyRef = PhysicsSimulation.Bodies.GetBodyReference(handle);
            bodyRef.Awake = true;
            var pose = bodyRef.Pose;
            bodyRef.Velocity.Linear += velocity * CurrentTimeStep;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RayHit[] RaycastFromPos(OpenTK.Mathematics.Vector3 pos, OpenTK.Mathematics.Vector3 dir, float maxDist = 1000, uint maxHits = 2)
        {
            DefaultRayHitHandler.LastResultData = new List<RayHit>();
            var handler = new DefaultRayHitHandler();
            PhysicsSimulation.RayCast(pos.ToNumerics(), dir.ToNumerics(), maxDist, ref handler);
            var array = new RayHit[DefaultRayHitHandler.LastResultData.Count];
            DefaultRayHitHandler.LastResultData.CopyTo(array);
            DefaultRayHitHandler.LastResultData = null;
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BodyHandle GetObjectHandle(GameObject obj)
        {
            return ObjectBodies[obj];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject GetObjectFromHandle(BodyHandle handle)
        {
            return ObjectBodies.Where((pair) => pair.Value == handle).FirstOrDefault().Key;
        }

        public class DefaultRayHitHandler : IRayHitHandler
        {

            public static List<RayHit>? LastResultData = null;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowTest(CollidableReference collidable)
            {
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                GameObject? hitObject;
                Logger.Info(Loc.Get("COLLIDABLE_MOBILITY") + collidable.Mobility.ToString());
                if (collidable.Mobility == CollidableMobility.Dynamic)
                {
                    hitObject = InstanceSelf.GetObjectFromHandle(collidable.BodyHandle);
                }
                else
                {
                    hitObject = InstanceSelf.StaticBodies.Where(pair => pair.Value == collidable.StaticHandle).FirstOrDefault().Key;
                }
                Logger.Info(Loc.Get("RAYCAST_OBJECT_HIT_ID") + hitObject.Id);
                var hit = new RayHit(true, hitObject, t, ray.Origin.ToOpenTK(), (ray.Origin + ray.Direction * t).ToOpenTK(), ray.Direction.ToOpenTK());
                LastResultData.Add(hit);
            }
        }
    }
}
