// See https://aka.ms/new-console-template for more information
using AttackGame;
using AttackGame.Game;
using EliminationEngine;
using EliminationEngine.GameObjects;

Console.WriteLine("Hello, World!");
var engine = new Elimination();
//var camera = new GameObject();
//camera.AddComponent<CameraComponent>();
//engine.AddGameObject(camera);
engine.RegisterSystem<UITestSystem>();
//engine.RegisterSystem<GameStateSystem>();
//engine.RegisterSystem<HowarhSystem>();
//engine.RegisterSystem<SpawnSystem>();
//engine.RegisterSystem<CameraSystem>();
//engine.RegisterSystem<MouseControlsSystem>();
engine.RegisterSystem<GameSystem>();
engine.Run();
