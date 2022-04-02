using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;
using EliminationEngine.Render.UI;

namespace AttackGame
{
    internal class UITestSystem : EntitySystem
    {
        public UITestSystem(Elimination e) : base(e)
        {

        }

        public override void PostLoad()
        {
            base.PostLoad();

            var canvas = Engine.GetSystem<GwenSystem>().GwenGui.Root;
            var window = new Gwen.Net.Control.Window(canvas);
            var label = new Gwen.Net.Control.Label(window);
            label.Text = "Hello World!";
        }
    }
}
