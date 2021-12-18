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

            //foreach (var vert in data.Vertices)
            //{
            //    vertsArr.AddRange(new float[] { vert.X, vert.Y, vert.Z });
            //}

            foreach (var face in data.Faces)
            {
                var faceVerts = new int[face.Vertices.Count];
                face.Vertices.CopyTo(faceVerts);
                var faceVertsList = faceVerts.Reverse();
                foreach (var vert in faceVertsList)
                {
                    indices.Add(vert - 1);
                    var vertData = data.Vertices[vert - 1];
                    vertsArr.AddRange(new float[] { vertData.X, vertData.Y, vertData.Z });
                }
                var faceTex = new int[face.TextureCoords.Count];
                face.TextureCoords.CopyTo(faceTex);
                var faceTexList = faceTex.Reverse();
                foreach (var coord in faceTexList)
                {
                    var realCoord = data.TextureCoords[coord - 1];
                    texCoords.AddRange(new float[] { realCoord.X, realCoord.Y });
                }
            }

            obj.AddComponent<GameObjects.Mesh>();
            var mesh = obj.GetComponent<GameObjects.Mesh>();
            mesh.Vertices = vertsArr;
            mesh.Indices = indices;
            mesh.TexCoords = texCoords;

            mesh.LoadMesh(texture);
        }

        public static void AddGLTFMeshToObject(ModelParser.GLTFData data, string texture, ref GameObject obj)
        {
            var vertsArr = new List<float>();
            var indices = new List<int>();
            var texCoords = new List<float>();

            foreach (var meshData in data.Meshes)
            {
                foreach (var prim in meshData.Primitives)
                {
                    foreach (var ind in prim.Indices)
                    {
                        var vert = prim.Vertices[(int)ind];
                        vertsArr.AddRange(new float[] { vert.X, vert.Y, vert.Z });
                        var coord = prim.UVs[(int)ind];
                        texCoords.AddRange(new float[] { coord.X, coord.Y });
                    }
                    //foreach (var coord in prim.UVs)
                    //{
                    //    texCoords.AddRange(new float[] { coord.X, coord.Y });
                    //}
                }
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
                public List<int> TextureCoords = new();
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
                            data.Vertices.Add(vec);
                            break;
                        case "f":
                            var fsplit = line.Split(' ');
                            var facesData = fsplit.Skip(1);
                            var face = new ObjData.FaceData();
                            foreach (var faceDat in facesData)
                            {
                                face.Vertices.Add(int.Parse(faceDat.Split('/')[0]));
                                face.TextureCoords.Add(int.Parse(faceDat.Split('/')[1]));
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
            public class PrimitiveData
            {
                public List<Vector3> Vertices = new();
                public List<Vector2> UVs = new();
                public List<uint> Indices = new();
            }
            public class MeshData
            {
                public List<float> Weights = new();
                public List<PrimitiveData> Primitives = new();
            }
            public List<MeshData> Meshes = new();
        }

        [Obsolete]
        public static GLTFData ParseGLTF(string path)
        {
            return new GLTFData(); // dummy
        }

        public static GLTFData ParseGLTFExternal(string path)
        {
            var modelData = new GLTFData();

            var model = ModelRoot.Load(path);
            var scene = model.DefaultScene;
            foreach (var node in scene.VisualChildren)
            {
                var meshData = new GLTFData.MeshData();
                var weights = node.Mesh.MorphWeights;
                meshData.Weights = weights.ToList();
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var prim = new GLTFData.PrimitiveData();
                    var verts = primitive.GetVertices("POSITION");
                    var indices = primitive.GetIndices();
                    prim.Indices = indices.ToList();
                    foreach (var vert in verts.AsVector3Array())
                    {
                        prim.Vertices.Add(new Vector3(vert.X, vert.Y, vert.Z));
                    }
                    foreach (var coord in primitive.GetVertices("TEXCOORD_0").AsVector2Array())
                    {
                        prim.UVs.Add(new Vector2(coord.X, coord.Y));
                    }
                    meshData.Primitives.Add(prim);
                }
                modelData.Meshes.Add(meshData);
            }

            return modelData;
        }
    }
}
