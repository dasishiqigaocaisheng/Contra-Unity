using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Utility.BidirectionalDict
{
    /// <summary>
    /// Represents a concurrent version of a bidirectional collection.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first values in the dictionary.</typeparam>
    /// <typeparam name="TSecond">The type of the second values in the dictionary.</typeparam>
    public class ConcurrentBiDictionary<TFirst, TSecond> : IBiDictionary<TFirst, TSecond>
                                                 where TFirst : notnull
                                                 where TSecond : notnull
    {
        /// <inheritdoc/>
        public bool IsSynced => Count == _secondToFirst.Count;

        /// <inheritdoc/>
        public int Count => _firstToSecond.Count;

        private readonly ConcurrentDictionary<TFirst, TSecond> _firstToSecond;
        private readonly ConcurrentDictionary<TSecond, TFirst> _secondToFirst;

        /// <summary>Initializes a new instance of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
        public ConcurrentBiDictionary()
        {
            _firstToSecond = new ConcurrentDictionary<TFirst, TSecond>();
            _secondToFirst = new ConcurrentDictionary<TSecond, TFirst>();
        }

        /// <summary>Initializes a new instance of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> class that contains elements copied from the specified <see cref="IEnumerable{T}" /> and uses the default equality comparer for the key type.</summary>
        /// <param name="collection">The <see cref="IEnumerable{T}" /> whose elements are copied to the new <see cref="ConcurrentBiDictionary{TFirst, TSecond}" />.</param>
        public ConcurrentBiDictionary(IEnumerable<KeyValuePair<TFirst, TSecond>> collection)
        {
            _firstToSecond = new ConcurrentDictionary<TFirst, TSecond>(collection);
            _secondToFirst = new ConcurrentDictionary<TSecond, TFirst>(collection.ToDictionary(k => k.Value, v => v.Key));
        }

        /// <summary>Initializes a new instance of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> class that is empty, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{T}" />.</summary>
        /// <param name="firstComparer">The <see cref="IEqualityComparer{TFirst}" /> implementation to use when comparing the first values, or <see langword="null" /> to use the default <see cref="IEqualityComparer{TFirst}" /> for the type of the key.</param>
        /// <param name="secondComparer">The <see cref="IEqualityComparer{TSecond}" /> implementation to use when comparing the first values, or <see langword="null" /> to use the default <see cref="IEqualityComparer{TSecond}" /> for the type of the key.</param>
        public ConcurrentBiDictionary(IEqualityComparer<TFirst> firstComparer, IEqualityComparer<TSecond> secondComparer)
        {
            _firstToSecond = new ConcurrentDictionary<TFirst, TSecond>(firstComparer);
            _secondToFirst = new ConcurrentDictionary<TSecond, TFirst>(secondComparer);
        }

        /// <inheritdoc/>
        public bool TryAdd(TFirst first, TSecond second)
        {
            if (!_firstToSecond.TryAdd(first, second))
            {
                return false;
            }

            if (!_secondToFirst.TryAdd(second, first))
            {
                _firstToSecond.TryRemove(first, out _);
                return false;
            }

            return true;
        }

        /// <summary>Uses the argument to add a value pair to the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> if the first value does not already exist, or to update a value pair in the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> if the first value already exists.</summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The second value of the pair</param>
        /// <returns>The second value for first value. This will be either the existing second value for the first value if the first value is already in the dictionary, or the new second value if the first value was not in the dictionary.</returns>
        public TSecond GetOrAdd(TFirst first, TSecond second)
        {
            var tempSecond = _firstToSecond.GetOrAdd(first, second);

            if (tempSecond.Equals(second))
            {
                _firstToSecond.GetOrAdd(first, second);
            }

            return tempSecond;
        }

        /// <summary>Uses the argument to add a value pair to the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> if the second value does not already exist, or to update a value pair in the <see cref="ConcurrentBiDictionary{TFirst, TSecond}" /> if the second value already exists.</summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The second value of the pair</param>
        /// <returns>The first value for second value. This will be either the existing first value for the second value if the second value is already in the dictionary, or the new first value if the second value was not in the dictionary.</returns>
        public TFirst GetOrAdd(TSecond second, TFirst first)
        {
            var tempFirst = _secondToFirst.GetOrAdd(second, first);

            if (tempFirst.Equals(first))
            {
                _firstToSecond.GetOrAdd(first, second);
            }

            return tempFirst;
        }

        /// <inheritdoc/>
        public void AddOrUpdate(TFirst first, TSecond second)
        {
            if (!_firstToSecond.TryAdd(first, second))
            {
                _firstToSecond[first] = second;
            }

            if (!_secondToFirst.TryAdd(second, first))
            {
                _secondToFirst[second] = first;
            }
        }

        /// <inheritdoc/>
        public bool TryRemove(TFirst first)
        {
            if (!_firstToSecond.TryRemove(first, out var second))
            {
                return false;
            }

            _secondToFirst.TryRemove(second, out _);

            return true;
        }

        /// <inheritdoc/>
        public bool TryRemove(TSecond second)
        {
            if (!_secondToFirst.TryRemove(second, out var first))
            {
                return false;
            }

            _firstToSecond.TryRemove(first, out _);

            return true;
        }

        /// <inheritdoc/>
        public bool Contains(TFirst first)
            => _firstToSecond.ContainsKey(first);

        /// <inheritdoc/>
        public bool Contains(TSecond second)
            => _secondToFirst.ContainsKey(second);

        /// <inheritdoc/>
        public void Clear()
        {
            _secondToFirst.Clear();
            _firstToSecond.Clear();
        }

        /// <inheritdoc/>
        public TSecond this[TFirst first]
            => _firstToSecond[first];

        /// <inheritdoc/>
        public TFirst this[TSecond second]
            => _secondToFirst[second];

        /// <inheritdoc/>
        public bool TryGet(TFirst first, out TSecond second)
            => _firstToSecond.TryGetValue(first, out second);

        /// <inheritdoc/>
        public bool TryGet(TSecond second, out TFirst first)
            => _secondToFirst.TryGetValue(second, out first);

        /// <summary>
        /// Gets the <see cref="IEnumerator{T}"/> of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/> of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}"/></returns>
        public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
            => _firstToSecond.GetEnumerator();

        /// <summary>
        /// Gets the <see cref="IEnumerator"/> of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/> of the <see cref="ConcurrentBiDictionary{TFirst, TSecond}"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
            => _firstToSecond.GetEnumerator();
    }
}
