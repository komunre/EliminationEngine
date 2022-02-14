using EliminationEngine.GameObjects;
using EliminationEngine.Tools;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont;

namespace EliminationEngine.Render
{
    public class UISystem : EntitySystem
    {
        public UISystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnDraw();

            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            var camera = cameras.ElementAt(0);

            var widgets = Engine.GetObjectsOfType<UIWidget>();
            if (widgets == null) return;
            foreach (var widget in widgets)
            {
                widget.RegenerateAll(camera);
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();
        }
    }
}
