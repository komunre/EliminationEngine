using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EliminationEngine.Tools
{
    public ref struct FileParser
    {
        private StreamReader _reader; 
        private FileParser(StreamReader reader)
        {
            this._reader = reader;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private object ReadObject(Type valueType)
        {
            this._reader.Read(); // consuming bracket opening
            var instance = Activator.CreateInstance(valueType);
            while(this._reader.Peek() != '}')
            {
                if (this._reader.Peek() == -1) throw new InvalidDataException("Unexpected EOF");
                if (!this.ReadProperty(valueType, instance)) throw new InvalidDataException("Unexpected EOF");
                this._reader.ReadWhitespace();
            }
            this._reader.ReadWhitespace();
            if (this._reader.Read() != '}') throw new InvalidDataException("Unmatched bracket");
            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private dynamic ReadArray(Type valueType)
        {
            this._reader.Read(); // consuming bracket opening
            var type = valueType.GetGenericArguments()[0];
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
            if (list == null) throw new Exception("Activator returned null in FileParser");

            while(this._reader.Peek() != ']')
            {
                if (this._reader.Peek() == -1) throw new InvalidDataException("Unexpected EOF");
                this._reader.ReadWhitespace();
                list.Add(ReadValue(type));
                this._reader.ReadVoidWhile((c) => (c == ',' || char.IsWhiteSpace(c)));
            }
            this._reader.ReadWhitespace();
            if (this._reader.Read() != ']') throw new InvalidDataException("Unmatched bracket");
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private string ReadString()
        {
            this._reader.Read(); // consuming quote opening
            var builder = new StringBuilder();
            while(this._reader.Peek() != '"')
            {
                if (this._reader.Peek() == -1) throw new InvalidDataException("Unexpected EOF");
                builder.Append((char)this._reader.Read());
            }
            if (this._reader.Read() != '"') throw new InvalidDataException("Unmatched string quote");
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private object TryReadInt()
        {
            var numStr = this._reader.ReadWhile(char.IsNumber);
            return int.Parse(numStr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining  | MethodImplOptions.AggressiveOptimization)]
        private object ReadValue(Type valueType)
        {
            this._reader.ReadWhitespace();
            var c = (char)this._reader.Peek();

            return c switch
            {
                '{' => this.ReadObject(valueType),
                '[' => this.ReadArray(valueType),
                '}' => throw new InvalidDataException($"Unexpected closing bracket"),
                '\'' => this.ReadString(),
                '\"' => this.ReadString(),
                _ => this.TryReadInt()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private bool ReadProperty(Type instanceType, object instance)
        {
            Logger.Info("FileParser: Reading property");
            this._reader.ReadWhitespace();
            if (this._reader.Peek() == -1) return false;

            var name = this._reader.ReadWhile((c) => c != '=');
            if (this._reader.Read() == -1) throw new InvalidDataException("Unexpected EOF");

            Logger.Info("FileParser: Getting property type");
            var prop = instanceType.GetProperty(name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
            {
                Logger.Error($"Cannot deserialize into this type! Unknown property: {name}");
                return false;
            }

            Logger.Info("FileParser: Reading value");
            var val = this.ReadValue(prop.PropertyType);
            prop.SetValue(instance, val);
            return true;
        }

        private object Deserialize(Type type)
        {
            object instance = Activator.CreateInstance(type);

            while(this._reader.Peek() != -1)
            {
                if (!this.ReadProperty(type, instance)) return instance;
            }
            return instance;
        }

        public static TClass Deserialize<TClass>(string s)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(s));
            using var reader = new StreamReader(ms);
            return Deserialize<TClass>(reader);
        }

        public static TClass Deserialize<TClass>(StreamReader s)
        {
            return (TClass) new FileParser(s).Deserialize(typeof(TClass));
        }
    }
}
