using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    public static class Logger
    {
        public static void LogVector(Vector3 vec)
        {
            Console.WriteLine(vec.X + ":" + vec.Y + ":" + vec.Z);
        }
    }
}
