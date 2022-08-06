using OpenTK.Windowing.Common;

namespace EliminationEngine.GameObjects
{
    public class EntitySystem
    {
        public Elimination Engine;
        public EntitySystem(Elimination e)
        {
            Engine = e;
        }
        public virtual void OnLoad()
        {

        }

        public virtual void PostLoad()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnDraw()
        {

        }

        public virtual void OnTextInput(TextInputEventArgs e)
        {

        }
    }
}
