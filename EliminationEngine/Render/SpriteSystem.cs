using EliminationEngine;
using EliminationEngine.GameObjects;

namespace EliminationEngine.Render
{
    public class SpriteSystem : EntitySystem
    {
        public static List<GameObject> RegisteredSprites = new();

        public SpriteSystem(Elimination e) : base(e) {

        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        public override void PostLoad()
        {
            base.PostLoad();
        }

        public override void OnDraw()
        {
            base.OnDraw();


        }
    }
}
