using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;

namespace EliminationEngine.Render.UI
{
    public class InformaUI : EntitySystem
    {
        protected List<GameObject> UIElements = new List<GameObject>();

        public InformaUI(Elimination e) : base(e) { }

        public override void OnLoad()
        {

        }
    }
}
