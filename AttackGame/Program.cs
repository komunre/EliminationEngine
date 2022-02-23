// See https://aka.ms/new-console-template for more information
using AttackGame;
using AttackGame.Game;
using EliminationEngine;
using EliminationEngine.GameObjects;

Console.WriteLine("Hello, World!");
var engine = new Elimination(800, 600);
engine.RegisterSystem<GameStateSystem>();
engine.RegisterSystem<HowarhSystem>();
//engine.RegisterSystem<SpawnSystem>();
engine.RegisterSystem<CameraSystem>();
engine.RegisterSystem<MouseControlsSystem>();
engine.Run();
