using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpGLTF.Schema2;
using EliminationEngine.GameObjects;

namespace EliminationEngine
{
    public static class ModelHelper
    {
        public static void AddObjMeshToObject(ModelParser.ObjData data, string texture, ref GameObject obj)
        {
            var vertsArr = new List<float>();
            var indices = new List<int>();
            var texCoords = new List<float>();

            foreach (var vert in data.Vertices)
            {
                vertsArr.AddRange(new float[] { vert.X, vert.Y, vert.Z });
            }

            foreach (var face in data.Faces)
            {
                foreach (var vert in face.Vertices)
                {
                    indices.Add(vert - 1);
                }
            }

            foreach (var coord in data.TextureCoords)
            {
                texCoords.AddRange(new float[] { coord.X, coord.Y });
            }

            obj.AddComponent<GameObjects.Mesh>();
            var mesh = obj.GetComponent<GameObjects.Mesh>();
            mesh.Vertices = vertsArr;
            mesh.Indices = indices;
            mesh.TexCoords = texCoords;

            mesh.LoadMesh(texture);
        }
    }
    public static class ModelParser
    {
        public class ObjData
        {
            public class TextureCoord
            {
                public float X;
                public float Y;
                public int U;
            }
            public class FaceData
            {
                public List<int> Vertices = new();
            }
            public List<Vector3> Vertices = new();
            public List<Vector3> Normals = new();
            public List<TextureCoord> TextureCoords = new();
            public List<FaceData> Faces = new();
        }

        public static ObjData ParseObj(string path)
        {
            var data = new ObjData();

            var reader = new StreamReader(File.OpenRead(path));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null && line != "")
                {
                    var type = line[0];
                    var realType = new string(new char[] { type });
                    if (line[1] != ' ')
                    {
                        realType = new string(new char[] { line[0], line[1] });
                    }
                    switch (realType)
                    {
                        case "v":
                            var split = line.Split(' ');
                            var vec = new Vector3((float)double.Parse(split[1], new CultureInfo("en-US")), (float)double.Parse(split[2], new CultureInfo("en-US")), (float)double.Parse(split[3], new CultureInfo("en-US")));
                            Console.WriteLine(vec.X + ":" + vec.Y + ":" + vec.Z);
                            data.Vertices.Add(vec);
                            break;
                        case "f":
                            var fsplit = line.Split(' ');
                            var facesData = fsplit.Skip(1);
                            var face = new ObjData.FaceData();
                            foreach (var faceDat in facesData)
                            {
                                face.Vertices.Add(int.Parse(faceDat.Split('/')[0]));
                            }
                            data.Faces.Add(face);
                            break;
                        case "vt":
                            var tsplit = line.Split(' ');
                            var x = float.Parse(tsplit[1], new CultureInfo("en-US"));
                            var y = float.Parse(tsplit[2], new CultureInfo("en-US"));
                            //var u = int.Parse(tsplit[3].Replace("[", "").Replace("]", ""));

                            var coord = new ObjData.TextureCoord();
                            coord.X = x;
                            coord.Y = y;
                            //coord.U = u;

                            data.TextureCoords.Add(coord);
                            break;
                        default:
                            break;
                    }
                }
            }

            return data;
        }

        public class GLTFData
        {

        }

        public static GLTFData ParseGLTF(string path)
        {
            return new GLTFData(); // dummy
        }

        public static void ParseGLTFExternal(string path)
        {
            var model = ModelRoot.Load(path);
            var scene = model.DefaultScene;
            foreach (var node in scene.VisualChildren)
            {
                var weh = node.Skin.GetJoint(0);
                foreach (var primitive in node.Mesh.Primitives)
                {
                    
                }
            }
        }
    }
}
