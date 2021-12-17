using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using EliminationEngine.GameObjects;
using System.Globalization;

namespace EliminationEngine
{
    public class EliminationWindow : GameWindow
    {
        public List<GameObject> GameObjects = new();
        public EliminationWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Viewport(0, 0, 800, 600);

            var obj = new GameObject();
            var data = ModelParser.ParseObj("res/test.obj");
            var vertsArr = new List<float>();
            var indices = new List<int>();

            ModelHelper.AddObjMeshToObject(data, "res/basic.png", ref obj);

            GameObjects.Add(obj);

            var triangle = new GameObject();
            triangle.AddComponent<Mesh>();
            var trMesh = triangle.GetComponent<Mesh>();
            trMesh.Vertices = new List<float> { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 1.0f, 0.0f };
            trMesh.Indices = new List<int> { 0, 1, 2 };

            trMesh.LoadMesh("res/basic.png");

            GameObjects.Add(triangle);
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            foreach (var gameObject in GameObjects)
            {
                if (gameObject.TryGetComponent<Mesh>(out var mesh))
                {
                    mesh.DrawMesh();
                }
            }

            SwapBuffers();
        }
    }
}
