using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using ImGuiNET;

namespace EliminationEngine.GameObjects
{
    public class EditorSystem : EntitySystem
    {
        public string LoadedMap = "UNLOADED";
        public bool EditorActive = false;
        public float EditorCameraSpeed = 4.5f;

        protected CameraComponent EditorCamera;
        protected bool _lastActive = false;

        protected CameraComponent? _lastActiveCamera = null;

        public EditorSystem(Elimination e) : base(e)
        {

        }

        public override void OnLoad()
        {
            base.OnLoad();

            var editCamObj = new GameObject();
            EditorCamera = editCamObj.AddComponent<CameraComponent>();
        }

        public override void OnDraw()
        {
            base.OnDraw();

            ImGui.Begin("Editor");
            if (ImGui.CollapsingHeader("Objects"))
            {

            }
            ImGui.End();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (_lastActive != EditorActive)
            {
                _lastActive = EditorActive;
                if (EditorActive)
                {
                    foreach (var cam in Engine.GetObjectsOfType<CameraComponent>())
                    {
                        if (cam.Active)
                        {
                            _lastActiveCamera = cam;
                            _lastActiveCamera.Active = false;
                            break;
                        }
                    }
                }
                else
                {
                    if (_lastActiveCamera != null)
                    {
                        _lastActiveCamera.Active = true;
                    }
                }
            }

            if (!EditorActive) return;

            var moveVector = Vector3.Zero;

            var playerDirections = EditorCamera.Owner.GetDirections();

            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                moveVector += -playerDirections[1];
            }
            if (Engine.KeyState.IsKeyDown(Keys.D))
            {
                moveVector += playerDirections[1];
            }
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                moveVector += playerDirections[0];
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                moveVector += -playerDirections[0];
            }

            EditorCamera.Owner.Position += moveVector * EditorCameraSpeed * Engine.DeltaTime;
        }
    }
}
