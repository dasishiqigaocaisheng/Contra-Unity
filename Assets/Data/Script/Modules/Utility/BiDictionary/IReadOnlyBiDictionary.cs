using System.Collections.Generic;

namespace Modules.Utility.BidirectionalDict
{
    /// <summary>
    /// Represents a read-only version of a bidirectional collection.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first values in the dictionary.</typeparam>
    /// <typeparam name="TSecond">The type of the second values in the dictionary.</typeparam>
    public interface IReadOnlyBiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>>
                                                              where TFirst : notnull
                                                              where TSecond : notnull
    {
        ///	<summary>Tells if the inner Dictionaries of the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}" /> are still synced.</summary>
        /// <returns>The bool if the inner Dictionaries of the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}" /> are still synced..</returns>
        bool IsSynced { get; }

        ///	<summary>Gets the number of value pairs contained in the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}" />.</summary>
        /// <returns>The number of value pairs contained in the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}" />.</returns>
        int Count { get; }

        /// <summary>
        /// Tells if the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}"/> contains the first value of the value pair.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool Contains(TFirst first);

        /// <summary>
        /// Tells if the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}"/> contains the second value of the value pair.
        /// </summary>
        /// <param name="second">The second value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool Contains(TSecond second);

        /// <summary>
        /// Gets the element by the specified value of the value pair
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <returns>The second value of the pair</returns>
        TSecond this[TFirst first] { get; }

        /// <summary>
        /// Gets the element by the specified value of the value pair
        /// </summary>
        /// <param name="second">The second value of the pair</param>
        /// <returns>The first value of the pair</returns>
        TFirst this[TSecond second] { get; }

        /// <summary>
        /// Tries to get a value of the value pair from the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}"/>, by the first value of the pair.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The first value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool TryGet(TFirst first, out TSecond second);


        /// <summary>
        /// Tries to get a value of the value pair from the <see cref="IReadOnlyBiDictionary{TFirst, TSecond}"/>, by the second value of the pair.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The second value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool TryGet(TSecond second, out TFirst first);
    }
}
