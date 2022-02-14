using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;
using EliminationEngine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AttackGame
{
    internal class TextTestSystem : EntitySystem
    {
        public TextTestSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();

            var tobj = new GameObject();
            var tcomp = tobj.AddComponent<UIWidget>();
            var sprGen = tobj.AddComponent<SpriteGenerator>();
            tcomp.Text = "Hello World!";
            tcomp.Size = 80;
            tcomp.Font = new SharpFont.Face(new SharpFont.Library(), "res/Oswald-Regular.ttf");
            tcomp.OnClick += () =>
            {
                Logger.Info("Hello World text was pressed");
            };
            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            var camera = cameras.ElementAt(0);
            tcomp.RegenerateAll(camera);

            tobj.Scale = new OpenTK.Mathematics.Vector3(1, 0.4f, 1);
            tobj.Position = new OpenTK.Mathematics.Vector3(-1, 0, 0);

            Engine.AddGameObject(tobj);

            var imageUI = new GameObject();
            var uicomp = imageUI.AddComponent<UIWidget>();
            uicomp.TextImage = (Image<Rgba32>)Image.Load("res/basic.png");
            uicomp.OnClick += () =>
            {
                Logger.Info("Image UI element was pressed");
            };
            uicomp.RegenerateAll(camera);
            Engine.AddGameObject(imageUI);
        }
    }
}
