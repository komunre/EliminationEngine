namespace EliminationEngine.Tools
{
    public class StreamHelper
    {
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[input.Length];
            input.Read(buffer);
            return buffer;
        }
    }
}
