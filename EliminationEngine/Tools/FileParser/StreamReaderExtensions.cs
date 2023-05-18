using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Tools
{
    internal static class StreamReaderExtensions
    {
        public const uint MAX_WHILE_READS = 10000000;

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
            uint count = 0;

            var builder = new StringBuilder();
            while (check((char) reader.Peek()) && count < MAX_WHILE_READS)
            {
                builder.Append((char) reader.Read());
                count++;
            }
            if (count >= MAX_WHILE_READS) throw new Exception("Too much reads in ReadWhile of StreamReaderExtensions");

            return builder.ToString();
        }
    }
}
