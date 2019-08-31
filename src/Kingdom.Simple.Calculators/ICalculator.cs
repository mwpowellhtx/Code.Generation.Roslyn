namespace Kingdom.Simple.Calculators
{
    /// <summary>
    /// Provides a Simple Calculator.
    /// </summary>
    public interface ICalculator
    {
        /// <summary>
        /// Returns the result of <paramref name="x"/> times <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int Multiply(int x, int y);
    }
}
