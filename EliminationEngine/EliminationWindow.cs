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

            var obj = new GameObject();
            /*var data = ModelParser.ParseObj("res/test.obj");
            var vertsArr = new List<float>();
            foreach (var face in data.Faces)
            {
                foreach (var vert in face.Vertices)
                {
                    var vertData = data.Vertices.ElementAt(vert - 1);
                    vertsArr.AddRange(new float[] { vertData.X, vertData.Y, vertData.Z } );
                }
            }
            Console.WriteLine("==============");
            //foreach (var vert in vertsArr)z
            //{
            //    Console.WriteLine(vert);
            //}*/
            obj.AddComponent<Mesh>();
            var mesh = obj.GetComponent<Mesh>();
            //mesh.Vertices = vertsArr;
            mesh.Vertices = new List<float> { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 1.0f, 0.0f };
            mesh.Indices = new List<int> { 0, 1, 2 };
            mesh.LoadMesh();

            GameObjects.Add(obj);
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
