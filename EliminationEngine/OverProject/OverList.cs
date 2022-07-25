using System.Collections;

namespace EliminationEngine.OverProject
{
    // Cringe, what even made me think this was a good idea?
    public class OverList<T> : IEnumerable<T>, IEnumerable
    {
        public T this[int index] { get => Container[index]; set => Container[index] = value; }

        public int Count => Container.Length;

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        private T[] Container = new T[0];

        public int Add(T item)
        {
            T[] copy = new T[Container.Length + 1];
            Container.CopyTo(copy, 0);
            var indx = Container.Length;
            copy[indx] = item;
            Container = copy;
            return indx;
        }

        public void Clear()
        {
            Container = new T[0];
        }

        public bool Contains(T item)
        {
            foreach (var obj in Container)
            {
                if (obj == null) continue;
                if (obj.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Container.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            Container.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)Container.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            for (var i = 0; i < Container.Length; i++)
            {
                var obj = Container[i];
                if (obj == null) continue;
                if (obj.Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            var copy = new T[Container.Length + 1];
            for (var i = 0; i < index; i++)
            {
                copy[i] = Container[i];
            }
            copy[index] = item;
            for (var i = index + 1; i < Container.Length; i++)
            {
                copy[i] = Container[i - 1];
            }
            Container = copy;
        }

        public bool Remove(T item)
        {
            var copy = new T[Container.Length - 1];
            for (var i = 0; i < Container.Length; i++)
            {
                var obj = Container[i];
                if (obj == null) continue;
                if (obj.Equals(item))
                {
                    copy[i] = Container[++i];
                }
                else
                {
                    copy[i] = Container[i];
                }
            }
            Container = copy;
            return true;
        }

        public void RemoveAt(int index)
        {
            var copy = new T[Container.Length - 1];
            for (var i = 0; i < index; i++)
            {
                copy[i] = Container[i];
            }
            for (var i = index + 1; i < Container.Length; i++)
            {
                copy[i] = Container[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)Container.GetEnumerator();
        }
    }
}
