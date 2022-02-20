namespace MComponents.MGrid
{
    public interface IMGridFormatterFactoryProvider
    {
        public IMGridObjectFormatter<T> GetFormatter<T>();
    }
}
