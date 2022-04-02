using EliminationEngine;
using EliminationEngine.GameObjects;
using Gwen.Net.Control;
using Gwen.Net.Renderer;
using Gwen.Net;

namespace EliminationEngine.Render.UI
{
    public class GwenSystem : EntitySystem
    {
        private Gwen.Net.OpenTk.Renderers.OpenTKGL40Renderer Renderer = new Gwen.Net.OpenTk.Renderers.OpenTKGL40Renderer();
        public Gwen.Net.Skin.TexturedBase GwenSkin;
        public Canvas GwenCanvas;
        public Gwen.Net.OpenTk.Input.OpenTkInputTranslator Input;
        public GwenSystem(Elimination e) : base(e)
        {
            GwenSkin = new Gwen.Net.Skin.TexturedBase(Renderer, "res/Default2.png");
            GwenSkin.DefaultFont = new Gwen.Net.Font(Renderer, "Arial");
            GwenCanvas = new Canvas(GwenSkin);
            GwenCanvas.Cursor = Cursor.Normal;
            Input = new Gwen.Net.OpenTk.Input.OpenTkInputTranslator(GwenCanvas);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            GwenCanvas.SetSize(800, 600);
            GwenCanvas.ShouldDrawBackground = true;
            GwenCanvas.BackgroundColor = GwenSkin.Colors.ModalBackground;
        }

        private void Window_TextInput(OpenTK.Windowing.Common.TextInputEventArgs obj)
        {
            Input.ProcessTextInput(obj);
        }

        private void Window_MouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            Input.ProcessMouseMove(obj);
        }

        private void Window_MouseButton(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            Input.ProcessMouseButton(obj);
        }

        private void Window_MouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            Input.ProcessMouseWheel(obj);
        }

        private void Window_KeyUp(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            Input.ProcessKeyUp(obj);
        }

        private void Window_KeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            Input.ProcessKeyDown(obj);
        }

        public override void PostLoad()
        {
            base.PostLoad();

            Engine.window.KeyDown += Window_KeyDown;
            Engine.window.KeyUp += Window_KeyUp;
            Engine.window.MouseWheel += Window_MouseWheel;
            Engine.window.MouseDown += Window_MouseButton;
            Engine.window.MouseMove += Window_MouseMove;
            Engine.window.MouseUp += Window_MouseButton;
            Engine.window.TextInput += Window_TextInput;
        }

        public override void OnDraw()
        {
            base.OnDraw();

            GwenCanvas.RenderCanvas();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            
        }
    }
}
