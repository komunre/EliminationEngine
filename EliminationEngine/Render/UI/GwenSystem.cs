using EliminationEngine;
using EliminationEngine.GameObjects;
using Gwen.Net.Control;
using Gwen.Net.Renderer;
using Gwen.Net;
using Gwen.Net.Platform;
using Gwen.Net.OpenTk;
using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine.Render.UI
{
    public class GwenSystem : EntitySystem
    {
        public IGwenGui GwenGui;

        private bool Initialized = false;
        public GwenSystem(Elimination e) : base(e)
        {
            GwenGui = GwenGuiFactory.CreateFromGame(Engine.window, GwenGuiSettings.Default.From(settings =>
            {
                settings.SkinFile = new FileInfo("res/DefaultSkin2.png");
                settings.DrawBackground = false;
            }));

            GwenGui.Load();
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

            GL.Disable(EnableCap.CullFace);
            GwenGui.Render();
            GL.Enable(EnableCap.CullFace);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            
        }
    }
}
