using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    public static class StreamReaderExt
    {
        public static float ReadF32(this StreamReader reader)
        {
            var f4data = new char[4];
            reader.Read(f4data, 0, 4);
            return BitConverter.ToSingle(new byte[] { (byte)f4data[0], (byte)f4data[1], (byte)f4data[2], (byte)f4data[3] });
        }

        public static float ReadI16(this StreamReader reader)
        {
            var i2data = new char[2];
            reader.Read(i2data, 0, 2);
            return BitConverter.ToInt16(new byte[] { (byte)i2data[0], (byte)i2data[1] });
        }

        public static bool ReadBoolean(this StreamReader reader)
        {
            var bdata = new char[1];
            reader.Read(bdata, 0, 1);
            return bdata[0] == 1 ? true : false;
        }

        public static double ReadDouble(this StreamReader reader)
        {
            var f8data = new char[8];
            reader.Read(f8data, 0, 4);
            return BitConverter.ToDouble(new byte[] { (byte)f8data[0], (byte)f8data[1], (byte)f8data[2], (byte)f8data[3], (byte)f8data[4], (byte)f8data[5], (byte)f8data[6], (byte)f8data[7] });
        }

        public static int ReadI32(this StreamReader reader)
        {
            var i4data = new char[4];
            reader.Read(i4data, 0, 4);
            return BitConverter.ToInt32(new byte[] { (byte)i4data[0], (byte)i4data[1], (byte)i4data[2], (byte)i4data[3] });
        }

        public static long ReadI64(this StreamReader reader)
        {
            var i8data = new char[8];
            reader.Read(i8data, 0, 4);
            return BitConverter.ToInt64(new byte[] { (byte)i8data[0], (byte)i8data[1], (byte)i8data[2], (byte)i8data[3], (byte)i8data[4], (byte)i8data[5], (byte)i8data[6], (byte)i8data[7] });
        }

        public static void Skip(this StreamReader reader, int length)
        {
            var useless = new char[length];
            reader.Read(useless, 0, length);
        }
    }
}
