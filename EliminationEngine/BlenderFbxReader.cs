using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    public class FbxData
    {
        public class Node
        {
            public List<Property> Props = new();
        }
        public class Property
        {
            public char Type = '0';
            public object? Data = null;
        }
        public List<Node> Nodes = new();
        public List<Vector3> Vertices = new();
        public List<Vector3> Normals = new();
    }
    public class BlenderFbxReader
    {
        public static FbxData LoadFbx(string path)
        {
            var reader = new StreamReader(path);
            var header = new char[27];
            reader.Read(header, 0, 27);

            var fbxData = new FbxData();
            var nodeCounter = 0;
            while (!reader.EndOfStream)
            {
                Console.WriteLine("Node #" + nodeCounter);
                var node = new FbxData.Node();
                var nodeData = new char[13]; // read all known data
                reader.Read(nodeData, 0, 13);
                var len = (int)nodeData[12]; // read name length
                var name = new char[len];
                reader.Read(name, 0, len); // read name
                Console.WriteLine("Prop name: " + new string(name));
                var propListLen = BitConverter.ToInt32(new byte[] { (byte)nodeData[8], (byte)nodeData[9], (byte)nodeData[10], (byte)nodeData[11] }); // read total properties count
                for (var i = 0; i < propListLen; i++)
                {
                    Console.WriteLine("Prop #" + i);
                    var type = new char[1]; // Read property type
                    reader.Read(type, 0, 1);
                    Console.WriteLine("Node type: " + type);

                    var prop = new FbxData.Property();
                    prop.Type = type[0];

                    switch (type[0])
                    {
                        case 'Y':
                            prop.Data = reader.ReadI16();
                            break;
                        case 'C':
                            prop.Data = reader.ReadBoolean();
                            break;
                        case 'I':
                            prop.Data = reader.ReadI32();
                            break;
                        case 'F':
                            prop.Data = reader.ReadF32();
                            break;
                        case 'D':
                            prop.Data = reader.ReadDouble();
                            break;
                        case 'L':
                            prop.Data = reader.ReadI64();
                            break;
                        case 'i':
                            var i4arrLen = reader.ReadI32();
                            var iarr = new int[i4arrLen];
                            reader.Skip(8);
                            for (var i4arr = 0; i4arr < i4arrLen; i4arr++)
                            {
                                iarr[i4arr] = reader.ReadI32();
                            }
                            prop.Data = iarr;
                            break;
                        case 'f':
                            var f4arrLen = reader.ReadI32();
                            var farr = new float[f4arrLen];
                            reader.Skip(8);
                            for (var f4arr = 0; f4arr < f4arrLen; f4arr++)
                            {
                                farr[f4arr] = reader.ReadF32();
                            }
                            prop.Data = farr;
                            break;
                        case 'd':
                            var d8arrLen = reader.ReadI32();
                            var darr = new double[d8arrLen];
                            reader.Skip(8);
                            for (var d8arr = 0; d8arr < d8arrLen; d8arr++)
                            {
                                darr[d8arr] = reader.ReadF32();
                            }
                            prop.Data = darr;
                            break;
                        case 'l':
                            var i8arrLen = reader.ReadI32();
                            var larr = new long[i8arrLen];
                            reader.Skip(8);
                            for (var i8arr = 0; i8arr < i8arrLen; i8arr++)
                            {
                                larr[i8arr] = reader.ReadI64();
                            }
                            prop.Data = larr;
                            break;
                        case 'b':
                            var barrLen = reader.ReadI32();
                            var barr = new bool[barrLen];
                            reader.Skip(8);
                            for (var i4arr = 0; i4arr < barrLen; i4arr++)
                            {
                                barr[i4arr] = reader.ReadBoolean();
                            }
                            prop.Data = barr;
                            break;
                        case 'S':
                            var slen = reader.ReadI32();
                            var str = new char[slen];
                            reader.Read(str, 0, slen);
                            prop.Data = new string(str);
                            break;
                        case 'R':
                            var blen = reader.ReadI32();
                            var bd = new char[blen];
                            reader.Read(bd, 0, blen);
                            var bytes = new byte[blen];
                            for (var bi = 0; bi < blen; bi++)
                            {
                                var b = (byte)bd[bi];
                                bytes[bi] = b;
                            }
                            prop.Data = bytes;
                            break;
                        case 'x': // I have no idea what it is
                            reader.Skip(12);
                            break;
                        default:
                            throw new InvalidDataException("Wrong fbx property type: " + new string(type));
                    }

                    if (prop.Data is char[])
                    {
                        Console.WriteLine("Data: " + new string(prop.Data as char[]));
                    }
                    else
                    {
                        Console.WriteLine("Data: " + prop.Data);
                    }
                    node.Props.Add(prop);
                }

                fbxData.Nodes.Add(node);
                nodeCounter++;
            }
            return fbxData;
        }
    }
}
