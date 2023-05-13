using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrandEngine.FileParser
{
    internal static class StreamReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void ReadWhitespace(this StreamReader reader)
        {
            reader.ReadVoidWhile(char.IsWhiteSpace);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void ReadVoidWhile(this StreamReader reader, Func<char, bool> check)
        {
            while (check((char)reader.Peek()))
            {
                reader.Read();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string ReadWhile(this StreamReader reader, Func<char, bool> check)
        {
            var builder = new StringBuilder();
            while (check((char) reader.Peek()))
            {
                builder.Append((char) reader.Read());
            }

            return builder.ToString();
        }
    }
}
