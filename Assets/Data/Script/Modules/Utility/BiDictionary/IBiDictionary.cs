namespace Modules.Utility.BidirectionalDict
{
    /// <summary>
    /// Represents a bidirectional collection of value pairs.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first values in the dictionary.</typeparam>
    /// <typeparam name="TSecond">The type of the second values in the dictionary.</typeparam>
    public interface IBiDictionary<TFirst, TSecond> : IReadOnlyBiDictionary<TFirst, TSecond>
                                                      where TFirst : notnull
                                                      where TSecond : notnull
    {
        /// <summary>
        /// Tries to add new value pair to the <see cref="IBiDictionary{TFirst, TSecond}"/>.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The second value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool TryAdd(TFirst first, TSecond second);

        /// <summary>
        /// Tries to add new value pair to the <see cref="IBiDictionary{TFirst, TSecond}"/>. If any of the values already exists, it will be updated.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <param name="second">The second value of the pair</param>
        void AddOrUpdate(TFirst first, TSecond second);

        /// <summary>
        /// Tries to remove a value pair from the <see cref="IBiDictionary{TFirst, TSecond}"/>, by the first value of the pair.
        /// </summary>
        /// <param name="first">The first value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool TryRemove(TFirst first);

        /// <summary>
        /// Tries to remove a value pair from the <see cref="IBiDictionary{TFirst, TSecond}"/>, by the second value of the pair.
        /// </summary>
        /// <param name="second">The second value of the pair</param>
        /// <returns>Returns <see langword="true"/>, if the operation was successful, otherwise returns <see langword="false"/>.</returns>
        bool TryRemove(TSecond second);

        /// <summary>
        /// Clears all value pairs in the <see cref="IBiDictionary{TFirst, TSecond}"/>.
        /// </summary>
        void Clear();
    }
}
