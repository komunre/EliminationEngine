using OpenTK.Windowing.Common;

namespace EliminationEngine.GameObjects
{
    public class EntitySystem
    {
        public Elimination Engine;
        public bool RunsWhilePaused = false;
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

        public virtual void OnObjectAdded(GameObject obj)
        {

        }

        public virtual void OnWindowResize(ResizeEventArgs args)
        {

        }

        public virtual void OnTextInput(TextInputEventArgs e)
        {

        }
    }
}
