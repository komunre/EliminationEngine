using EliminationEngine.GameObjects;
using EliminationEngine.Tools;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EliminationEngine.Render
{
    public class UISystem : EntitySystem
    {
        public UISystem(Elimination e) : base(e)
        {
        }

        public override void PostLoad()
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

        public override void OnUpdate()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            var camera = cameras.ElementAt(0);

            var widgets = Engine.GetObjectsOfType<UIWidget>();
            if (widgets == null) return;
            foreach (var widget in widgets)
            {
                if (widget.Changed)
                {
                    widget.RegenerateAll(camera);
                }
            }

            if (Engine.MouseState.IsButtonDown(MouseButton.Left))
            {
                var pos = Engine.MouseState.Position;
                pos.X /= camera.Width;
                pos.Y /= camera.Height;

                foreach (var widget in widgets)
                {
                    if (!Engine.GetCursorLockState())
                    {
                        var objPos = widget.Owner.GlobalPosition;
                        var objScale = widget.Owner.GlobalScale;
                        objPos.Y -= objScale.Y;

                        if (widget.OnScreen)
                        {
                            if ((pos.X > objPos.X && pos.Y > objPos.Y) && (pos.X < objPos.X + objScale.X && pos.Y < objPos.Y + objScale.Y))
                            {
                                if (!widget.Pressed)
                                {
                                    widget.OnClickCall();
                                }
                                widget.Pressed = true;
                                continue;
                            }
                        }
                        else
                        {
                            // Raycast here
                        }
                    }
                    else
                    {
                        var resList = Engine.GetSystem<Raycast>().RaycastFromObject(camera.Owner);
                        var res = resList[0];
                        if (res.Hit && res.HitObject != null)
                        {
                            if (res.HitObject.TryGetComponent<UIWidget>(out var worldWidget))
                            {
                                worldWidget.OnClickCall();
                            }
                        }
                    }

                    widget.Pressed = false;
                }
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();
        }
    }
}
