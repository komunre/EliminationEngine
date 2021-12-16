﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FbxSharp;
using ObjLoader.Loader.Loaders;

namespace EliminationEngine.GameObjects
{
    public static class MeshLoader
    {
        public static void LoadMeshFromFbx(string path, ref GameObject obj)
        {
            obj.AddComponent<Mesh>();
            var mesh = obj.GetComponent<Mesh>();
            if (mesh == null) return;
            var fimporter = new Importer();
            var scene = fimporter.Import(path);

            foreach (var node in scene.Nodes)
            {
                var meshNode = (FbxSharp.Mesh)node.GetNodeAttributeByIndex(0);
                var verts = Enumerable.Range(0, meshNode.GetControlPointsCount()).Select(ix =>
                {
                    var cp = meshNode.GetControlPointAt((int)ix);
                    var baked = node.EvaluateGlobalTransform().MultNormalize(cp);
                    var pos = baked.ToVector3();
                    return new float[] { (float)pos.X, (float)pos.Y, (float)pos.Z };
                }).ToList();

                var realVerts = new List<float>();
                foreach (var vert in verts)
                {
                    realVerts.AddRange(vert);
                }

                mesh.Vertices = realVerts;
                mesh.LoadMesh();
            }
        }

        private static Stream LoadMaterial(string mat)
        {
            return File.OpenRead(Path.Combine("res/" + mat));
        }

        public static void LoadMeshFromObj(string path, ref GameObject obj)
        {
            obj.AddComponent<Mesh>();
            var mesh = obj.GetComponent<Mesh>();
            if (mesh == null) return;

            var objLoader = new ObjLoaderFactory().Create();
            var stream = new FileStream(path, FileMode.Open);
            var result = objLoader.Load(stream);

            var verts = new List<float>();
            foreach (var vert in result.Vertices)
            {
                var arr = new float[] { vert.X, vert.Y, vert.Z };
                Console.WriteLine(vert.X + ":" + vert.Y + ":" + vert.Z);
                verts.AddRange(arr);
            }

            mesh.Vertices = verts;
            mesh.LoadMesh();
        }
    }
}
