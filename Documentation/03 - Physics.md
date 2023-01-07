# Physics

Physics is simulated using external BepuPhysics v2.0 physics engine as a library. It allows many different collision methodics, optimized, and uses `unsafe` code.

BepuPhysics is not limited to any certain shapes, and so it is allowed to load complex meshes into the simulation.

---

Elimination Engine serves physics system under `EliminationEngine.Systems.PhysicsSystem`.

variable `EliminationEngine.Physics.PhysicsSystem.Initialized` indicates if Physics System was initialized. Does not indicate if it was loaded correctly.

variable `EliminationEngine.Physics.PhysicsSystem.AutoUpdateObjects` sets if physics engine should automtaically update object transforms according to simulation. Manual approach is allowed by `UpdateObjectFromSimulationInfo`.

variable `EliminationEngine.Physics.PhysicsSystem.CurrentTimeStep` sets current timestep of the simulation. It is not recommended to change this value past loading stage.

variable `EliminationEngine.Physics.PhysicsSystem.InitFunc` sets the initialize function. Changing this variable allows to customize simulation callbacks and properties.

method `EliminationEngine.Physics.PhysicsSystem.AddPhysicsObject` adds dynamic object to the simulation. Mesh can be different ftom the primary model.

method `EliminationEngine.Physics.PhysicsSystem.AddStaticObjects` is same method as `AddPhysicsObject`, but adds static object, that is not allowed to move or have any velocity at all.