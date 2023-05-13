using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Reflection;

namespace GrandEngine.FileParser
{
    public ref struct FileParser
    {
        private StreamReader _reader; 
        private FileParser(StreamReader reader)
        {
            this._reader = reader;
        }

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

        private dynamic ReadArray(Type valueType)
        {
            this._reader.Read(); // consuming bracket opening
            var type = valueType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));

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

        private object TryReadInt()
        {
            var numStr = this._reader.ReadWhile(char.IsNumber);
            return int.Parse(numStr);
        }

        private object ReadValue(Type valueType)
        {
            this._reader.ReadWhitespace();
            var c = (char)this._reader.Peek();

            return c switch
            {
                '{' => this.ReadObject(valueType),
                '[' => this.ReadArray(valueType),
                '}' => throw new InvalidDataException($"Unexpected closing bracket"),
                '\"' => this.ReadString(),
                _ => this.TryReadInt()
            };
        }

        private bool ReadProperty(Type instanceType, object instance)
        {
            this._reader.ReadWhitespace();
            if (this._reader.Peek() == -1) return false;

            var name = this._reader.ReadWhile((c) => c != '=');
            if (this._reader.Read() == -1) throw new InvalidDataException("Unexpected EOF");

            var prop = instanceType.GetProperty(name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (prop == null)
            {
                throw new InvalidDataException($"Cannot deserialize into this type! Unknown property: {name}");
            }

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
