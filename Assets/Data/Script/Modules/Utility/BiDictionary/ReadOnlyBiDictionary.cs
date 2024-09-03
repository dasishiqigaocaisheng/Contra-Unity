using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Modules.Utility.BidirectionalDict
{
    /// <summary>
    /// Represents a read-only version of a bidirectional collection.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first values in the dictionary.</typeparam>
    /// <typeparam name="TSecond">The type of the second values in the dictionary.</typeparam>
    public class ReadOnlyBiDictionary<TFirst, TSecond> : IReadOnlyBiDictionary<TFirst, TSecond>
                                                         where TFirst : notnull
                                                         where TSecond : notnull
    {
        /// <inheritdoc/>
        public bool IsSynced => Count == _secondToFirst.Count;

        /// <inheritdoc/>
        public int Count => _firstToSecond.Count;

        private readonly IReadOnlyDictionary<TFirst, TSecond> _firstToSecond;
        private readonly IReadOnlyDictionary<TSecond, TFirst> _secondToFirst;

        /// <summary>Initializes a new instance of the <see cref="ReadOnlyBiDictionary{TFirst, TSecond}" /> class that contains elements copied from the specified <see cref="IEnumerable{T}" /> and uses the default equality comparer for the key type.</summary>
        /// <param name="collection">The <see cref="IEnumerable{T}" /> whose elements are copied to the new <see cref="ReadOnlyBiDictionary{TFirst, TSecond}" />.</param>
        public ReadOnlyBiDictionary(IEnumerable<KeyValuePair<TFirst, TSecond>> collection)
        {
            _firstToSecond = new ReadOnlyDictionary<TFirst, TSecond>(collection.ToDictionary(k => k.Key, v => v.Value));
            _secondToFirst = new ReadOnlyDictionary<TSecond, TFirst>(collection.ToDictionary(k => k.Value, v => v.Key));
        }

        /// <inheritdoc/>
        public bool Contains(TFirst first)
            => _firstToSecond.ContainsKey(first);

        /// <inheritdoc/>
        public bool Contains(TSecond second)
            => _secondToFirst.ContainsKey(second);

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
        /// Gets the <see cref="IEnumerator{T}"/> of the <see cref="ReadOnlyBiDictionary{TFirst, TSecond}"/>.
        /// </summary>BiDictionary
        /// <returns>The <see cref="IEnumerator{T}"/> of the <see cref="ReadOnlyBiDictionary{TFirst, TSecond}"/></returns>
        public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
            => _firstToSecond.GetEnumerator();

        /// <summary>
        /// Gets the <see cref="IEnumerator"/> of the <see cref="ReadOnlyBiDictionary{TFirst, TSecond}"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/> of the <see cref="ReadOnlyBiDictionary{TFirst, TSecond}"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
            => _firstToSecond.GetEnumerator();
    }
}
