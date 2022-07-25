namespace EliminationEngine.GameObjects
{
    public class DebugRenderSystem : EntitySystem
    {
        public bool DebugActive = false;
        public DebugRenderSystem(Elimination e) : base(e)
        {

        }

        public override void OnDraw()
        {
            base.OnDraw();

        }

        public void DrawLine()
        {

        }
    }
}