# Getting Started

Project uses .NET 6.0.

---

Starting the new program in Elimination Engine is manual, but relatively simplistic process.

In the new project, link to the EliminationEngine project, add `using EliminationEngine;` statement to the beginning of main file.

Create an instance of `Elimination` class, pass `args` of type `string[]` from main method arguments. `new Elimination(args);`.

Register new game system using `EliminationEngine.Elimination.RegisterSystem<EliminationEngine.GameObjects.EntitySystem>()`. In short form - `Elimination.RegisterSystem<EntitySystem()`, where EntitySystem is a child of `EliminationEngine.GameObjects.EntitySystem` (`EntitySystem`) class.

Unregister engine systems using `EliminationEngine.Elimination.UnregisterSystem<EliminationEngine.GameObjects.EntitySystem>()` if needed.

Set to headless if required, by setting `EliminationEngine.Elimination.Headless` to `true`.

Run the engine using `EliminationEngine.Elimination.Run()`.

---

Template Program:

```cs

using EliminationEngine;
using EliminationEngine.GameObjects;

var engine = new Elimination();
engine.RegisterSystem<YourGameSystem>();
engine.RegisterSystem<AnotherYourGameSystem>();
engine.Run();
```