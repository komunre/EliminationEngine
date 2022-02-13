using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;
using EliminationEngine.Render;

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
            var tcomp = tobj.AddComponent<TextComponent>();
            tobj.Position = new OpenTK.Mathematics.Vector3(-100, 0, 0);
            tobj.AddComponent<RotateDemoComponent>();
            tcomp.Font = new SharpFont.Face(Engine.GetSystem<TextSystem>().Lib, "res/Oswald-Regular.ttf");
            tcomp.Text = "Hello text!";
            tcomp.Changed = true;
            tcomp.OnScreen = true;
            tcomp.Size = 40;

            Engine.AddGameObject(tobj);
        }
    }
}
