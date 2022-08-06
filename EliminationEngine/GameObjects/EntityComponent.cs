namespace EliminationEngine.GameObjects
{
    public class EntityComponent
    {
        public GameObject Owner;
        public EntityComponent(GameObject owner)
        {
            Owner = owner;
        }
    }
}
