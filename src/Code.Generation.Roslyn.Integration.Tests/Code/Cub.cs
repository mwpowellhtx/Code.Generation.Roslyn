namespace Foo
{
    public class Cub : Car
    {
        public Cub()
        {
        }

        internal bool InternalIsDisposed => IsDisposed;
    }
}
