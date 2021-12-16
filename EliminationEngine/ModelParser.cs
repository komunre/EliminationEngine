﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    public static class ModelParser
    {
        public class ObjData
        {
            public class TextureCoord
            {
                public int X;
                public int Y;
                public int Uv;
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
    }
}
