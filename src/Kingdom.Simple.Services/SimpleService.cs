namespace Kingdom.Simple.Services
{
    /// <inheritdoc />
    public class SimpleService : ISimpleService
    {
        /// <inheritdoc />
        public bool IsOdd(int x) => x % 2 == 1;

        /// <inheritdoc />
        public bool IsEvenOrZero(int x) => x % 2 == 0;
    }
}
