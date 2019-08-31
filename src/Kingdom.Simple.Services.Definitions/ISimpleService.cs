namespace Kingdom.Simple.Services
{
    /// <summary>
    /// Provides a Simple Service.
    /// </summary>
    public interface ISimpleService
    {
        /// <summary>
        /// Returns whether <paramref name="x"/> IsOdd.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        bool IsOdd(int x);

        /// <summary>
        /// Returns whether <paramref name="x"/> IsEvenOrZero.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        bool IsEvenOrZero(int x);
    }
}
