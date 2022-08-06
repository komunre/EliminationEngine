using System.Data;

namespace EliminationEngine.Extensions
{
    public static class StringExtensions
    {
        public static SplitEnumerator SplitNoAlloc(this string s, char delimiter)
        {
            return new SplitEnumerator(s.AsSpan(), delimiter);
        }

        public static SplitEnumerator SplitNoAlloc(this ReadOnlySpan<char> s, char delimiter)
        {
            return new SplitEnumerator(s, delimiter);
        }

        public ref struct SplitEnumerator
        {
            private ReadOnlySpan<char> _span;
            private readonly char _delimiter;

            public SplitEnumerator(ReadOnlySpan<char> span, char delimiter)
            {
                this._span = span;
                this._delimiter = delimiter;
                this.Current = default;
            }

            public SplitEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                if (this._span.Length == 0) return false;

                var index = this._span.IndexOf(this._delimiter);
                if (index == -1)
                {
                    Current = _span;
                    _span = ReadOnlySpan<char>.Empty;
                    return true;
                }

                Current = _span.Slice(0, index);
                _span = _span.Slice(index + 1);
                return true;
            }

            public ReadOnlySpan<char> GetMove
            {
                get
                {
                    if (!this.MoveNext()) throw new DataException("EOF");
                    return Current;
                }
            }

            public ReadOnlySpan<char> Current { get; private set; }
        }
    }
}
