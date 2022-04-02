// See https://aka.ms/new-console-template for more information
using AttackGame;
using AttackGame.Game;
using EliminationEngine;
using EliminationEngine.GameObjects;

Console.WriteLine("Hello, World!");
var engine = new Elimination(800, 600);
<<<<<<< HEAD
engine.RegisterSystem<GameStateSystem>();
engine.RegisterSystem<HowarhSystem>();
engine.RegisterSystem<SpawnSystem>();
engine.RegisterSystem<CameraSystem>();
engine.RegisterSystem<MouseControlsSystem>();
=======
var camera = new GameObject();
camera.AddComponent<CameraComponent>();
engine.AddGameObject(camera);
//engine.RegisterSystem<GameStateSystem>();
//engine.RegisterSystem<HowarhSystem>();
//engine.RegisterSystem<SpawnSystem>();
//engine.RegisterSystem<CameraSystem>();
//engine.RegisterSystem<MouseControlsSystem>();
//engine.RegisterSystem<GameSystem>();
>>>>>>> 4f6faa3 (oh god help me with ui)
engine.Run();
