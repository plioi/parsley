using System;
using System.Collections.Generic;

namespace Parsley
{
    public class Stream<T>
    {
        private readonly T current;
        private readonly Lazy<Stream<T>> rest;

        public Stream(IEnumerator<T> enumerator)
        {
            if (!enumerator.MoveNext())
                throw new ArgumentException("Streams may not be empty.");

            current = enumerator.Current;
            rest = new Lazy<Stream<T>>(() => LazyAdvance(enumerator));
        }

        private Stream(T current, IEnumerator<T> enumerator)
        {
            this.current = current;
            rest = new Lazy<Stream<T>>(() => LazyAdvance(enumerator));
        }

        public T Current
        {
            get { return current; }
        }

        public Stream<T> Advance()
        {
            return rest.Value;
        }

        private Stream<T> LazyAdvance(IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
                return new Stream<T>(enumerator.Current, enumerator);

            return this;
        }
    }
}