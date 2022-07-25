namespace EliminationEngine.GameObjects
{
    public class RemovalTimer : EntityComponent
    {
        public float Timer = 2.0f;
        public RemovalTimer(GameObject owner) : base(owner)
        {

        }
    }
}
